using LogoDesignPortal.Application.Configuration;
using LogoDesignPortal.Application.DTOs.DesignerPayout;
using LogoDesignPortal.Application.Exceptions;
using LogoDesignPortal.Application.Helpers;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Interfaces.Persistence;
using LogoDesignPortal.Domain;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LogoDesignPortal.Application.Services;

public class DesignerPayoutService : IDesignerPayoutService
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly IRealtimeEntityUpdateSender _entityUpdateSender;
    private readonly ICommentService _commentService;
    private readonly ProductionSafetyOptions _safetyOptions;
    private readonly ILogger<DesignerPayoutService> _logger;

    public DesignerPayoutService(
        IApplicationDbContext context,
        INotificationService notificationService,
        IRealtimeEntityUpdateSender entityUpdateSender,
        ICommentService commentService,
        IOptions<ProductionSafetyOptions> safetyOptions,
        ILogger<DesignerPayoutService> logger)
    {
        _context = context;
        _notificationService = notificationService;
        _entityUpdateSender = entityUpdateSender;
        _commentService = commentService;
        _safetyOptions = safetyOptions?.Value ?? new ProductionSafetyOptions();
        _logger = logger;
    }

    public async Task<List<DesignerPricingInfoDto>> GetDesignPricingInfoAsync(Guid? designerId = null)
    {
        // 1. Try designer-specific pricing first (DesignerLogoPricing)
        if (designerId.HasValue)
        {
            var designerPricings = await _context.DesignerLogoPricings
                .Where(p => p.DesignerId == designerId.Value && p.IsActive && !p.IsDeleted)
                .OrderBy(p => p.DesignCategory)
                .ThenBy(p => p.DesignType)
                .ToListAsync();

            if (designerPricings.Count > 0)
            {
                return designerPricings.Select(p => new DesignerPricingInfoDto
                {
                    DesignCategory = p.DesignCategory.ToString(),
                    DesignType = p.DesignType.ToString(),
                    DefaultPrice = p.DefaultPrice > 0 ? p.DefaultPrice : null,
                    RequiresCustomPrice = p.DefaultPrice <= 0
                }).ToList();
            }
        }

        // 2. Fall back to global DesignPricing
        var pricings = await _context.DesignPricings
            .Where(p => p.IsActive && !p.IsDeleted)
            .OrderBy(p => p.DesignCategory)
            .ThenBy(p => p.DesignType)
            .ToListAsync();

        if (pricings.Count == 0)
        {
            // Same flow as client pricing: SuperAdmin sets DesignPricing or default 0.
            return new List<DesignerPricingInfoDto>
            {
                new() { DesignCategory = "EmbroideryDigitizing", DesignType = "LeftChest", DefaultPrice = 0m, RequiresCustomPrice = true },
                new() { DesignCategory = "EmbroideryDigitizing", DesignType = "JacketBack", DefaultPrice = 0m, RequiresCustomPrice = true },
                new() { DesignCategory = "VectorScreenPrinting", DesignType = "SimpleVector", DefaultPrice = 0m, RequiresCustomPrice = true },
                new() { DesignCategory = "VectorScreenPrinting", DesignType = "ComplexVector", DefaultPrice = null, RequiresCustomPrice = true },
                new() { DesignCategory = "CustomPatch", DesignType = "LeftChest", DefaultPrice = 0m, RequiresCustomPrice = true },
                new() { DesignCategory = "CustomPatch", DesignType = "JacketBack", DefaultPrice = 0m, RequiresCustomPrice = true }
            };
        }

        return pricings.Select(p => new DesignerPricingInfoDto
        {
            DesignCategory = p.DesignCategory.ToString(),
            DesignType = p.DesignType.ToString(),
            DefaultPrice = p.DefaultPrice > 0 ? p.DefaultPrice : null,
            RequiresCustomPrice = p.DefaultPrice <= 0
        }).ToList();
    }

    public async Task SubmitDesignerPricingAsync(Guid orderId, SubmitDesignerPricingRequestDto request, Guid designerUserId)
    {
        var order = await _context.LogoOrders
            .Include(o => o.Designer)
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

        if (order == null)
            throw new InvalidOperationException("Order not found.");

        var designer = await _context.DesignerProfiles.FirstOrDefaultAsync(d => d.UserId == designerUserId && !d.IsDeleted);
        if (designer == null || order.DesignerId != designer.Id)
            throw new ForbiddenAccessException("You don't have access to submit pricing for this order.");

        if (OrderLockingHelper.IsOrderLocked(order.Status))
            throw new InvalidOperationException(OrderLockingHelper.LockedOrderMessage);

        if (DesignerPayoutPricingRules.HasFinalizedDesignerPayout(order))
            throw new InvalidOperationException("Price has already been approved. You cannot change it.");

        if (request.ProposedPrice < 0)
            throw new InvalidOperationException("Proposed price cannot be negative.");
        // ComplexVector requires ProposedPrice > 0
        if (request.DesignType == DesignType.ComplexVector && request.ProposedPrice <= 0)
            throw new InvalidOperationException("Complex Vector requires a proposed price greater than zero.");

        // Get standard price: DesignerLogoPricing first, then DesignPricing (global)
        var standardPrice = await GetStandardPriceFromTableAsync(request.DesignCategory, request.DesignType, designer.Id);
        order.DesignCategory = request.DesignCategory;
        order.DesignType = request.DesignType;
        order.StandardPrice = standardPrice;
        order.ProposedPrice = request.ProposedPrice;
        order.DesignerProposedPrice = request.ProposedPrice;

        var differs = standardPrice == null || request.ProposedPrice != standardPrice.Value;
        order.RequiresPriceApproval = differs;
        order.PriceApprovalStatus = differs ? PriceApprovalStatus.PendingApproval : PriceApprovalStatus.AutoApproved;

        if (!differs)
        {
            order.ApprovedPrice = request.ProposedPrice;
            order.DesignerApprovedPrice = request.ProposedPrice;
        }
        else
        {
            order.ApprovedPrice = null;
            order.DesignerApprovedPrice = null;
        }

        order.UpdatedAt = DateTime.UtcNow;
        order.UpdatedBy = designerUserId;

        if (differs)
        {
            TransitionToPriceApprovalPendingForDesignerIfNeeded(order, designerUserId);
        }

        await _context.SaveChangesAsync();

        // When price requires admin approval, notify Admin/SuperAdmin
        if (differs)
        {
            var submitLine = $"Proposed designer payout: PKR {request.ProposedPrice:N0}.";
            if (!string.IsNullOrWhiteSpace(request.Reason))
                submitLine += $" Note: {request.Reason.Trim()}";
            await _commentService.AppendPriceNegotiationNoteAsync(orderId, designerUserId, CommentType.PriceNegotiationDesigner, submitLine);

            try
            {
                var orderNumber = NotificationFormatHelper.GetOrderNumber(orderId);
                var title = "Designer Price Approval Needed";
                var message = $"Designer proposed PKR {request.ProposedPrice:N0} for order (#{orderNumber}). Approve to add to designer invoice.";
                await _notificationService.CreateNotificationForRoleAsync("Admin", title, message, NotificationType.Info, NotificationReferenceType.Order, orderId, designerUserId);
                await _notificationService.CreateNotificationForRoleAsync("SuperAdmin", title, message, NotificationType.Info, NotificationReferenceType.Order, orderId, designerUserId);
            }
            catch
            {
                // Notification failure must not affect pricing submission
            }
        }
    }

    public async Task ProposeDesignerPriceAsync(Guid orderId, ProposeDesignerPriceRequestDto request, Guid designerUserId)
    {
        var order = await _context.LogoOrders
            .Include(o => o.Designer)
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

        if (order == null)
            throw new InvalidOperationException("Order not found.");

        var designer = await _context.DesignerProfiles.FirstOrDefaultAsync(d => d.UserId == designerUserId && !d.IsDeleted);
        if (designer == null || order.DesignerId != designer.Id)
            throw new ForbiddenAccessException("You don't have access to propose pricing for this order.");

        if (OrderLockingHelper.IsOrderLocked(order.Status))
            throw new InvalidOperationException(OrderLockingHelper.LockedOrderMessage);

        if (DesignerPayoutPricingRules.HasFinalizedDesignerPayout(order))
            throw new InvalidOperationException("Price has already been approved. You cannot change it.");

        if (request.ProposedPrice <= 0)
            throw new InvalidOperationException("Proposed price must be greater than zero.");

        order.DesignerProposedPrice = request.ProposedPrice;
        order.ProposedPrice = request.ProposedPrice;
        order.RequiresPriceApproval = true;
        order.PriceApprovalStatus = PriceApprovalStatus.PendingApproval;
        order.ApprovedPrice = null;
        order.DesignerApprovedPrice = null;
        order.UpdatedAt = DateTime.UtcNow;
        order.UpdatedBy = designerUserId;

        TransitionToPriceApprovalPendingForDesignerIfNeeded(order, designerUserId);

        await _context.SaveChangesAsync();

        var proposeLine = $"Proposed designer payout: PKR {request.ProposedPrice:N0}.";
        if (!string.IsNullOrWhiteSpace(request.Message))
            proposeLine += $" {request.Message.Trim()}";
        await _commentService.AppendPriceNegotiationNoteAsync(orderId, designerUserId, CommentType.PriceNegotiationDesigner, proposeLine);

        try
        {
            var orderNumber = NotificationFormatHelper.GetOrderNumber(orderId);
            var title = "Designer Price Approval Needed";
            var message = $"Designer proposed PKR {request.ProposedPrice:N0} for order (#{orderNumber}). Approve to add to designer invoice.";
            await _notificationService.CreateNotificationForRoleAsync("Admin", title, message, NotificationType.Info, NotificationReferenceType.Order, orderId, designerUserId);
            await _notificationService.CreateNotificationForRoleAsync("SuperAdmin", title, message, NotificationType.Info, NotificationReferenceType.Order, orderId, designerUserId);
        }
        catch
        {
            // Notification failure must not affect pricing submission
        }
    }

    /// <summary>
    /// Order stays in PriceApprovalPending (not In Progress) while admin must approve a designer payout price.
    /// If already PriceApprovalPending (e.g. client price request), status is unchanged.
    /// </summary>
    private void TransitionToPriceApprovalPendingForDesignerIfNeeded(LogoOrder order, Guid userId)
    {
        if (order.Status == OrderStatus.PriceApprovalPending)
            return;

        if (order.Status != OrderStatus.InProgress && order.Status != OrderStatus.RevisionRequested)
            return;

        var previousStatus = order.Status;
        OrderStatusStateMachine.ValidateTransition(previousStatus, OrderStatus.PriceApprovalPending);
        var now = DateTime.UtcNow;
        order.Status = OrderStatus.PriceApprovalPending;
        order.UpdatedAt = now;
        order.UpdatedBy = userId;

        _context.OrderStatusHistories.Add(new OrderStatusHistory
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            PreviousStatus = previousStatus,
            NewStatus = OrderStatus.PriceApprovalPending,
            Notes = "Awaiting price approval (designer payout)",
            ChangedBy = userId,
            CreatedAt = now
        });
    }

    private async Task<decimal?> GetStandardPriceFromTableAsync(DesignCategory category, DesignType designType, Guid? designerId = null)
    {
        // 1. Try designer-specific pricing first
        if (designerId.HasValue)
        {
            var designerPricing = await _context.DesignerLogoPricings
                .FirstOrDefaultAsync(p => p.DesignerId == designerId.Value && p.DesignCategory == category && p.DesignType == designType && p.IsActive && !p.IsDeleted);
            if (designerPricing != null && designerPricing.DefaultPrice > 0)
                return designerPricing.DefaultPrice;
        }

        // 2. Fall back to global DesignPricing
        var pricing = await _context.DesignPricings
            .FirstOrDefaultAsync(p => p.DesignCategory == category && p.DesignType == designType && p.IsActive && !p.IsDeleted);
        if (pricing == null || pricing.DefaultPrice <= 0)
            return null;
        return pricing.DefaultPrice;
    }

    public async Task ApproveDesignerPriceAsync(Guid orderId, ApproveDesignerPriceRequestDto request, Guid adminUserId)
    {
        var order = await _context.LogoOrders
            .Include(o => o.Client)
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

        if (order == null)
            throw new InvalidOperationException("Order not found.");

        if (order.PriceApprovalStatus != PriceApprovalStatus.PendingApproval && order.PriceApprovalStatus != PriceApprovalStatus.Modified)
            throw new InvalidOperationException("This order does not have a price pending approval.");

        var previousOrderStatus = order.Status;
        var previousDesignerPriceStatus = order.PriceApprovalStatus;

        var proposedPrice = order.DesignerProposedPrice ?? order.ProposedPrice;
        switch (request.Action)
        {
            case DesignerPriceApprovalAction.Approve:
                if (!proposedPrice.HasValue)
                    throw new InvalidOperationException("No proposed price to approve.");
                order.ApprovedPrice = proposedPrice.Value;
                order.DesignerApprovedPrice = proposedPrice.Value;
                order.PriceApprovalStatus = PriceApprovalStatus.Approved;
                break;

            case DesignerPriceApprovalAction.Modify:
                if (!request.ApprovedPrice.HasValue || request.ApprovedPrice.Value <= 0)
                    throw new InvalidOperationException("Approved price is required when modifying.");
                order.ApprovedPrice = request.ApprovedPrice.Value;
                order.DesignerApprovedPrice = request.ApprovedPrice.Value;
                order.PriceApprovalStatus = PriceApprovalStatus.Approved;
                break;

            case DesignerPriceApprovalAction.Reject:
                order.PriceApprovalStatus = PriceApprovalStatus.Rejected;
                order.PriceApproved = false;
                order.ApprovedPrice = null;
                order.DesignerApprovedPrice = null;
                break;

            default:
                throw new InvalidOperationException("Invalid approval action.");
        }

        order.RequiresPriceApproval = false;
        order.UpdatedAt = DateTime.UtcNow;
        order.UpdatedBy = adminUserId;

        RestoreOrderStatusAfterDesignerPriceDecision(order, request.Action, previousOrderStatus, previousDesignerPriceStatus, adminUserId);

        await _context.SaveChangesAsync();

        var payoutLine = request.Action switch
        {
            DesignerPriceApprovalAction.Approve when proposedPrice.HasValue =>
                $"Approved designer payout at PKR {proposedPrice.Value:N0}.",
            DesignerPriceApprovalAction.Modify when request.ApprovedPrice.HasValue =>
                $"Set designer payout to PKR {request.ApprovedPrice.Value:N0} (modified).",
            DesignerPriceApprovalAction.Reject => "Rejected the proposed designer payout.",
            _ => "Updated designer payout approval."
        };
        if (!string.IsNullOrWhiteSpace(request.Message))
            payoutLine += $" Message: {request.Message.Trim()}";
        await _commentService.AppendPriceNegotiationNoteAsync(orderId, adminUserId, CommentType.PriceNegotiationDesigner, payoutLine);

        try
        {
            if (order.DesignerId.HasValue)
            {
                var designerUserId = await _context.DesignerProfiles
                    .Where(d => d.Id == order.DesignerId.Value && !d.IsDeleted)
                    .Select(d => d.UserId)
                    .FirstOrDefaultAsync();

                if (designerUserId != Guid.Empty)
                {
                    var orderNumber = NotificationFormatHelper.GetOrderNumber(orderId);
                    var designerTitle = request.Action switch
                    {
                        DesignerPriceApprovalAction.Approve => "Designer Price Approved",
                        DesignerPriceApprovalAction.Modify => "Designer Price Updated",
                        DesignerPriceApprovalAction.Reject => "Designer Price Rejected",
                        _ => "Designer Price Decision"
                    };
                    var designerMessage = request.Action switch
                    {
                        DesignerPriceApprovalAction.Approve when proposedPrice.HasValue =>
                            $"Your proposed payout for order (#{orderNumber}) was approved at PKR {proposedPrice.Value:N0}.",
                        DesignerPriceApprovalAction.Modify when request.ApprovedPrice.HasValue =>
                            $"Your proposed payout for order (#{orderNumber}) was updated to PKR {request.ApprovedPrice.Value:N0}.",
                        DesignerPriceApprovalAction.Reject =>
                            $"Your proposed payout for order (#{orderNumber}) was rejected. Please review and propose again if needed.",
                        _ => $"A designer payout decision was recorded for order (#{orderNumber})."
                    };
                    if (!string.IsNullOrWhiteSpace(request.Message))
                        designerMessage += $" Note: {request.Message.Trim()}";

                    await _notificationService.CreateNotificationForUserAsync(
                        designerUserId,
                        designerTitle,
                        designerMessage,
                        NotificationType.Info,
                        NotificationReferenceType.Order,
                        orderId,
                        adminUserId
                    );
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to notify designer about payout decision for order {OrderId}", orderId);
        }

        try
        {
            var recipientIds = await GetOrderUpdateRecipientIdsAsync(order);
            await _entityUpdateSender.SendOrderStatusChangedAsync(orderId, order.Status.ToString(), adminUserId, recipientIds);
            await _entityUpdateSender.SendOrderUpdatedAsync(orderId, order.Status.ToString(), recipientIds);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send realtime order updates for designer payout decision on order {OrderId}", orderId);
        }
    }

    /// <summary>
    /// Gets users who should see order grid updates (client, admins/superadmins, assigned designer).
    /// </summary>
    private async Task<List<Guid>> GetOrderUpdateRecipientIdsAsync(LogoOrder order)
    {
        var userIds = new List<Guid>();

        if (order.Client != null)
            userIds.Add(order.Client.UserId);

        userIds.AddRange(await GetAdminAndSuperAdminUserIdsAsync());

        if (order.DesignerId.HasValue)
        {
            var designerUserId = await _context.DesignerProfiles
                .Where(d => d.Id == order.DesignerId.Value && !d.IsDeleted)
                .Select(d => d.UserId)
                .FirstOrDefaultAsync();
            if (designerUserId != Guid.Empty)
                userIds.Add(designerUserId);
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

    /// <summary>
    /// After admin resolves designer payout pricing, return to In Progress unless the client still owes approval
    /// on a different proposed charge (ClientPrice vs ClientChargePrice).
    /// </summary>
    private void RestoreOrderStatusAfterDesignerPriceDecision(
        LogoOrder order,
        DesignerPriceApprovalAction action,
        OrderStatus previousOrderStatus,
        PriceApprovalStatus previousDesignerPriceStatus,
        Guid adminUserId)
    {
        if (previousOrderStatus != OrderStatus.PriceApprovalPending)
            return;
        if (previousDesignerPriceStatus != PriceApprovalStatus.PendingApproval &&
            previousDesignerPriceStatus != PriceApprovalStatus.Modified)
            return;

        var clientPriceDiffersFromCharge = order.ClientPrice.HasValue &&
            Math.Abs(order.ClientChargePrice - order.ClientPrice.Value) > 0.01m;

        if (clientPriceDiffersFromCharge)
            return;

        var now = DateTime.UtcNow;
        OrderStatusStateMachine.ValidateTransition(OrderStatus.PriceApprovalPending, OrderStatus.InProgress);
        order.Status = OrderStatus.InProgress;
        order.UpdatedAt = now;
        order.UpdatedBy = adminUserId;

        var note = action switch
        {
            DesignerPriceApprovalAction.Approve => "Designer payout price approved — work continues",
            DesignerPriceApprovalAction.Modify => "Designer payout price set by admin — work continues",
            DesignerPriceApprovalAction.Reject => "Designer payout price rejected — work continues",
            _ => "Designer price decision recorded"
        };

        _context.OrderStatusHistories.Add(new OrderStatusHistory
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            PreviousStatus = OrderStatus.PriceApprovalPending,
            NewStatus = OrderStatus.InProgress,
            Notes = note,
            ChangedBy = adminUserId,
            CreatedAt = now
        });
    }

    public async Task<List<OrderPricingSummaryDto>> GetOrdersPendingPriceApprovalAsync()
    {
        var orders = await _context.LogoOrders
            .Include(o => o.Designer)
                .ThenInclude(d => d!.User)
            .Where(o => !o.IsDeleted &&
                       o.PriceApprovalStatus == PriceApprovalStatus.PendingApproval &&
                       o.DesignerId != null)
            .OrderByDescending(o => o.UpdatedAt)
            .ToListAsync();

        return orders.Select(o => new OrderPricingSummaryDto
        {
            OrderId = o.Id,
            OrderTitle = o.Title,
            DesignCategory = o.DesignCategory?.ToString(),
            DesignType = o.DesignType?.ToString(),
            StandardPrice = o.StandardPrice,
            ProposedPrice = o.ProposedPrice,
            ApprovedPrice = o.ApprovedPrice,
            PriceApprovalStatus = o.PriceApprovalStatus.ToString(),
            PriceApproved = DesignerPayoutPricingRules.HasFinalizedDesignerPayout(o),
            CompletedDate = o.CompletedDate
        }).ToList();
    }

    public async Task<DesignerInvoiceResponseDto> GenerateDesignerInvoiceAsync(Guid designerId, int year, int month, Guid adminUserId)
    {
        if (_safetyOptions.DisableDesignerPayout)
            throw new InvalidOperationException("Designer payout temporarily disabled by administrator.");

        var designer = await _context.DesignerProfiles
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Id == designerId && !d.IsDeleted);

        if (designer == null)
            throw new InvalidOperationException("Designer not found.");

        var startDate = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = startDate.AddMonths(1);

        var eligibleOrders = await _context.LogoOrders
            .Where(o => o.DesignerId == designerId &&
                       !o.IsDeleted &&
                       o.Status == OrderStatus.Completed &&
                       !o.IsDesignerInvoiced &&
                       o.CompletedDate >= startDate &&
                       o.CompletedDate < endDate)
            .Where(DesignerPayoutPricingRules.EligibleForDesignerInvoiceExpression)
            .OrderBy(o => o.CompletedDate)
            .ToListAsync();

        if (eligibleOrders.Count == 0)
            throw new InvalidOperationException($"No completed orders with approved prices found for designer in {startDate:MMMM yyyy}.");

        var invoiceNumber = $"DINV-{year}{month:D2}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
        var billingPeriod = startDate.ToString("MMMM yyyy");
        var invoiceId = Guid.NewGuid();

        // Atomic transaction: create invoice, add items, update orders - all or nothing
        await _context.ExecuteInTransactionAsync(async (ct) =>
        {
            var invoice = new DesignerInvoice
            {
                Id = invoiceId,
                DesignerId = designerId,
                InvoiceNumber = invoiceNumber,
                TotalAmount = 0,
                Status = DesignerInvoiceStatus.Pending,
                BillingPeriod = billingPeriod,
                IssueDate = DateTime.UtcNow,
                CreatedBy = adminUserId
            };
            _context.DesignerInvoices.Add(invoice);

            decimal totalAmount = 0;
            foreach (var order in eligibleOrders)
            {
                // Financial safety guard: validate order status and amounts
                if (order.Status != OrderStatus.Completed)
                    continue;
                var approvedAmount = order.DesignerApprovedPrice ?? order.ApprovedPrice ?? 0;
                if (approvedAmount <= 0)
                    continue;

                // Re-check: skip if already invoiced (race condition guard)
                var freshOrder = await _context.LogoOrders
                    .AsNoTracking()
                    .FirstOrDefaultAsync(o => o.Id == order.Id && !o.IsDeleted, ct);
                if (freshOrder == null || freshOrder.IsDesignerInvoiced)
                    continue;

                // Defensive: ensure no duplicate orderId in this invoice (unique constraint per invoice)
                var existingItem = await _context.DesignerInvoiceItems
                    .AnyAsync(i => i.DesignerInvoiceId == invoiceId && i.OrderId == order.Id && !i.IsDeleted, ct);
                if (existingItem)
                    continue;

                var item = new DesignerInvoiceItem
                {
                    Id = Guid.NewGuid(),
                    DesignerInvoiceId = invoiceId,
                    OrderId = order.Id,
                    Description = $"Order: {order.Title}",
                    Amount = approvedAmount,
                    CreatedBy = adminUserId
                };
                _context.DesignerInvoiceItems.Add(item);

                var orderToUpdate = await _context.LogoOrders.FirstOrDefaultAsync(o => o.Id == order.Id, ct);
                if (orderToUpdate != null)
                {
                    orderToUpdate.IsDesignerInvoiced = true;
                    orderToUpdate.DesignerInvoiceId = invoiceId;
                }

                totalAmount += approvedAmount;
            }

            invoice.TotalAmount = totalAmount;
            await _context.SaveChangesAsync(ct);
        });

        var itemsCount = await _context.DesignerInvoiceItems.CountAsync(i => i.DesignerInvoiceId == invoiceId && !i.IsDeleted);
        if (itemsCount == 0)
            throw new InvalidOperationException($"All eligible orders were already invoiced. No new invoice items created for designer in {startDate:MMMM yyyy}.");

        _logger.LogInformation("DesignerPayoutGenerated. InvoiceId={InvoiceId}, DesignerId={DesignerId}, UserId={UserId}, Timestamp={Timestamp}",
            invoiceId, designerId, adminUserId, DateTime.UtcNow);

        return await GetDesignerInvoiceByIdAsync(invoiceId) ?? throw new InvalidOperationException("Failed to retrieve created invoice.");
    }

    public async Task<List<DesignerInvoiceResponseDto>> GetDesignerInvoicesAsync(Guid designerId)
    {
        var invoices = await _context.DesignerInvoices
            .Include(i => i.Designer)
                .ThenInclude(d => d.User)
            .Include(i => i.Items)
                .ThenInclude(item => item.Order)
            .Include(i => i.Adjustments)
            .Where(i => i.DesignerId == designerId && !i.IsDeleted)
            .OrderByDescending(i => i.IssueDate)
            .ToListAsync();

        return invoices.Select(MapToDto).ToList();
    }

    public async Task<DesignerInvoiceResponseDto?> GetDesignerInvoiceByIdAsync(Guid invoiceId)
    {
        var invoice = await _context.DesignerInvoices
            .Include(i => i.Designer)
                .ThenInclude(d => d.User)
            .Include(i => i.Items)
                .ThenInclude(item => item.Order)
            .Include(i => i.Adjustments)
            .FirstOrDefaultAsync(i => i.Id == invoiceId && !i.IsDeleted);

        return invoice == null ? null : MapToDto(invoice);
    }

    public async Task MarkDesignerInvoicePaidAsync(Guid invoiceId, Guid adminUserId)
    {
        var invoice = await _context.DesignerInvoices
            .FirstOrDefaultAsync(i => i.Id == invoiceId && !i.IsDeleted);

        if (invoice == null)
            throw new InvalidOperationException("Designer invoice not found.");

        invoice.Status = DesignerInvoiceStatus.Paid;
        invoice.PaidDate = DateTime.UtcNow;
        invoice.UpdatedAt = DateTime.UtcNow;
        invoice.UpdatedBy = adminUserId;

        await _context.SaveChangesAsync();
    }

    public async Task<List<OrderPricingSummaryDto>> GetDesignerPayoutEligibleOrdersAsync(Guid designerId)
    {
        var orders = await _context.LogoOrders
            .Where(o => o.DesignerId == designerId &&
                       !o.IsDeleted &&
                       o.Status == OrderStatus.Completed &&
                       !o.IsDesignerInvoiced)
            .Where(DesignerPayoutPricingRules.EligibleForDesignerInvoiceExpression)
            .OrderByDescending(o => o.CompletedDate)
            .ToListAsync();

        return orders.Select(o => new OrderPricingSummaryDto
        {
            OrderId = o.Id,
            OrderTitle = o.Title,
            DesignCategory = o.DesignCategory?.ToString(),
            DesignType = o.DesignType?.ToString(),
            StandardPrice = o.StandardPrice,
            ProposedPrice = o.ProposedPrice,
            ApprovedPrice = o.ApprovedPrice,
            PriceApprovalStatus = o.PriceApprovalStatus.ToString(),
            PriceApproved = DesignerPayoutPricingRules.HasFinalizedDesignerPayout(o),
            CompletedDate = o.CompletedDate
        }).ToList();
    }

    public async Task<List<DesignerPayoutEligibleOrderDto>> GetDesignerPayoutEligibleOrdersForBuilderAsync(Guid designerId)
    {
        var orders = await _context.LogoOrders
            .Include(o => o.Client)
                .ThenInclude(c => c.User)
            .Where(o => o.DesignerId == designerId &&
                       !o.IsDeleted &&
                       o.Status == OrderStatus.Completed &&
                       !o.IsDesignerInvoiced)
            .Where(DesignerPayoutPricingRules.EligibleForDesignerInvoiceExpression)
            .OrderBy(o => o.CompletedDate)
            .ToListAsync();

        var orderIds = orders.Select(o => o.Id).ToList();
        var galleries = await _context.ClientGalleries
            .Where(g => orderIds.Contains(g.OrderId) && !g.IsDeleted)
            .OrderBy(g => g.ApprovedAt)
            .ToListAsync();
        var firstGalleryByOrder = galleries
            .GroupBy(g => g.OrderId)
            .ToDictionary(g => g.Key, g => g.First());

        return orders.Select(o =>
        {
            var clientName = o.Client?.User != null
                ? $"{o.Client.User.FirstName} {o.Client.User.LastName}".Trim()
                : o.Client?.CompanyName ?? "Unknown";
            if (string.IsNullOrEmpty(clientName))
                clientName = "Unknown";

            var previewUrl = firstGalleryByOrder.TryGetValue(o.Id, out var gallery)
                ? $"/api/files/{gallery.FileId}/download"
                : null;

            return new DesignerPayoutEligibleOrderDto
            {
                OrderId = o.Id,
                OrderNumber = NotificationFormatHelper.GetOrderNumber(o.Id),
                ClientName = clientName,
                DesignCategory = o.DesignCategory?.ToString(),
                DesignType = o.DesignType?.ToString(),
                ApprovedPrice = o.ApprovedPrice!.Value,
                CompletedDate = o.CompletedDate,
                PreviewImageUrl = previewUrl
            };
        }).ToList();
    }

    public async Task<DesignerPayoutOrderPreviewDto?> GetOrderPreviewAsync(Guid orderId)
    {
        var order = await _context.LogoOrders
            .Include(o => o.Client)
                .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

        if (order == null)
            return null;

        var clientName = order.Client?.User != null
            ? $"{order.Client.User.FirstName} {order.Client.User.LastName}".Trim()
            : order.Client?.CompanyName ?? "Unknown";
        if (string.IsNullOrEmpty(clientName))
            clientName = "Unknown";

        var finalFiles = await _context.ClientGalleries
            .Where(g => g.OrderId == orderId && !g.IsDeleted)
            .OrderBy(g => g.ApprovedAt)
            .Select(g => $"/api/files/{g.FileId}/download")
            .ToListAsync();

        return new DesignerPayoutOrderPreviewDto
        {
            OrderId = order.Id,
            OrderNumber = NotificationFormatHelper.GetOrderNumber(order.Id),
            ClientName = clientName,
            DesignCategory = order.DesignCategory?.ToString(),
            DesignType = order.DesignType?.ToString(),
            CompletedDate = order.CompletedDate,
            FinalFiles = finalFiles
        };
    }

    public async Task<DesignerInvoiceResponseDto> GenerateInvoiceFromBuilderAsync(GenerateDesignerInvoiceRequestDto request, Guid adminUserId)
    {
        if (_safetyOptions.DisableDesignerPayout)
            throw new InvalidOperationException("Designer payout temporarily disabled by administrator.");

        if (request.Orders == null || !request.Orders.Any())
            throw new InvalidOperationException("At least one order must be selected.");

        var designer = await _context.DesignerProfiles
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Id == request.DesignerId && !d.IsDeleted);

        if (designer == null)
            throw new InvalidOperationException("Designer not found.");

        var orderIds = request.Orders.Select(x => x.OrderId).Distinct().ToList();
        var orderAmounts = request.Orders.ToDictionary(x => x.OrderId, x => x.Amount);

        var billingPeriod = $"{request.BillingPeriodStart:MMM d} – {request.BillingPeriodEnd:MMM d, yyyy}";
        var invoiceNumber = $"DINV-{request.BillingPeriodStart:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
        var invoiceId = Guid.NewGuid();

        await _context.ExecuteInTransactionAsync(async (ct) =>
        {
            var invoice = new DesignerInvoice
            {
                Id = invoiceId,
                DesignerId = request.DesignerId,
                InvoiceNumber = invoiceNumber,
                TotalAmount = 0,
                Status = DesignerInvoiceStatus.Pending,
                BillingPeriod = billingPeriod,
                IssueDate = DateTime.UtcNow,
                CreatedBy = adminUserId
            };
            _context.DesignerInvoices.Add(invoice);

            decimal totalAmount = 0;
            foreach (var orderId in orderIds)
            {
                if (!orderAmounts.TryGetValue(orderId, out var amount) || amount <= 0)
                    continue;

                var order = await _context.LogoOrders
                    .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted, ct);

                if (order == null)
                    continue;

                if (order.Status != OrderStatus.Completed ||
                    !DesignerPayoutPricingRules.HasFinalizedDesignerPayout(order) ||
                    order.IsDesignerInvoiced ||
                    order.DesignerId != request.DesignerId)
                    continue;

                var alreadyInvoiced = await _context.DesignerInvoiceItems
                    .AnyAsync(i => i.OrderId == orderId && !i.IsDeleted, ct);
                if (alreadyInvoiced)
                    continue;

                var item = new DesignerInvoiceItem
                {
                    Id = Guid.NewGuid(),
                    DesignerInvoiceId = invoiceId,
                    OrderId = orderId,
                    Description = $"Order: {order.Title}",
                    Amount = amount,
                    CreatedBy = adminUserId
                };
                _context.DesignerInvoiceItems.Add(item);

                order.IsDesignerInvoiced = true;
                // BillingEligible must NOT be modified by designer payout - it is for client invoicing only
                order.InvoicedDate = DateTime.UtcNow;
                order.DesignerInvoiceId = invoiceId;

                totalAmount += amount;
            }

            invoice.TotalAmount = totalAmount;
            await _context.SaveChangesAsync(ct);
        });

        var itemsCount = await _context.DesignerInvoiceItems.CountAsync(i => i.DesignerInvoiceId == invoiceId && !i.IsDeleted);
        if (itemsCount == 0)
            throw new InvalidOperationException("No valid orders were added to the invoice. All selected orders may have failed validation or were already invoiced.");

        return await GetDesignerInvoiceByIdAsync(invoiceId) ?? throw new InvalidOperationException("Failed to retrieve created invoice.");
    }

    public async Task<List<OrderPricingSummaryDto>> GetDesignerOrdersPendingApprovalAsync(Guid designerId)
    {
        var orders = await _context.LogoOrders
            .Where(o => o.DesignerId == designerId &&
                       !o.IsDeleted &&
                       (o.PriceApprovalStatus == PriceApprovalStatus.PendingApproval || o.PriceApprovalStatus == PriceApprovalStatus.Modified) &&
                       o.RequiresPriceApproval)
            .OrderByDescending(o => o.UpdatedAt)
            .ToListAsync();

        return orders.Select(o => new OrderPricingSummaryDto
        {
            OrderId = o.Id,
            OrderTitle = o.Title,
            DesignCategory = o.DesignCategory?.ToString(),
            DesignType = o.DesignType?.ToString(),
            StandardPrice = o.StandardPrice,
            ProposedPrice = o.ProposedPrice,
            ApprovedPrice = o.ApprovedPrice,
            PriceApprovalStatus = o.PriceApprovalStatus.ToString(),
            PriceApproved = DesignerPayoutPricingRules.HasFinalizedDesignerPayout(o),
            CompletedDate = o.CompletedDate
        }).ToList();
    }

    private static DesignerInvoiceResponseDto MapToDto(DesignerInvoice invoice)
    {
        var designerName = invoice.Designer?.User != null
            ? $"{invoice.Designer.User.FirstName} {invoice.Designer.User.LastName}".Trim()
            : "Designer";

        var itemsTotal = invoice.Items.Sum(i => i.Amount);
        var adjustmentsTotal = invoice.Adjustments?.Sum(a => a.Amount) ?? 0;
        var totalAmount = itemsTotal + adjustmentsTotal;

        return new DesignerInvoiceResponseDto
        {
            Id = invoice.Id,
            DesignerId = invoice.DesignerId,
            DesignerName = designerName,
            InvoiceNumber = invoice.InvoiceNumber,
            TotalAmount = totalAmount,
            Status = invoice.Status.ToString(),
            BillingPeriod = invoice.BillingPeriod,
            IssueDate = invoice.IssueDate,
            PaidDate = invoice.PaidDate,
            Notes = invoice.Notes,
            Items = invoice.Items.Select(i => new DesignerInvoiceItemDto
            {
                Id = i.Id,
                OrderId = i.OrderId,
                OrderTitle = i.Order?.Title ?? "",
                Description = i.Description,
                Amount = i.Amount,
                DesignCategory = i.Order?.DesignCategory?.ToString(),
                DesignType = i.Order?.DesignType?.ToString()
            }).ToList(),
            Adjustments = (invoice.Adjustments ?? []).Select(a => new DesignerInvoiceAdjustmentDto
            {
                Id = a.Id,
                Description = a.Description,
                Amount = a.Amount
            }).ToList()
        };
    }

    public async Task UpdateDesignerInvoiceItemAsync(Guid invoiceId, Guid itemId, decimal amount, Guid adminUserId)
    {
        var invoice = await _context.DesignerInvoices
            .Include(i => i.Items)
            .Include(i => i.Adjustments)
            .FirstOrDefaultAsync(i => i.Id == invoiceId && !i.IsDeleted);

        if (invoice == null)
            throw new InvalidOperationException("Designer invoice not found.");

        if (invoice.Status == DesignerInvoiceStatus.Paid)
            throw new InvalidOperationException("Cannot edit a paid invoice.");

        var item = invoice.Items.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
            throw new InvalidOperationException("Invoice item not found.");

        if (amount <= 0)
            throw new InvalidOperationException("Amount must be greater than zero.");

        item.Amount = amount;
        item.UpdatedAt = DateTime.UtcNow;
        item.UpdatedBy = adminUserId;

        invoice.TotalAmount = invoice.Items.Sum(i => i.Amount) + (invoice.Adjustments?.Sum(a => a.Amount) ?? 0);
        invoice.UpdatedAt = DateTime.UtcNow;
        invoice.UpdatedBy = adminUserId;

        await _context.SaveChangesAsync();
    }

    public async Task AddDesignerInvoiceAdjustmentAsync(Guid invoiceId, string description, decimal amount, Guid adminUserId)
    {
        var invoice = await _context.DesignerInvoices
            .Include(i => i.Items)
            .Include(i => i.Adjustments)
            .FirstOrDefaultAsync(i => i.Id == invoiceId && !i.IsDeleted);

        if (invoice == null)
            throw new InvalidOperationException("Designer invoice not found.");

        if (invoice.Status == DesignerInvoiceStatus.Paid)
            throw new InvalidOperationException("Cannot edit a paid invoice.");

        if (string.IsNullOrWhiteSpace(description))
            throw new InvalidOperationException("Description is required.");

        var adjustment = new DesignerInvoiceAdjustment
        {
            Id = Guid.NewGuid(),
            DesignerInvoiceId = invoiceId,
            Description = description.Trim(),
            Amount = amount,
            CreatedBy = adminUserId
        };

        _context.DesignerInvoiceAdjustments.Add(adjustment);

        var itemsTotal = invoice.Items.Sum(i => i.Amount);
        var adjustmentsTotal = (invoice.Adjustments?.Sum(a => a.Amount) ?? 0) + amount;
        invoice.TotalAmount = itemsTotal + adjustmentsTotal;
        invoice.UpdatedAt = DateTime.UtcNow;
        invoice.UpdatedBy = adminUserId;

        await _context.SaveChangesAsync();
    }

    public async Task RemoveDesignerInvoiceAdjustmentAsync(Guid invoiceId, Guid adjustmentId, Guid adminUserId)
    {
        var invoice = await _context.DesignerInvoices
            .Include(i => i.Items)
            .Include(i => i.Adjustments)
            .FirstOrDefaultAsync(i => i.Id == invoiceId && !i.IsDeleted);

        if (invoice == null)
            throw new InvalidOperationException("Designer invoice not found.");

        if (invoice.Status == DesignerInvoiceStatus.Paid)
            throw new InvalidOperationException("Cannot edit a paid invoice.");

        var adjustment = invoice.Adjustments?.FirstOrDefault(a => a.Id == adjustmentId);
        if (adjustment == null)
            throw new InvalidOperationException("Adjustment not found.");

        _context.DesignerInvoiceAdjustments.Remove(adjustment);

        var itemsTotal = invoice.Items.Sum(i => i.Amount);
        var adjustmentsTotal = (invoice.Adjustments?.Where(a => a.Id != adjustmentId).Sum(a => a.Amount) ?? 0);
        invoice.TotalAmount = itemsTotal + adjustmentsTotal;
        invoice.UpdatedAt = DateTime.UtcNow;
        invoice.UpdatedBy = adminUserId;

        await _context.SaveChangesAsync();
    }

    public async Task<List<DesignerPayoutOverviewDto>> GetDesignerPayoutOverviewAsync()
    {
        var orders = await _context.LogoOrders
            .Include(o => o.Designer)
                .ThenInclude(d => d!.User)
            .Where(o => !o.IsDeleted &&
                       o.Status == OrderStatus.Completed &&
                       !o.IsDesignerInvoiced &&
                       o.DesignerId != null)
            .Where(DesignerPayoutPricingRules.EligibleForDesignerInvoiceExpression)
            .ToListAsync();

        var grouped = orders
            .GroupBy(o => o.DesignerId!.Value)
            .Select(g =>
            {
                var designer = g.First().Designer;
                var designerName = designer?.User != null
                    ? $"{designer.User.FirstName} {designer.User.LastName}".Trim()
                    : "Designer";
                if (string.IsNullOrEmpty(designerName))
                    designerName = "Designer";

                return new DesignerPayoutOverviewDto
                {
                    DesignerId = g.Key,
                    DesignerName = designerName,
                    CompletedOrders = g.Count(),
                    PendingPayoutAmount = g.Sum(o => o.ApprovedPrice!.Value)
                };
            })
            .OrderByDescending(x => x.PendingPayoutAmount)
            .ToList();

        return grouped;
    }

    public async Task<List<OrderPricingSummaryDto>> GetEligibleOrdersForPeriodAsync(Guid designerId, int year, int month)
    {
        var startDate = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = startDate.AddMonths(1);

        var orders = await _context.LogoOrders
            .Where(o => o.DesignerId == designerId &&
                       !o.IsDeleted &&
                       o.Status == OrderStatus.Completed &&
                       !o.IsDesignerInvoiced &&
                       o.CompletedDate >= startDate &&
                       o.CompletedDate < endDate)
            .Where(DesignerPayoutPricingRules.EligibleForDesignerInvoiceExpression)
            .OrderBy(o => o.CompletedDate)
            .ToListAsync();

        return orders.Select(o => new OrderPricingSummaryDto
        {
            OrderId = o.Id,
            OrderTitle = o.Title,
            DesignCategory = o.DesignCategory?.ToString(),
            DesignType = o.DesignType?.ToString(),
            StandardPrice = o.StandardPrice,
            ProposedPrice = o.ProposedPrice,
            ApprovedPrice = o.ApprovedPrice,
            PriceApprovalStatus = o.PriceApprovalStatus.ToString(),
            PriceApproved = DesignerPayoutPricingRules.HasFinalizedDesignerPayout(o),
            CompletedDate = o.CompletedDate
        }).ToList();
    }

    public async Task<DesignerPayoutSummaryDto> GetPayoutSummaryAsync()
    {
        var orders = await _context.LogoOrders
            .Where(o => !o.IsDeleted &&
                       o.Status == OrderStatus.Completed &&
                       !o.IsDesignerInvoiced &&
                       o.DesignerId != null)
            .Where(DesignerPayoutPricingRules.EligibleForDesignerInvoiceExpression)
            .ToListAsync();

        var designerCount = orders.Select(o => o.DesignerId!.Value).Distinct().Count();
        var totalOrders = orders.Count;
        var totalAmount = orders.Sum(o => o.ApprovedPrice!.Value);

        return new DesignerPayoutSummaryDto
        {
            DesignersWithPendingPayout = designerCount,
            TotalEligibleOrders = totalOrders,
            TotalPendingPayoutAmount = totalAmount
        };
    }
}
