using LogoDesignPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LogoDesignPortal.API.Controllers.Admin;

[ApiController]
[Route("api/admin/financial")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class FinancialController : ControllerBase
{
    private readonly IFinancialAnalyticsService _financialService;
    private readonly ILogger<FinancialController> _logger;

    public FinancialController(IFinancialAnalyticsService financialService, ILogger<FinancialController> logger)
    {
        _financialService = financialService;
        _logger = logger;
    }

    [HttpGet("overview")]
    [ProducesResponseType(typeof(FinancialOverviewDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOverview()
    {
        var data = await _financialService.GetOverviewAsync();
        return Ok(data);
    }

    [HttpGet("revenue-trend")]
    [ProducesResponseType(typeof(RevenueTrendDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRevenueTrend()
    {
        var data = await _financialService.GetRevenueTrendAsync();
        return Ok(data);
    }

    [HttpGet("orders-vs-revenue")]
    [ProducesResponseType(typeof(OrdersVsRevenueDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrdersVsRevenue()
    {
        var data = await _financialService.GetOrdersVsRevenueAsync();
        return Ok(data);
    }

    [HttpGet("package-revenue")]
    [ProducesResponseType(typeof(PackageRevenueDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPackageRevenue()
    {
        var data = await _financialService.GetPackageRevenueAsync();
        return Ok(data);
    }

    [HttpGet("designer-revenue")]
    [ProducesResponseType(typeof(DesignerRevenueDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDesignerRevenue()
    {
        var data = await _financialService.GetDesignerRevenueAsync();
        return Ok(data);
    }

    [HttpGet("client-revenue")]
    [ProducesResponseType(typeof(ClientRevenueDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetClientRevenue()
    {
        var data = await _financialService.GetClientRevenueAsync();
        return Ok(data);
    }

    [HttpGet("invoice-status")]
    [ProducesResponseType(typeof(InvoiceStatusDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInvoiceStatus()
    {
        var data = await _financialService.GetInvoiceStatusAsync();
        return Ok(data);
    }

    [HttpGet("weekly-revenue")]
    [ProducesResponseType(typeof(WeeklyRevenueDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWeeklyRevenue()
    {
        var data = await _financialService.GetWeeklyRevenueAsync();
        return Ok(data);
    }

    [HttpGet("order-value-trend")]
    [ProducesResponseType(typeof(OrderValueTrendDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrderValueTrend()
    {
        var data = await _financialService.GetOrderValueTrendAsync();
        return Ok(data);
    }

    [HttpGet("activity-feed")]
    [ProducesResponseType(typeof(FinancialActivityFeedDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActivityFeed()
    {
        var data = await _financialService.GetActivityFeedAsync();
        return Ok(data);
    }

    [HttpGet("revenue-forecast")]
    [ProducesResponseType(typeof(RevenueForecastDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRevenueForecast()
    {
        var data = await _financialService.GetRevenueForecastAsync();
        return Ok(data);
    }
}
