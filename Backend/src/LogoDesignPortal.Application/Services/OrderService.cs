using AutoMapper;
using LogoDesignPortal.Application.DTOs.Orders;
using LogoDesignPortal.Application.DTOs.Users;
using LogoDesignPortal.Application.Exceptions;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Interfaces.Persistence;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace LogoDesignPortal.Application.Services;

public class OrderService : IOrderService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public OrderService(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<OrderResponseDto> CreateOrderAsync(CreateOrderRequestDto request, Guid clientId)
    {
        var client = await _context.ClientProfiles
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.UserId == clientId && !c.IsDeleted);

        if (client == null)
        {
            throw new InvalidOperationException("Client profile not found.");
        }

        var order = _mapper.Map<LogoOrder>(request);
        order.Id = Guid.NewGuid();
        order.ClientId = client.Id;
        order.CreatedAt = DateTime.UtcNow;
        order.CreatedBy = clientId;
        // Priority is already set by AutoMapper with default value

        _context.LogoOrders.Add(order);

        // Create initial status history
        var statusHistory = new OrderStatusHistory
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            PreviousStatus = OrderStatus.WaitingForAdminApproval,
            NewStatus = OrderStatus.WaitingForAdminApproval,
            ChangedBy = clientId,
            CreatedAt = DateTime.UtcNow
        };

        _context.OrderStatusHistories.Add(statusHistory);

        // Create order log
        await CreateOrderLogAsync(
            order.Id,
            OrderAction.Created,
            null,
            OrderStatus.WaitingForAdminApproval,
            "Client",
            clientId,
            "Order created"
        );

        await _context.SaveChangesAsync();

        return await GetOrderByIdAsync(order.Id, clientId, "Client");
    }

    public async Task<OrderResponseDto?> GetOrderByIdAsync(Guid orderId, Guid? userId, string? userRole)
    {
        var order = await _context.LogoOrders
            .Include(o => o.Client)
                .ThenInclude(c => c.User)
            .Include(o => o.Designer)
                .ThenInclude(d => d.User)
            .Include(o => o.Files)
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

        if (order == null)
        {
            return null;
        }

        // Authorization checks
        // Use ForbiddenAccessException for authorization failures (user is authenticated but lacks permission)
        // This will result in 403 Forbidden instead of 401 Unauthorized
        if (userRole == "Client" && order.Client.UserId != userId)
        {
            throw new ForbiddenAccessException("You don't have access to this order.");
        }

        if (userRole == "Designer")
        {
            // For designers, we need to check if the order is assigned to their DesignerProfile
            // order.DesignerId is the DesignerProfile.Id, not the User.Id
            if (order.DesignerId == null)
            {
                // Order not assigned to any designer yet
                throw new ForbiddenAccessException("You don't have access to this order.");
            }

            // Get the designer's profile to compare DesignerProfile.Id
            var designer = await _context.DesignerProfiles
                .FirstOrDefaultAsync(d => d.UserId == userId && !d.IsDeleted);

            if (designer == null || order.DesignerId != designer.Id)
            {
                throw new ForbiddenAccessException("You don't have access to this order.");
            }
        }

        var response = _mapper.Map<OrderResponseDto>(order);
        
        // Mask client info for Admin and Designer
        if (userRole == "Admin" || userRole == "Designer")
        {
            response.Client = new ClientInfoDto
            {
                Id = order.Client.Id,
                CompanyName = order.Client.CompanyName,
                FirstName = order.Client.User.FirstName,
                LastName = order.Client.User.LastName,
                // Email and phone masked
            };
        }

        // Never show client identity to Designer
        if (userRole == "Designer")
        {
            response.Client = null; // Remove client info completely
        }

        return response;
    }

    public async Task<List<OrderResponseDto>> GetOrdersByClientAsync(Guid clientId)
    {
        var client = await _context.ClientProfiles
            .FirstOrDefaultAsync(c => c.UserId == clientId && !c.IsDeleted);

        if (client == null)
        {
            return new List<OrderResponseDto>();
        }

        var query = _context.LogoOrders
            .Include(o => o.Client)
                .ThenInclude(c => c.User)
            .Include(o => o.Designer)
                .ThenInclude(d => d.User)
            .Include(o => o.Files)
            .Where(o => o.ClientId == client.Id && !o.IsDeleted);

        // Filter out archived orders by default
        query = query.Where(o => !o.IsArchived);

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return _mapper.Map<List<OrderResponseDto>>(orders);
    }

    public async Task<List<OrderResponseDto>> GetOrdersByDesignerAsync(Guid designerId)
    {
        var designer = await _context.DesignerProfiles
            .FirstOrDefaultAsync(d => d.UserId == designerId && !d.IsDeleted);

        if (designer == null)
        {
            return new List<OrderResponseDto>();
        }

        var orders = await _context.LogoOrders
            .Include(o => o.Client)
                .ThenInclude(c => c.User)
            .Include(o => o.Designer)
                .ThenInclude(d => d.User)
            .Include(o => o.Files)
            .Where(o => o.DesignerId == designer.Id && !o.IsDeleted)
            .ToListAsync();

        var response = _mapper.Map<List<OrderResponseDto>>(orders);
        
        // Remove client info for designers
        foreach (var order in response)
        {
            order.Client = null;
        }

        return response;
    }

    public async Task<List<OrderResponseDto>> GetAllOrdersAsync(string? userRole)
    {
        var query = _context.LogoOrders
            .Include(o => o.Client)
                .ThenInclude(c => c.User)
            .Include(o => o.Designer)
                .ThenInclude(d => d.User)
            .Include(o => o.Files)
            .Where(o => !o.IsDeleted);

        // Filter out archived orders by default
        query = query.Where(o => !o.IsArchived);

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        var response = _mapper.Map<List<OrderResponseDto>>(orders);

        // Mask client info based on role
        if (userRole == "Admin")
        {
            foreach (var order in response)
            {
                if (order.Client != null)
                {
                    var originalOrder = orders.FirstOrDefault(o => o.Id == order.Id);
                    if (originalOrder != null && originalOrder.Client != null)
                    {
                        order.Client = new ClientInfoDto
                        {
                            Id = originalOrder.Client.Id,
                            CompanyName = originalOrder.Client.CompanyName,
                            FirstName = originalOrder.Client.User.FirstName,
                            LastName = originalOrder.Client.User.LastName,
                            // Email and phone masked
                        };
                    }
                }
            }
        }
        else if (userRole == "Designer")
        {
            foreach (var order in response)
            {
                order.Client = null;
            }
        }

        return response;
    }

    public async Task<OrderResponseDto> AssignOrderToDesignerAsync(Guid orderId, Guid designerId, Guid assignedBy)
    {
        var order = await _context.LogoOrders
            .Include(o => o.Client)
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

        if (order == null)
        {
            throw new InvalidOperationException("Order not found.");
        }

        var designer = await _context.DesignerProfiles
            .FirstOrDefaultAsync(d => d.UserId == designerId && !d.IsDeleted);

        if (designer == null)
        {
            throw new InvalidOperationException("Designer not found.");
        }

        var previousStatus = order.Status;
        order.DesignerId = designer.Id;
        order.Status = OrderStatus.InProgress;
        order.UpdatedAt = DateTime.UtcNow;
        order.UpdatedBy = assignedBy;

        // Create status history
        var statusHistory = new OrderStatusHistory
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            PreviousStatus = previousStatus,
            NewStatus = OrderStatus.InProgress,
            Notes = $"Assigned to designer",
            ChangedBy = assignedBy,
            CreatedAt = DateTime.UtcNow
        };

        _context.OrderStatusHistories.Add(statusHistory);
        await _context.SaveChangesAsync();

        return await GetOrderByIdAsync(orderId, assignedBy, "SuperAdmin");
    }

    public async Task<OrderResponseDto> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusRequestDto request, Guid userId)
    {
        var order = await _context.LogoOrders
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

        if (order == null)
        {
            throw new InvalidOperationException("Order not found.");
        }

        if (!Enum.TryParse<OrderStatus>(request.Status, out var newStatus))
        {
            throw new InvalidOperationException("Invalid status.");
        }

        var previousStatus = order.Status;
        order.Status = newStatus;
        order.UpdatedAt = DateTime.UtcNow;
        order.UpdatedBy = userId;

        // Create status history
        var statusHistory = new OrderStatusHistory
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            PreviousStatus = previousStatus,
            NewStatus = newStatus,
            Notes = request.Notes,
            ChangedBy = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.OrderStatusHistories.Add(statusHistory);
        await _context.SaveChangesAsync();

        return await GetOrderByIdAsync(orderId, userId, null);
    }

    public async Task<OrderResponseDto> RequestPriceApprovalAsync(Guid orderId, RequestPriceApprovalDto request, Guid requestedBy)
    {
        var order = await _context.LogoOrders
            .Include(o => o.Client)
                .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

        if (order == null)
        {
            throw new InvalidOperationException("Order not found.");
        }

        order.ProposedPrice = request.ProposedPrice;
        order.RequiresPriceApproval = true;
        order.PriceApproved = false;
        order.Status = OrderStatus.PriceApprovalPending;
        order.UpdatedAt = DateTime.UtcNow;
        order.UpdatedBy = requestedBy;

        // Create status history
        var statusHistory = new OrderStatusHistory
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            PreviousStatus = order.Status,
            NewStatus = OrderStatus.PriceApprovalPending,
            Notes = $"Price approval requested: ${request.ProposedPrice}. {request.Notes}",
            ChangedBy = requestedBy,
            CreatedAt = DateTime.UtcNow
        };

        _context.OrderStatusHistories.Add(statusHistory);

        // Create notification for client
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = order.Client.UserId,
            OrderId = orderId,
            Title = "Price Approval Required",
            Message = $"Order '{order.Title}' requires price approval. Proposed price: ${request.ProposedPrice}",
            Type = NotificationType.PriceApproval,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        _context.Notifications.Add(notification);

        await _context.SaveChangesAsync();

        return await GetOrderByIdAsync(orderId, requestedBy, "Admin");
    }

    public async Task<OrderResponseDto> ApprovePriceAsync(Guid orderId, ApprovePriceDto request, Guid approvedBy)
    {
        var order = await _context.LogoOrders
            .Include(o => o.Client)
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

        if (order == null)
        {
            throw new InvalidOperationException("Order not found.");
        }

        if (order.Client.UserId != approvedBy)
        {
            throw new UnauthorizedAccessException("Only the client can approve the price.");
        }

        order.PriceApproved = request.Approved;
        order.RequiresPriceApproval = false;

        if (request.Approved)
        {
            order.Price = order.ProposedPrice ?? order.Price;
            order.Status = OrderStatus.WaitingForAdminApproval;
        }
        else
        {
            // If rejected, status remains PriceApprovalPending for admin to adjust
            order.Status = OrderStatus.PriceApprovalPending;
        }

        order.UpdatedAt = DateTime.UtcNow;
        order.UpdatedBy = approvedBy;

        // Create status history
        var statusHistory = new OrderStatusHistory
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            PreviousStatus = OrderStatus.PriceApprovalPending,
            NewStatus = order.Status,
            Notes = request.Approved ? "Price approved by client" : $"Price rejected. Comment: {request.Comment}",
            ChangedBy = approvedBy,
            CreatedAt = DateTime.UtcNow
        };

        _context.OrderStatusHistories.Add(statusHistory);
        await _context.SaveChangesAsync();

        return await GetOrderByIdAsync(orderId, approvedBy, "Client");
    }

    public async Task<OrderResponseDto> ApproveOrderAsync(Guid orderId, Guid approvedBy)
    {
        var order = await _context.LogoOrders
            .Include(o => o.Client)
                .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

        if (order == null)
        {
            throw new InvalidOperationException("Order not found.");
        }

        if (order.Status != OrderStatus.WaitingForAdminApproval)
        {
            throw new InvalidOperationException("Order is not in a state that can be approved.");
        }

        var previousStatus = order.Status;
        order.Status = OrderStatus.InProgress;
        order.UpdatedAt = DateTime.UtcNow;
        order.UpdatedBy = approvedBy;

        // Create status history
        var statusHistory = new OrderStatusHistory
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            PreviousStatus = previousStatus,
            NewStatus = OrderStatus.InProgress,
            Notes = "Order approved by admin",
            ChangedBy = approvedBy,
            CreatedAt = DateTime.UtcNow
        };

        _context.OrderStatusHistories.Add(statusHistory);

        // Create notification for client
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = order.Client.UserId,
            OrderId = orderId,
            Title = "Order Approved",
            Message = $"Your order '{order.Title}' has been approved and is now in progress.",
            Type = NotificationType.OrderStatusChange,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        _context.Notifications.Add(notification);

        await _context.SaveChangesAsync();

        return await GetOrderByIdAsync(orderId, approvedBy, "Admin");
    }

    public async Task<OrderResponseDto> SendFilesToClientAsync(Guid orderId, List<Guid> fileIds, Guid sentBy)
    {
        var order = await _context.LogoOrders
            .Include(o => o.Client)
                .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

        if (order == null)
        {
            throw new InvalidOperationException("Order not found.");
        }

        var files = await _context.LogoFiles
            .Where(f => fileIds.Contains(f.Id) && f.OrderId == orderId && !f.IsDeleted)
            .ToListAsync();

        if (files.Count != fileIds.Count)
        {
            throw new InvalidOperationException("Some files were not found.");
        }

        // Make files visible to client
        foreach (var file in files)
        {
            file.IsVisibleToClient = true;
            file.IsAdminApproved = true;
            file.ApprovedBy = sentBy;
            file.ApprovedAt = DateTime.UtcNow;
            file.UpdatedAt = DateTime.UtcNow;
            file.UpdatedBy = sentBy;
        }

        // Update order status
        var previousStatus = order.Status;
        order.Status = OrderStatus.PreviewDelivered;
        order.UpdatedAt = DateTime.UtcNow;
        order.UpdatedBy = sentBy;

        // Create status history
        var statusHistory = new OrderStatusHistory
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            PreviousStatus = previousStatus,
            NewStatus = OrderStatus.PreviewDelivered,
            Notes = $"Files sent to client for review",
            ChangedBy = sentBy,
            CreatedAt = DateTime.UtcNow
        };

        _context.OrderStatusHistories.Add(statusHistory);

        // Create notification for client
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = order.Client.UserId,
            OrderId = orderId,
            Title = "Preview Files Available",
            Message = $"Preview files for order '{order.Title}' are now available for review.",
            Type = NotificationType.FileUpload,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        _context.Notifications.Add(notification);

        await _context.SaveChangesAsync();

        return await GetOrderByIdAsync(orderId, sentBy, "Admin");
    }

    public async Task<OrderResponseDto> UpdateOrderAsync(Guid orderId, UpdateOrderRequestDto request, Guid userId)
    {
        var order = await _context.LogoOrders
            .Include(o => o.Client)
                .ThenInclude(c => c.User)
            .Include(o => o.Designer)
                .ThenInclude(d => d.User)
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

        if (order == null)
        {
            throw new InvalidOperationException("Order not found.");
        }

        // Check if user is the client who created the order
        var client = await _context.ClientProfiles
            .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsDeleted);

        if (client == null || order.ClientId != client.Id)
        {
            throw new UnauthorizedAccessException("You can only update your own orders.");
        }

        // Only allow updates if order is still waiting for admin approval or price approval pending
        if (order.Status != OrderStatus.WaitingForAdminApproval && 
            order.Status != OrderStatus.PriceApprovalPending)
        {
            throw new InvalidOperationException("Order can only be updated before admin approval or while price approval is pending.");
        }

        // Update order properties
        order.Title = request.Title;
        order.Description = request.Description;
        order.Price = request.Price;
        order.Priority = request.Priority ?? order.Priority;
        order.Deadline = request.Deadline;
        order.Instructions = request.Instructions;
        order.RequiredFormats = request.RequiredFormats;
        order.Requirements = request.Requirements;
        order.ColorPreferences = request.ColorPreferences;
        order.StylePreferences = request.StylePreferences;
        order.UpdatedAt = DateTime.UtcNow;
        order.UpdatedBy = userId;

        await _context.SaveChangesAsync();

        return await GetOrderByIdAsync(orderId, userId, "Client");
    }

    private async Task CreateOrderLogAsync(Guid orderId, OrderAction action, OrderStatus? previousStatus, OrderStatus? newStatus, string performedBy, Guid? performedById, string? note = null, string? metadata = null)
    {
        var orderLog = new OrderLog
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            Action = action,
            PreviousStatus = previousStatus,
            NewStatus = newStatus,
            PerformedBy = performedBy,
            PerformedById = performedById,
            Note = note,
            Metadata = metadata,
            CreatedAt = DateTime.UtcNow
        };

        _context.OrderLogs.Add(orderLog);
    }

    public async Task<OrderResponseDto> CancelOrderAsync(Guid orderId, CancelOrderRequestDto request, Guid userId, string userRole)
    {
        var order = await _context.LogoOrders
            .Include(o => o.Client)
                .ThenInclude(c => c.User)
            .Include(o => o.Designer)
                .ThenInclude(d => d.User)
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

        if (order == null)
        {
            throw new InvalidOperationException("Order not found.");
        }

        var previousStatus = order.Status;
        OrderStatus newStatus;
        bool isCancelledByUser = false;

        // Determine cancellation status based on user role
        if (userRole == "Client")
        {
            // Check if user is the client who created the order
            var client = await _context.ClientProfiles
                .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsDeleted);

            if (client == null || order.ClientId != client.Id)
            {
                throw new UnauthorizedAccessException("You can only cancel your own orders.");
            }

            // Clients can only cancel if status is Pending or Paid (or WaitingForAdminApproval/PriceApprovalPending for backward compatibility)
            if (order.Status != OrderStatus.Pending && 
                order.Status != OrderStatus.Paid &&
                order.Status != OrderStatus.WaitingForAdminApproval &&
                order.Status != OrderStatus.PriceApprovalPending)
            {
                throw new InvalidOperationException("Order can only be cancelled if status is Pending or Paid.");
            }

            // Cannot cancel if processing has started
            if (order.Status == OrderStatus.Processing || order.Status == OrderStatus.InProgress)
            {
                throw new InvalidOperationException("Order cannot be cancelled once processing has started.");
            }

            newStatus = OrderStatus.CancelledByUser;
            isCancelledByUser = true;
        }
        else if (userRole == "Admin" || userRole == "SuperAdmin")
        {
            // Admins can cancel at any stage
            newStatus = OrderStatus.CancelledByAdmin;
        }
        else
        {
            throw new UnauthorizedAccessException("You don't have permission to cancel orders.");
        }

        // Update order
        order.Status = newStatus;
        order.CancellationReason = request.Reason;
        order.CancelledBy = userId;
        order.CancelledAt = DateTime.UtcNow;
        order.IsCancelledByUser = isCancelledByUser;
        order.UpdatedAt = DateTime.UtcNow;
        order.UpdatedBy = userId;

        // Create status history
        var statusHistory = new OrderStatusHistory
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            PreviousStatus = previousStatus,
            NewStatus = newStatus,
            Notes = $"Order cancelled by {userRole}. Reason: {request.Reason}",
            ChangedBy = userId,
            CreatedAt = DateTime.UtcNow
        };
        _context.OrderStatusHistories.Add(statusHistory);

        // Create order log
        await CreateOrderLogAsync(
            orderId,
            OrderAction.Cancelled,
            previousStatus,
            newStatus,
            userRole,
            userId,
            request.Reason,
            JsonSerializer.Serialize(new { isCancelledByUser })
        );

        await _context.SaveChangesAsync();

        return await GetOrderByIdAsync(orderId, userId, userRole);
    }

    public async Task<OrderResponseDto> ArchiveOrderAsync(Guid orderId, ArchiveOrderRequestDto? request, Guid userId)
    {
        var order = await _context.LogoOrders
            .Include(o => o.Client)
                .ThenInclude(c => c.User)
            .Include(o => o.Designer)
                .ThenInclude(d => d.User)
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

        if (order == null)
        {
            throw new InvalidOperationException("Order not found.");
        }

        if (order.IsArchived)
        {
            throw new InvalidOperationException("Order is already archived.");
        }

        var previousStatus = order.Status;

        // Archive order
        order.IsArchived = true;
        order.ArchivedAt = DateTime.UtcNow;
        order.ArchivedBy = userId;
        order.UpdatedAt = DateTime.UtcNow;
        order.UpdatedBy = userId;

        // Create order log
        await CreateOrderLogAsync(
            orderId,
            OrderAction.Archived,
            previousStatus,
            null,
            "Admin",
            userId,
            request?.Note ?? "Order archived"
        );

        await _context.SaveChangesAsync();

        return await GetOrderByIdAsync(orderId, userId, "Admin");
    }

    public async Task<OrderResponseDto> UnarchiveOrderAsync(Guid orderId, Guid userId)
    {
        var order = await _context.LogoOrders
            .Include(o => o.Client)
                .ThenInclude(c => c.User)
            .Include(o => o.Designer)
                .ThenInclude(d => d.User)
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

        if (order == null)
        {
            throw new InvalidOperationException("Order not found.");
        }

        if (!order.IsArchived)
        {
            throw new InvalidOperationException("Order is not archived.");
        }

        // Unarchive order
        order.IsArchived = false;
        order.ArchivedAt = null;
        order.ArchivedBy = null;
        order.UpdatedAt = DateTime.UtcNow;
        order.UpdatedBy = userId;

        // Create order log
        await CreateOrderLogAsync(
            orderId,
            OrderAction.Unarchived,
            null,
            order.Status,
            "Admin",
            userId,
            "Order unarchived"
        );

        await _context.SaveChangesAsync();

        return await GetOrderByIdAsync(orderId, userId, "Admin");
    }

    public async Task<OrderResponseDto> RefundOrderAsync(Guid orderId, RefundOrderRequestDto request, Guid userId)
    {
        var order = await _context.LogoOrders
            .Include(o => o.Client)
                .ThenInclude(c => c.User)
            .Include(o => o.Designer)
                .ThenInclude(d => d.User)
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

        if (order == null)
        {
            throw new InvalidOperationException("Order not found.");
        }

        if (order.IsRefunded)
        {
            throw new InvalidOperationException("Order has already been refunded.");
        }

        if (request.Amount > order.Price)
        {
            throw new InvalidOperationException("Refund amount cannot exceed order price.");
        }

        var previousStatus = order.Status;

        // Process refund
        order.IsRefunded = true;
        order.RefundedAt = DateTime.UtcNow;
        order.RefundedBy = userId;
        order.RefundAmount = request.Amount;
        order.RefundReason = request.Reason;
        order.Status = OrderStatus.Refunded;
        order.UpdatedAt = DateTime.UtcNow;
        order.UpdatedBy = userId;

        // Create status history
        var statusHistory = new OrderStatusHistory
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            PreviousStatus = previousStatus,
            NewStatus = OrderStatus.Refunded,
            Notes = $"Order refunded. Amount: {request.Amount}, Reason: {request.Reason}",
            ChangedBy = userId,
            CreatedAt = DateTime.UtcNow
        };
        _context.OrderStatusHistories.Add(statusHistory);

        // Create order log
        await CreateOrderLogAsync(
            orderId,
            OrderAction.Refunded,
            previousStatus,
            OrderStatus.Refunded,
            "Admin",
            userId,
            request.Reason,
            JsonSerializer.Serialize(new { refundAmount = request.Amount })
        );

        await _context.SaveChangesAsync();

        return await GetOrderByIdAsync(orderId, userId, "Admin");
    }

    public async Task<List<OrderLogResponseDto>> GetOrderLogsAsync(Guid orderId)
    {
        var logs = await _context.OrderLogs
            .Where(l => l.OrderId == orderId && !l.IsDeleted)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();

        return logs.Select(l => new OrderLogResponseDto
        {
            Id = l.Id,
            OrderId = l.OrderId,
            Action = l.Action.ToString(),
            PreviousStatus = l.PreviousStatus?.ToString(),
            NewStatus = l.NewStatus?.ToString(),
            PerformedBy = l.PerformedBy,
            PerformedById = l.PerformedById,
            Note = l.Note,
            Metadata = l.Metadata,
            CreatedAt = l.CreatedAt
        }).ToList();
    }

    // Keep DeleteOrderAsync for backward compatibility but mark as obsolete
    [Obsolete("Use CancelOrderAsync instead. Orders should never be hard-deleted.")]
    public async Task<bool> DeleteOrderAsync(Guid orderId, Guid userId, string? userRole = null)
    {
        // Redirect to cancel for backward compatibility
        var cancelRequest = new CancelOrderRequestDto
        {
            Reason = "Order deleted (legacy method - should use CancelOrderAsync)"
        };
        
        try
        {
            await CancelOrderAsync(orderId, cancelRequest, userId, userRole ?? "Client");
            return true;
        }
        catch
        {
            return false;
        }
    }
}
