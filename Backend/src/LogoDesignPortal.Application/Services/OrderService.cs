using AutoMapper;
using LogoDesignPortal.Application.DTOs.Orders;
using LogoDesignPortal.Application.DTOs.Users;
using LogoDesignPortal.Application.Exceptions;
using LogoDesignPortal.Application.Helpers;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Interfaces.Persistence;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace LogoDesignPortal.Application.Services;

public class OrderService : IOrderService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;
    private readonly IRealtimeEntityUpdateSender _entityUpdateSender;
    private readonly IFileService _fileService;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IApplicationDbContext context, IMapper mapper, INotificationService notificationService, IRealtimeEntityUpdateSender entityUpdateSender, IFileService fileService, ILogger<OrderService> logger)
    {
        _context = context;
        _mapper = mapper;
        _notificationService = notificationService;
        _entityUpdateSender = entityUpdateSender;
        _fileService = fileService;
        _logger = logger;
    }

    public async Task<OrderResponseDto> CreateOrderAsync(CreateOrderRequestDto request, Guid clientId)
    {
        var client = await EnsureClientExistsAsync(clientId);

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

        _logger.LogInformation("OrderCreated. OrderId={OrderId}, UserId={UserId}", order.Id, clientId);

        // Notify all Admin and SuperAdmin users about the new order
        var clientName = $"{client.User.FirstName} {client.User.LastName}".Trim();
        if (string.IsNullOrEmpty(clientName)) clientName = client.CompanyName ?? "A client";
        var orderNumber = NotificationFormatHelper.GetOrderNumber(order.Id);
        var title = "New Order Submitted";
        var message = $"{clientName} placed a new order (#{orderNumber})";

        try
        {
            await _notificationService.CreateNotificationForRoleAsync("Admin", title, message, NotificationType.OrderStatusChange, NotificationReferenceType.Order, order.Id);
            await _notificationService.CreateNotificationForRoleAsync("SuperAdmin", title, message, NotificationType.OrderStatusChange, NotificationReferenceType.Order, order.Id);
            var adminUserIds = await GetAdminAndSuperAdminUserIdsAsync();
            await _entityUpdateSender.SendOrderCreatedAsync(order.Id, adminUserIds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create notifications for new order {OrderId}. Order was created successfully.", order.Id);
        }

        return await GetOrderByIdAsync(order.Id, clientId, "Client");
    }

    public async Task<OrderResponseDto> CreateOrderWithFilesAsync(CreateOrderRequestDto request, IFormFile[] files, Guid clientId, string? description = null)
    {
        if (files == null || files.Length == 0)
            throw new InvalidOperationException("At least one reference file is required for order creation.");

        var client = await EnsureClientExistsAsync(clientId);

        List<LogoFile>? preparedFiles = null;
        LogoOrder? order = null;

        try
        {
            await _context.ExecuteInTransactionAsync(async (ct) =>
            {
                order = _mapper.Map<LogoOrder>(request);
                order.Id = Guid.NewGuid();
                order.ClientId = client.Id;
                order.CreatedAt = DateTime.UtcNow;
                order.CreatedBy = clientId;

                _context.LogoOrders.Add(order);

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

                await CreateOrderLogAsync(order.Id, OrderAction.Created, null, OrderStatus.WaitingForAdminApproval, "Client", clientId, "Order created with files");

                preparedFiles = await _fileService.PrepareReferenceFilesForOrderAsync(order.Id, files, clientId, description);
                foreach (var f in preparedFiles)
                {
                    _context.LogoFiles.Add(f);
                }

                await _context.SaveChangesAsync(ct);
            });
        }
        catch
        {
            if (preparedFiles != null)
            {
                foreach (var f in preparedFiles)
                {
                    try
                    {
                        if (System.IO.File.Exists(f.FilePath))
                            System.IO.File.Delete(f.FilePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete orphaned file {Path} after order creation rollback.", f.FilePath);
                    }
                }
            }
            throw;
        }

        var clientName = $"{client.User.FirstName} {client.User.LastName}".Trim();
        if (string.IsNullOrEmpty(clientName)) clientName = client.CompanyName ?? "A client";
        var orderNumber = NotificationFormatHelper.GetOrderNumber(order.Id);
        var title = "New Order Submitted";
        var message = $"{clientName} placed a new order (#{orderNumber})";

        try
        {
            await _notificationService.CreateNotificationForRoleAsync("Admin", title, message, NotificationType.OrderStatusChange, NotificationReferenceType.Order, order.Id);
            await _notificationService.CreateNotificationForRoleAsync("SuperAdmin", title, message, NotificationType.OrderStatusChange, NotificationReferenceType.Order, order.Id);
            var adminUserIds = await GetAdminAndSuperAdminUserIdsAsync();
            await _entityUpdateSender.SendOrderCreatedAsync(order!.Id, adminUserIds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create notifications for new order {OrderId}. Order was created successfully.", order!.Id);
        }

        return await GetOrderByIdAsync(order!.Id, clientId, "Client");
    }

    private async Task<ClientProfile> EnsureClientExistsAsync(Guid clientId)
    {
        var client = await _context.ClientProfiles
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.UserId == clientId && !c.IsDeleted);

        if (client == null)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == clientId && !u.IsDeleted);

            if (user == null || user.Role?.Name != "Client")
                throw new InvalidOperationException("Client profile not found. Please ensure the client has a company name set in their profile.");

            var deletedProfile = await _context.ClientProfiles
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.UserId == clientId && c.IsDeleted);

            if (deletedProfile != null)
            {
                deletedProfile.IsDeleted = false;
                deletedProfile.DeletedAt = null;
                deletedProfile.DeletedBy = null;
                deletedProfile.CompanyName = string.IsNullOrEmpty(deletedProfile.CompanyName) ? $"{user.FirstName} {user.LastName}".Trim() : deletedProfile.CompanyName;
                if (string.IsNullOrEmpty(deletedProfile.CompanyName)) deletedProfile.CompanyName = "Personal";
                deletedProfile.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                client = deletedProfile;
            }
            else
            {
                client = new ClientProfile
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    CompanyName = $"{user.FirstName} {user.LastName}".Trim(),
                    ContactName = $"{user.FirstName} {user.LastName}".Trim(),
                    CreatedAt = DateTime.UtcNow
                };
                if (string.IsNullOrEmpty(client.CompanyName)) client.CompanyName = "Personal";
                _context.ClientProfiles.Add(client);
                await _context.SaveChangesAsync();
                client = await _context.ClientProfiles.Include(c => c.User).FirstAsync(c => c.Id == client.Id);
            }
        }

        return client;
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

        // Check if order already has an invoice
        response.HasInvoice = await _context.InvoiceOrders
            .AnyAsync(io => io.OrderId == orderId);
        
        // Mask client info for Admin and Designer
        if (userRole == "Admin" || userRole == "Designer")
        {
            response.Client = new ClientInfoDto
            {
                Id = order.Client.Id,
                UserId = order.Client.UserId,
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

        // Mask designer identity from Client: show "Company Design Team" instead
        if (userRole == "Client" && response.Designer != null)
        {
            response.AssignedDesignerDisplayName = "Company Design Team";
            response.Designer = null; // Hide designer name, email, profile, userId
        }
        else if (userRole == "Client" && order.DesignerId.HasValue)
        {
            response.AssignedDesignerDisplayName = "Company Design Team";
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

        var response = _mapper.Map<List<OrderResponseDto>>(orders);

        // Check which orders already have invoices
        var clientOrderIds = orders.Select(o => o.Id).ToList();
        var clientOrderIdsWithInvoices = await _context.InvoiceOrders
            .Where(io => io.OrderId.HasValue && clientOrderIds.Contains(io.OrderId.Value))
            .Select(io => io.OrderId!.Value)
            .Distinct()
            .ToListAsync();

        foreach (var order in response)
        {
            order.HasInvoice = clientOrderIdsWithInvoices.Contains(order.Id);
            // Mask designer identity from Client
            var orig = orders.FirstOrDefault(o => o.Id == order.Id);
            if (order.Designer != null || (orig != null && orig.DesignerId.HasValue))
            {
                order.AssignedDesignerDisplayName = "Company Design Team";
                order.Designer = null;
            }
        }

        return response;
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

        // Check which orders already have invoices
        var orderIds = orders.Select(o => o.Id).ToList();
        var orderIdsWithInvoices = await _context.InvoiceOrders
            .Where(io => io.OrderId.HasValue && orderIds.Contains(io.OrderId.Value))
            .Select(io => io.OrderId!.Value)
            .Distinct()
            .ToListAsync();

        foreach (var order in response)
        {
            order.HasInvoice = orderIdsWithInvoices.Contains(order.Id);
        }

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
                            UserId = originalOrder.Client.UserId,
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

        if (OrderLockingHelper.IsOrderLocked(order.Status))
        {
            throw new InvalidOperationException(OrderLockingHelper.LockedOrderMessage);
        }

        var designer = await _context.DesignerProfiles
            .FirstOrDefaultAsync(d => d.UserId == designerId && !d.IsDeleted);

        if (designer == null)
        {
            throw new InvalidOperationException("Designer not found.");
        }

        var previousStatus = order.Status;
        order.DesignerId = designer.Id;
        // Fixed: Only change status if order is waiting for admin approval
        if (order.Status == OrderStatus.WaitingForAdminApproval)
        {
            order.Status = OrderStatus.InProgress;
        }
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

        _logger.LogInformation("DesignerAssigned. OrderId={OrderId}, AssignedBy={AssignedBy}, DesignerId={DesignerId}", orderId, assignedBy, designerId);

        // Notify designer: Order assigned
        try
        {
            var orderNumber = NotificationFormatHelper.GetOrderNumber(orderId);
            var title = "Order Assigned";
            var message = $"You have been assigned order (#{orderNumber})";
            await _notificationService.CreateNotificationAsync(
                designerId,
                title,
                message,
                NotificationType.OrderStatusChange,
                orderId,
                NotificationReferenceType.Order,
                orderId,
                assignedBy
            );
            await _entityUpdateSender.SendOrderAssignedAsync(orderId, designerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify designer of order assignment {OrderId}.", orderId);
        }

        return await GetOrderByIdAsync(orderId, assignedBy, "SuperAdmin");
    }

    public async Task<OrderResponseDto> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusRequestDto request, Guid userId, string? userRole = null)
    {
        var order = await _context.LogoOrders
            .Include(o => o.Client)
                .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

        if (order == null)
        {
            throw new InvalidOperationException("Order not found.");
        }

        if (OrderLockingHelper.IsOrderLocked(order.Status))
        {
            throw new InvalidOperationException(OrderLockingHelper.LockedOrderMessage);
        }

        if (!Enum.TryParse<OrderStatus>(request.Status, out var newStatus))
        {
            throw new InvalidOperationException("Invalid status.");
        }

        // Role-based status transition validation
        if (!string.IsNullOrEmpty(userRole))
        {
            var allowedStatuses = GetAllowedStatusesForRole(userRole, order.Status, order.Client?.UserId == userId);
            if (!allowedStatuses.Contains(newStatus))
            {
                throw new InvalidOperationException($"You don't have permission to change status to '{newStatus}'. Allowed statuses for {userRole}: {string.Join(", ", allowedStatuses)}");
            }
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

        // Notify client when order status changes (if client exists)
        if (order.Client != null && order.Client.UserId != userId)
        {
            var orderNumber = NotificationFormatHelper.GetOrderNumber(orderId);
            string title;
            string message;
            if (newStatus == OrderStatus.CancelledByAdmin)
            {
                title = "Order Rejected";
                message = $"Your order (#{orderNumber}) was rejected";
            }
            else if (newStatus == OrderStatus.Completed)
            {
                title = "Order Completed";
                message = $"Your order (#{orderNumber}) has been completed";
            }
            else
            {
                title = "Order Status Updated";
                message = $"Order (#{orderNumber}) status updated to {newStatus}.";
            }
            await _notificationService.CreateNotificationAsync(
                order.Client.UserId,
                title,
                message,
                NotificationType.OrderStatusChange,
                orderId,
                NotificationReferenceType.Order,
                orderId
            );
        }

        if (newStatus == OrderStatus.FinalApproved)
        {
            _logger.LogInformation("FinalApproved. OrderId={OrderId}, UserId={UserId}", orderId, userId);
        }

        // When client approves preview (FinalApproved), notify Admin and Designer
        if (newStatus == OrderStatus.FinalApproved && order.Client != null && order.Client.UserId == userId)
        {
            var clientName = $"{order.Client.User?.FirstName} {order.Client.User?.LastName}".Trim();
            if (string.IsNullOrEmpty(clientName)) clientName = "Client";
            var orderNumber = NotificationFormatHelper.GetOrderNumber(orderId);
            var approveTitle = "Preview Approved";
            var approveMessageForAdmin = $"{clientName} approved the preview for order (#{orderNumber})";
            var approveMessageForDesigner = $"Client approved the preview for order (#{orderNumber})"; // Designer must not see client name
            try
            {
                await _notificationService.CreateNotificationForRoleAsync("Admin", approveTitle, approveMessageForAdmin, NotificationType.OrderStatusChange, NotificationReferenceType.Order, orderId);
                await _notificationService.CreateNotificationForRoleAsync("SuperAdmin", approveTitle, approveMessageForAdmin, NotificationType.OrderStatusChange, NotificationReferenceType.Order, orderId);
                if (order.DesignerId.HasValue)
                {
                    var designerUserId = await _context.DesignerProfiles
                        .Where(d => d.Id == order.DesignerId.Value && !d.IsDeleted)
                        .Select(d => d.UserId)
                        .FirstOrDefaultAsync();
                    if (designerUserId != Guid.Empty)
                    {
                        await _notificationService.CreateNotificationAsync(designerUserId, approveTitle, approveMessageForDesigner, NotificationType.OrderStatusChange, orderId, NotificationReferenceType.Order, orderId, userId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create preview approved notifications for order {OrderId}.", orderId);
            }
        }

        // When client requests revision (RevisionRequested), notify Admin and Designer
        if (newStatus == OrderStatus.RevisionRequested && order.Client != null && order.Client.UserId == userId)
        {
            var clientName = $"{order.Client.User?.FirstName} {order.Client.User?.LastName}".Trim();
            if (string.IsNullOrEmpty(clientName)) clientName = "Client";
            var orderNumber = NotificationFormatHelper.GetOrderNumber(orderId);
            var revisionTitle = "Revision Requested";
            var revisionMessageForAdmin = $"{clientName} requested a revision for order (#{orderNumber})";
            var revisionMessageForDesigner = $"Client requested a revision for order (#{orderNumber})"; // Designer must not see client name
            try
            {
                await _notificationService.CreateNotificationForRoleAsync("Admin", revisionTitle, revisionMessageForAdmin, NotificationType.RevisionRequest, NotificationReferenceType.Order, orderId);
                await _notificationService.CreateNotificationForRoleAsync("SuperAdmin", revisionTitle, revisionMessageForAdmin, NotificationType.RevisionRequest, NotificationReferenceType.Order, orderId);
                if (order.DesignerId.HasValue)
                {
                    var designerUserId = await _context.DesignerProfiles
                        .Where(d => d.Id == order.DesignerId.Value && !d.IsDeleted)
                        .Select(d => d.UserId)
                        .FirstOrDefaultAsync();
                    if (designerUserId != Guid.Empty)
                    {
                        await _notificationService.CreateNotificationAsync(designerUserId, revisionTitle, revisionMessageForDesigner, NotificationType.RevisionRequest, orderId, NotificationReferenceType.Order, orderId, userId);
                        var adminUserIds = await GetAdminAndSuperAdminUserIdsAsync();
                        var designerUserIds = new[] { designerUserId };
                        await _entityUpdateSender.SendPreviewRejectedAsync(orderId, clientName, adminUserIds.Concat(designerUserIds));
                    }
                }
                else
                {
                    var adminUserIds = await GetAdminAndSuperAdminUserIdsAsync();
                    await _entityUpdateSender.SendPreviewRejectedAsync(orderId, clientName, adminUserIds);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create preview rejected notifications for order {OrderId}.", orderId);
            }
        }

        // Real-time entity update: notify client and admins so grids refresh
        var statusUpdateUserIds = await GetOrderUpdateRecipientIdsAsync(order);
        await _entityUpdateSender.SendOrderStatusChangedAsync(orderId, newStatus.ToString(), userId, statusUpdateUserIds);

        // When client approves preview (FinalApproved), notify Admin so their grid updates
        if (newStatus == OrderStatus.FinalApproved && order.Client != null && order.Client.UserId == userId)
        {
            var clientName = $"{order.Client.User?.FirstName} {order.Client.User?.LastName}".Trim();
            if (string.IsNullOrEmpty(clientName)) clientName = "Client";
            var adminUserIds = await GetAdminAndSuperAdminUserIdsAsync();
            await _entityUpdateSender.SendPreviewApprovedAsync(orderId, clientName, newStatus.ToString(), adminUserIds);
        }

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
        await _context.SaveChangesAsync();

        // Create notification for client via NotificationService
        var orderNumber = NotificationFormatHelper.GetOrderNumber(orderId);
        await _notificationService.CreateNotificationAsync(
            order.Client.UserId,
            "Price Approval Required",
            $"Order (#{orderNumber}) requires price approval. Proposed price: ${request.ProposedPrice}",
            NotificationType.PriceApproval,
            orderId,
            NotificationReferenceType.Order,
            orderId
        );

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

        // Null check to prevent NullReferenceException
        if (order.Client == null || order.Client.UserId != approvedBy)
        {
            throw new UnauthorizedAccessException("Only the client can approve the price.");
        }

        order.PriceApproved = request.Approved;
        order.RequiresPriceApproval = false;

        if (request.Approved)
        {
            // Fixed: Use ProposedPrice when approved
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
        await _context.SaveChangesAsync();

        // Create notification for client via NotificationService
        var orderNumber = NotificationFormatHelper.GetOrderNumber(orderId);
        await _notificationService.CreateNotificationAsync(
            order.Client.UserId,
            "Order Approved",
            $"Your order (#{orderNumber}) has been approved",
            NotificationType.OrderStatusChange,
            orderId,
            NotificationReferenceType.Order,
            orderId
        );

        return await GetOrderByIdAsync(orderId, approvedBy, "Admin");
    }

    public async Task<OrderResponseDto> SendPreviewBatchToClientAsync(Guid orderId, Guid previewBatchId, Guid sentBy)
    {
        var order = await _context.LogoOrders
            .Include(o => o.Client)
                .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

        if (order == null)
            throw new InvalidOperationException("Order not found.");

        if (OrderLockingHelper.IsOrderLocked(order.Status))
            throw new InvalidOperationException(OrderLockingHelper.LockedOrderMessage);

        var files = await _context.LogoFiles
            .Where(f => f.OrderId == orderId && f.PreviewBatchId == previewBatchId && !f.IsDeleted)
            .ToListAsync();

        if (files.Count == 0)
            throw new InvalidOperationException("No preview files found for this batch. Admin must send entire preview batch.");

        foreach (var f in files)
        {
            if (f.FileType != Domain.Enums.FileType.Preview)
                throw new InvalidOperationException("Only preview files can be sent via preview batch. File type mismatch.");
        }

        return await SendFilesToClientInternalAsync(order, files, sentBy);
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

        if (OrderLockingHelper.IsOrderLocked(order.Status))
        {
            throw new InvalidOperationException(OrderLockingHelper.LockedOrderMessage);
        }

        var files = await _context.LogoFiles
            .Where(f => fileIds.Contains(f.Id) && f.OrderId == orderId && !f.IsDeleted)
            .ToListAsync();

        if (files.Count != fileIds.Count)
        {
            throw new InvalidOperationException("Some files were not found.");
        }

        // Validate: when sending preview files, must send entire batch (no partial delivery)
        var previewFiles = files.Where(f => f.FileType == Domain.Enums.FileType.Preview).ToList();
        if (previewFiles.Count > 0)
        {
            var batchIds = previewFiles.Select(f => f.PreviewBatchId).Where(id => id.HasValue).Distinct().ToList();
            if (batchIds.Count > 1)
                throw new InvalidOperationException("Admin must send entire preview batch. Cannot mix files from different batches.");
            if (batchIds.Count == 1)
            {
                var fullBatchCount = await _context.LogoFiles
                    .CountAsync(f => f.OrderId == orderId && f.PreviewBatchId == batchIds[0] && !f.IsDeleted);
                if (previewFiles.Count != fullBatchCount)
                    throw new InvalidOperationException("Admin must send entire preview batch. Partial delivery is not allowed.");
            }
        }

        return await SendFilesToClientInternalAsync(order, files, sentBy);
    }

    private async Task<OrderResponseDto> SendFilesToClientInternalAsync(LogoOrder order, List<LogoFile> files, Guid sentBy)
    {
        // Set version = delivery round for Preview files (1st batch=1, 2nd batch=2, etc.)
        var previewFiles = files.Where(f => f.FileType == Domain.Enums.FileType.Preview).ToList();
        if (previewFiles.Count > 0)
        {
            var previouslySentBatchCount = await _context.LogoFiles
                .Where(f => f.OrderId == order.Id && f.IsVisibleToClient && f.PreviewBatchId != null && !f.IsDeleted)
                .Where(f => !previewFiles.Select(p => p.PreviewBatchId).Contains(f.PreviewBatchId))
                .Select(f => f.PreviewBatchId)
                .Distinct()
                .CountAsync();
            var deliveryVersion = previouslySentBatchCount + 1;
            foreach (var file in previewFiles)
            {
                file.VersionNumber = deliveryVersion;
            }
        }

        foreach (var file in files)
        {
            file.IsVisibleToClient = true;
            file.IsAdminApproved = true;
            file.ApprovedBy = sentBy;
            file.ApprovedAt = DateTime.UtcNow;
            file.UpdatedAt = DateTime.UtcNow;
            file.UpdatedBy = sentBy;
        }

        var previousStatus = order.Status;
        order.Status = OrderStatus.PreviewDelivered;
        order.UpdatedAt = DateTime.UtcNow;
        order.UpdatedBy = sentBy;

        var statusHistory = new OrderStatusHistory
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            PreviousStatus = previousStatus,
            NewStatus = OrderStatus.PreviewDelivered,
            Notes = "Files sent to client for review",
            ChangedBy = sentBy,
            CreatedAt = DateTime.UtcNow
        };

        _context.OrderStatusHistories.Add(statusHistory);
        await _context.SaveChangesAsync();

        _logger.LogInformation("PreviewForwarded. OrderId={OrderId}, UserId={UserId}, FileCount={FileCount}", order.Id, sentBy, files.Count);

        var orderNumber = NotificationFormatHelper.GetOrderNumber(order.Id);
        await _notificationService.CreateNotificationAsync(
            order.Client!.UserId,
            "Preview Files Available",
            $"Preview files were uploaded for order (#{orderNumber})",
            NotificationType.FileUpload,
            order.Id,
            NotificationReferenceType.Order,
            order.Id
        );

        await _entityUpdateSender.SendPreviewDeliveredAsync(order.Id, OrderStatus.PreviewDelivered.ToString(), order.Client.UserId);

        return await GetOrderByIdAsync(order.Id, sentBy, "Admin");
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

        if (OrderLockingHelper.IsOrderLocked(order.Status))
        {
            throw new InvalidOperationException(OrderLockingHelper.LockedOrderMessage);
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

        if (OrderLockingHelper.IsOrderLocked(order.Status))
        {
            throw new InvalidOperationException(OrderLockingHelper.LockedOrderMessage);
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

        // Notify client when admin cancels order
        if (newStatus == OrderStatus.CancelledByAdmin && order.Client != null)
        {
            try
            {
                var orderNumber = NotificationFormatHelper.GetOrderNumber(orderId);
                await _notificationService.CreateNotificationAsync(
                    order.Client.UserId,
                    "Order Cancelled",
                    $"Your order (#{orderNumber}) was cancelled",
                    NotificationType.OrderStatusChange,
                    orderId,
                    NotificationReferenceType.Order,
                    orderId,
                    userId
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to notify client of order cancellation {OrderId}.", orderId);
            }
        }

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

    public async Task<List<OrderLogResponseDto>> GetOrderLogsWithAccessAsync(Guid orderId, Guid userId, string? userRole)
    {
        // Verify user has access to the order (same rules as GetOrderById)
        var order = await _context.LogoOrders
            .Include(o => o.Client)
            .Include(o => o.Designer)
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

        if (order == null)
        {
            return new List<OrderLogResponseDto>();
        }

        if (userRole == "Client" && order.Client.UserId != userId)
        {
            throw new ForbiddenAccessException("You don't have access to this order.");
        }

        if (userRole == "Designer")
        {
            if (order.DesignerId == null)
            {
                throw new ForbiddenAccessException("You don't have access to this order.");
            }
            var designer = await _context.DesignerProfiles
                .FirstOrDefaultAsync(d => d.UserId == userId && !d.IsDeleted);
            if (designer == null || order.DesignerId != designer.Id)
            {
                throw new ForbiddenAccessException("You don't have access to this order.");
            }
        }

        return await GetOrderLogsAsync(orderId);
    }

    public async Task<OrderResponseDto> SetAllowUploadsAsync(Guid orderId, bool allowUploads, Guid userId)
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
            throw new InvalidOperationException("Order not found.");
        }

        if (OrderLockingHelper.IsOrderLocked(order.Status))
        {
            throw new InvalidOperationException(OrderLockingHelper.LockedOrderMessage);
        }

        order.AllowUploads = allowUploads;
        order.UpdatedAt = DateTime.UtcNow;
        order.UpdatedBy = userId;

        await _context.SaveChangesAsync();

        return await GetOrderByIdAsync(orderId, userId, "Admin");
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

    private List<OrderStatus> GetAllowedStatusesForRole(string userRole, OrderStatus currentStatus, bool isOrderOwner)
    {
        var allowedStatuses = new List<OrderStatus>();

        switch (userRole)
        {
            case "Client":
                // Clients can only:
                // - Request revisions when preview is delivered
                // - Approve final when preview is delivered
                if (currentStatus == OrderStatus.PreviewDelivered)
                {
                    allowedStatuses.Add(OrderStatus.RevisionRequested);
                    allowedStatuses.Add(OrderStatus.FinalApproved);
                }
                // Clients can cancel their own orders in certain statuses
                if (isOrderOwner && (currentStatus == OrderStatus.WaitingForAdminApproval || 
                                     currentStatus == OrderStatus.PriceApprovalPending ||
                                     currentStatus == OrderStatus.Pending))
                {
                    allowedStatuses.Add(OrderStatus.Cancelled);
                }
                break;

            case "Designer":
                // Designers can only:
                // - Mark as PreviewDelivered when uploading files (usually handled by file upload)
                // - Update to PreviewDelivered if in InProgress or RevisionRequested
                if (currentStatus == OrderStatus.InProgress || currentStatus == OrderStatus.RevisionRequested)
                {
                    allowedStatuses.Add(OrderStatus.PreviewDelivered);
                }
                break;

            case "Admin":
            case "SuperAdmin":
                // Admins and SuperAdmins have full control - can set any status
                allowedStatuses.AddRange(Enum.GetValues<OrderStatus>());
                break;

            default:
                // Unknown role - no permissions
                break;
        }

        // Always allow keeping the same status (no change)
        if (!allowedStatuses.Contains(currentStatus))
        {
            allowedStatuses.Add(currentStatus);
        }

        return allowedStatuses;
    }

    /// <summary>
    /// Gets user IDs who should receive real-time order updates (client, admins, designer if assigned).
    /// </summary>
    private async Task<List<Guid>> GetOrderUpdateRecipientIdsAsync(LogoOrder order)
    {
        var userIds = new List<Guid>();

        if (order.Client != null)
        {
            userIds.Add(order.Client.UserId);
        }

        var adminIds = await GetAdminAndSuperAdminUserIdsAsync();
        userIds.AddRange(adminIds);

        if (order.DesignerId.HasValue)
        {
            var designer = await _context.DesignerProfiles
                .Where(d => d.Id == order.DesignerId.Value && !d.IsDeleted)
                .Select(d => d.UserId)
                .FirstOrDefaultAsync();
            if (designer != Guid.Empty)
            {
                userIds.Add(designer);
            }
        }

        return userIds.Distinct().ToList();
    }

    private async Task<List<Guid>> GetAdminAndSuperAdminUserIdsAsync()
    {
        var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
        var superAdminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "SuperAdmin");
        if (adminRole == null && superAdminRole == null)
            return new List<Guid>();

        var roleIds = new List<Guid>();
        if (adminRole != null) roleIds.Add(adminRole.Id);
        if (superAdminRole != null) roleIds.Add(superAdminRole.Id);

        return await _context.Users
            .Where(u => !u.IsDeleted && roleIds.Contains(u.RoleId))
            .Select(u => u.Id)
            .ToListAsync();
    }
}
