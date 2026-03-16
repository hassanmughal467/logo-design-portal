using LogoDesignPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LogoDesignPortal.API.Controllers.Admin;

[ApiController]
[Route("api/admin/analytics")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(IAnalyticsService analyticsService, ILogger<AnalyticsController> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    [HttpGet("overview")]
    [ProducesResponseType(typeof(AnalyticsOverviewDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOverview()
    {
        var data = await _analyticsService.GetOverviewAsync();
        return Ok(data);
    }

    [HttpGet("orders")]
    [ProducesResponseType(typeof(OrderAnalyticsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrderAnalytics()
    {
        var data = await _analyticsService.GetOrderAnalyticsAsync();
        return Ok(data);
    }

    [HttpGet("revenue")]
    [ProducesResponseType(typeof(RevenueAnalyticsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRevenueAnalytics()
    {
        var data = await _analyticsService.GetRevenueAnalyticsAsync();
        return Ok(data);
    }

    [HttpGet("designers")]
    [ProducesResponseType(typeof(DesignerAnalyticsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDesignerAnalytics()
    {
        var data = await _analyticsService.GetDesignerAnalyticsAsync();
        return Ok(data);
    }

    [HttpGet("clients")]
    [ProducesResponseType(typeof(ClientAnalyticsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetClientAnalytics()
    {
        var data = await _analyticsService.GetClientAnalyticsAsync();
        return Ok(data);
    }

    [HttpGet("workflow")]
    [ProducesResponseType(typeof(WorkflowAnalyticsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWorkflowAnalytics()
    {
        var data = await _analyticsService.GetWorkflowAnalyticsAsync();
        return Ok(data);
    }

    [HttpGet("system")]
    [ProducesResponseType(typeof(SystemAnalyticsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSystemAnalytics()
    {
        var data = await _analyticsService.GetSystemAnalyticsAsync();
        return Ok(data);
    }

    [HttpGet("forecast")]
    [ProducesResponseType(typeof(ForecastAnalyticsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetForecastAnalytics()
    {
        var data = await _analyticsService.GetForecastAnalyticsAsync();
        return Ok(data);
    }

    [HttpGet("insights")]
    [ProducesResponseType(typeof(InsightsAnalyticsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInsights()
    {
        var data = await _analyticsService.GetInsightsAsync();
        return Ok(data);
    }
}
