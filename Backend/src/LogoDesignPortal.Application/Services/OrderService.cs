using AutoMapper;
using LogoDesignPortal.Application.DTOs.Orders;
using LogoDesignPortal.Application.DTOs.Users;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Interfaces.Persistence;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;

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

        _context.LogoOrders.Add(order);

        // Create initial status history
        var statusHistory = new OrderStatusHistory
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            PreviousStatus = OrderStatus.Pending,
            NewStatus = OrderStatus.Pending,
            ChangedBy = clientId,
            CreatedAt = DateTime.UtcNow
        };

        _context.OrderStatusHistories.Add(statusHistory);
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
        if (userRole == "Client" && order.Client.UserId != userId)
        {
            throw new UnauthorizedAccessException("You don't have access to this order.");
        }

        if (userRole == "Designer" && order.DesignerId != userId)
        {
            throw new UnauthorizedAccessException("You don't have access to this order.");
        }

        var response = _mapper.Map<OrderResponseDto>(order);
        
        // Mask client info for Admin and Designer
        if (userRole == "Admin" || userRole == "Designer")
        {
            response.Client = new ClientInfoDto
            {
                Id = order.Client.Id,
                CompanyName = order.Client.CompanyName,
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

        var orders = await _context.LogoOrders
            .Include(o => o.Client)
                .ThenInclude(c => c.User)
            .Include(o => o.Designer)
                .ThenInclude(d => d.User)
            .Include(o => o.Files)
            .Where(o => o.ClientId == client.Id && !o.IsDeleted)
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
        var orders = await _context.LogoOrders
            .Include(o => o.Client)
                .ThenInclude(c => c.User)
            .Include(o => o.Designer)
                .ThenInclude(d => d.User)
            .Include(o => o.Files)
            .Where(o => !o.IsDeleted)
            .ToListAsync();

        var response = _mapper.Map<List<OrderResponseDto>>(orders);

        // Mask client info based on role
        if (userRole == "Admin")
        {
            foreach (var order in response)
            {
                order.Client = new ClientInfoDto
                {
                    Id = order.Client!.Id,
                    CompanyName = order.Client.CompanyName,
                    // Email and phone masked
                };
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
}
