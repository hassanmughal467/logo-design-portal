using LogoDesignPortal.API.Attributes;
using LogoDesignPortal.API.Extensions;
using LogoDesignPortal.Application.Constants;
using LogoDesignPortal.API.Models;
using LogoDesignPortal.Application.DTOs.Orders;
using LogoDesignPortal.Application.Exceptions;
using LogoDesignPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LogoDesignPortal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    [HttpPost]
    [Authorize(Roles = "Client")]
    [RequirePermission("CreateOrder")]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequestDto request)
    {
        try
        {
            if (User.GetUserId() is not { } userId)
            {
                return Unauthorized(new { error = "User identity could not be determined." });
            }

            var order = await _orderService.CreateOrderAsync(request, userId);
            return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("with-files")]
    [Authorize(Roles = "Client")]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [RequestSizeLimit(UploadLimits.MaxMultipartBytes)]
    public async Task<IActionResult> CreateOrderWithFiles([FromForm] string order, [FromForm] IFormFileCollection files, [FromForm] string? description = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(order))
            {
                return BadRequest(new { error = "Order data is required." });
            }

            if (files == null || files.Count == 0)
            {
                return BadRequest(new { error = "At least one reference file is required." });
            }

            var request = System.Text.Json.JsonSerializer.Deserialize<CreateOrderRequestDto>(order, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (request == null)
            {
                return BadRequest(new { error = "Invalid order data." });
            }

            if (User.GetUserId() is not { } userId)
            {
                return Unauthorized(new { error = "User identity could not be determined." });
            }

            var fileArray = files.ToArray();
            var result = await _orderService.CreateOrderWithFilesAsync(request, fileArray, userId, description);
            return CreatedAtAction(nameof(GetOrderById), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (System.Text.Json.JsonException ex)
        {
            return BadRequest(new { error = $"Invalid order JSON: {ex.Message}" });
        }
    }

    [HttpPost("manual-completed")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [RequestSizeLimit(UploadLimits.MaxMultipartBytes)]
    public async Task<IActionResult> CreateManualCompletedOrder([FromForm] string order, [FromForm] IFormFileCollection files)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(order))
            {
                return BadRequest(new { error = "Order data is required." });
            }

            if (files == null || files.Count == 0)
            {
                return BadRequest(new { error = "At least one final file is required." });
            }

            var request = System.Text.Json.JsonSerializer.Deserialize<CreateManualCompletedOrderRequestDto>(
                order,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (request == null)
            {
                return BadRequest(new { error = "Invalid order data." });
            }

            if (User.GetUserId() is not { } userId)
            {
                return Unauthorized(new { error = "User identity could not be determined." });
            }

            var created = await _orderService.CreateManualCompletedOrderAsync(request, files.ToArray(), userId);
            return CreatedAtAction(nameof(GetOrderById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (System.Text.Json.JsonException ex)
        {
            return BadRequest(new { error = $"Invalid order JSON: {ex.Message}" });
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetOrderById(Guid id)
    {
        try
        {
            if (User.GetUserId() is not { } userId)
            {
                return Unauthorized(this.StandardError("User identity could not be determined."));
            }

            var userRole = User.GetUserRole();
            var order = await _orderService.GetOrderByIdAsync(id, userId, userRole);

            if (order == null)
            {
                return NotFound(this.StandardError("Order not found."));
            }

            return Ok(order);
        }
        catch (ForbiddenAccessException ex)
        {
            // User is authenticated but doesn't have access to this resource
            return StatusCode(StatusCodes.Status403Forbidden, this.StandardError(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            // User is not authenticated
            return Unauthorized(this.StandardError(ex.Message));
        }
    }

    [HttpGet("my-orders")]
    [Authorize(Roles = "Client")]
    [ProducesResponseType(typeof(List<OrderResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyOrders()
    {
        if (User.GetUserId() is not { } userId)
        {
            return Unauthorized(new { error = "User identity could not be determined." });
        }

        var orders = await _orderService.GetOrdersByClientAsync(userId);
        return Ok(orders);
    }

    [HttpGet("assigned-orders")]
    [Authorize(Roles = "Designer")]
    [ProducesResponseType(typeof(List<OrderResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAssignedOrders()
    {
        if (User.GetUserId() is not { } userId)
        {
            return Unauthorized(new { error = "User identity could not be determined." });
        }

        var orders = await _orderService.GetOrdersByDesignerAsync(userId);
        return Ok(orders);
    }

    /// <summary>Typeahead order search (10–20 items).</summary>
    [HttpGet("search")]
    [Authorize]
    [RequirePermission("ViewAllOrders")]
    [ProducesResponseType(typeof(IReadOnlyList<OrderTypeaheadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SearchOrders([FromQuery] string? query, [FromQuery] int limit = 15)
    {
        limit = Math.Clamp(limit, 1, 20);
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        var items = await _orderService.SearchOrdersForTypeaheadAsync(userRole, query, limit);
        return Ok(items);
    }

    [HttpGet]
    [Authorize]
    [RequirePermission("ViewAllOrders")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var paged = await _orderService.GetOrdersPagedAsync(userRole, page, pageSize);
        var totalPages = paged.PageSize > 0 ? (int)Math.Ceiling((double)paged.Total / paged.PageSize) : 0;
        return Ok(new ApiResponse<object>
        {
            Data = new { items = paged.Items, total = paged.Total, page = paged.Page, pageSize = paged.PageSize },
            Message = null,
            Meta = new ApiMeta { Total = paged.Total, Page = paged.Page, PageSize = paged.PageSize, TotalPages = totalPages }
        });
    }

    [HttpPost("{id}/assign")]
    [Authorize] // Must be authenticated
    [RequirePermission("AssignOrder")] // Permission-based: SuperAdmin can grant to Admin
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AssignOrder(Guid id, [FromBody] AssignOrderRequestDto request)
    {
        try
        {
            if (User.GetUserId() is not { } assignedBy)
            {
                return Unauthorized(new { error = "User identity could not be determined." });
            }

            var order = await _orderService.AssignOrderToDesignerAsync(id, request.DesignerId, assignedBy);
            return Ok(order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "SuperAdmin,Admin,Designer,Client")]
    [RequirePermission("UpdateOrderStatus")]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusRequestDto request)
    {
        try
        {
            if (User.GetUserId() is not { } userId)
            {
                return Unauthorized(new { error = "User identity could not be determined." });
            }

            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var order = await _orderService.UpdateOrderStatusAsync(id, request, userId, userRole);
            return Ok(order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id}/request-price-approval")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RequestPriceApproval(Guid id, [FromBody] RequestPriceApprovalDto request)
    {
        try
        {
            if (User.GetUserId() is not { } requestedBy)
            {
                return Unauthorized(new { error = "User identity could not be determined." });
            }

            var order = await _orderService.RequestPriceApprovalAsync(id, request, requestedBy);
            return Ok(order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id}/approve-price")]
    [Authorize(Roles = "Client")]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ApprovePrice(Guid id, [FromBody] ApprovePriceDto request)
    {
        try
        {
            if (User.GetUserId() is not { } approvedBy)
            {
                return Unauthorized(new { error = "User identity could not be determined." });
            }

            var order = await _orderService.ApprovePriceAsync(id, request, approvedBy);
            return Ok(order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id}/respond-price-approval")]
    [Authorize(Roles = "Client")]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RespondPriceApproval(Guid id, [FromBody] RespondPriceApprovalDto request)
    {
        try
        {
            if (User.GetUserId() is not { } userId)
            {
                return Unauthorized(new { error = "User identity could not be determined." });
            }

            var order = await _orderService.RespondToPriceApprovalAsync(id, request, userId);
            return Ok(order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Approve a pending order.
    /// - With no body / no <c>designerId</c>: order moves to <c>ApprovedUnassigned</c>
    ///   and shows up in the SuperAdmin Unassigned Orders alert widget.
    /// - With <c>designerId</c>: order is approved and assigned in one step, moving to <c>InProgress</c>.
    /// Accepts both an empty body (Approve Only) and a JSON body with <c>designerId</c>/<c>notes</c>.
    /// </summary>
    [HttpPost("{id}/approve")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ApproveOrder(Guid id, [FromBody] ApproveOrderRequestDto? request = null)
    {
        try
        {
            if (User.GetUserId() is not { } approvedBy)
            {
                return Unauthorized(new { error = "User identity could not be determined." });
            }

            var order = await _orderService.ApproveOrderAsync(id, request ?? new ApproveOrderRequestDto(), approvedBy);
            return Ok(order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Returns the count of orders currently parked in <c>ApprovedUnassigned</c>.
    /// Powers the SuperAdmin dashboard "Unassigned Orders" alert widget.
    /// </summary>
    [HttpGet("unassigned-count")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUnassignedApprovedCount()
    {
        var count = await _orderService.GetUnassignedApprovedCountAsync();
        return Ok(new { count, status = "ApprovedUnassigned" });
    }

    [HttpPost("{id}/send-files-to-client")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [RequirePermission("UpdateOrderStatus")]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendFilesToClient(Guid id, [FromBody] List<Guid> fileIds)
    {
        try
        {
            if (User.GetUserId() is not { } sentBy)
            {
                return Unauthorized(new { error = "User identity could not be determined." });
            }

            var order = await _orderService.SendFilesToClientAsync(id, fileIds, sentBy);
            return Ok(order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id}/send-preview-batch")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [RequirePermission("UpdateOrderStatus")]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendPreviewBatchToClient(Guid id, [FromBody] SendPreviewBatchRequestDto? request)
    {
        try
        {
            if (request == null)
            {
                return BadRequest(new { error = "PreviewBatchId is required." });
            }

            if (User.GetUserId() is not { } sentBy)
            {
                return Unauthorized(new { error = "User identity could not be determined." });
            }

            var order = await _orderService.SendPreviewBatchToClientAsync(id, request.PreviewBatchId, sentBy);
            return Ok(order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Client")]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateOrder(Guid id, [FromBody] UpdateOrderRequestDto request)
    {
        try
        {
            if (User.GetUserId() is not { } userId)
            {
                return Unauthorized(new { error = "User identity could not be determined." });
            }

            var order = await _orderService.UpdateOrderAsync(id, request, userId);
            return Ok(order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
        }
    }

    [HttpPost("{id}/cancel")]
    [Authorize(Roles = "Client,Admin,SuperAdmin")]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CancelOrder(Guid id, [FromBody] CancelOrderRequestDto request)
    {
        try
        {
            if (User.GetUserId() is not { } userId)
            {
                return Unauthorized(new { error = "User identity could not be determined." });
            }

            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var order = await _orderService.CancelOrderAsync(id, request, userId, userRole!);
            return Ok(order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
        }
    }

    [HttpPost("{id}/archive")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ArchiveOrder(Guid id, [FromBody] ArchiveOrderRequestDto? request)
    {
        try
        {
            if (User.GetUserId() is not { } userId)
            {
                return Unauthorized(new { error = "User identity could not be determined." });
            }

            var order = await _orderService.ArchiveOrderAsync(id, request, userId);
            return Ok(order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id}/unarchive")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UnarchiveOrder(Guid id)
    {
        try
        {
            if (User.GetUserId() is not { } userId)
            {
                return Unauthorized(new { error = "User identity could not be determined." });
            }

            var order = await _orderService.UnarchiveOrderAsync(id, userId);
            return Ok(order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id}/refund")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefundOrder(Guid id, [FromBody] RefundOrderRequestDto request)
    {
        try
        {
            if (User.GetUserId() is not { } userId)
            {
                return Unauthorized(new { error = "User identity could not be determined." });
            }

            var order = await _orderService.RefundOrderAsync(id, request, userId);
            return Ok(order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}/client-price")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateClientChargePrice(Guid id, [FromBody] UpdateClientChargePriceRequestDto request)
    {
        try
        {
            if (User.GetUserId() is not { } userId)
            {
                return Unauthorized(new { error = "User identity could not be determined." });
            }

            var userRole = User.FindFirstValue(System.Security.Claims.ClaimTypes.Role);
            var order = await _orderService.UpdateClientChargePriceAsync(id, request, userId, userRole);
            return Ok(order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}/allow-uploads")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetAllowUploads(Guid id, [FromBody] SetAllowUploadsRequestDto request)
    {
        try
        {
            if (User.GetUserId() is not { } userId)
            {
                return Unauthorized(new { error = "User identity could not be determined." });
            }

            var order = await _orderService.SetAllowUploadsAsync(id, request.AllowUploads, userId);
            return Ok(order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id}/logs")]
    [Authorize]
    [ProducesResponseType(typeof(List<OrderLogResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderLogs(Guid id)
    {
        try
        {
            if (User.GetUserId() is not { } userId)
            {
                return Unauthorized(new { error = "User identity could not be determined." });
            }

            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var logs = await _orderService.GetOrderLogsWithAccessAsync(id, userId, userRole);
            return Ok(logs);
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order logs for order {OrderId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An error occurred while retrieving order logs." });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Client,Admin,SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Obsolete("Use CancelOrder endpoint instead. Orders should never be hard-deleted.")]
    public async Task<IActionResult> DeleteOrder(Guid id)
    {
        try
        {
            if (User.GetUserId() is not { } userId)
            {
                return Unauthorized(new { error = "User identity could not be determined." });
            }

            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var cancelRequest = new CancelOrderRequestDto
            {
                Reason = "Order deleted via legacy endpoint - should use cancel endpoint"
            };
            var order = await _orderService.CancelOrderAsync(id, cancelRequest, userId, userRole!);
            return Ok(new { message = "Order cancelled successfully (legacy delete endpoint).", order });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
        }
    }
}
