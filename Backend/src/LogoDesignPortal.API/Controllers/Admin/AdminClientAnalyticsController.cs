using LogoDesignPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LogoDesignPortal.API.Controllers.Admin;

[ApiController]
[Route("api/admin/client-analytics")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class AdminClientAnalyticsController : ControllerBase
{
    private readonly IClientAnalyticsService _clientAnalytics;
    private readonly IClientChurnAnalyticsService _clientChurnAnalytics;
    private readonly ILogger<AdminClientAnalyticsController> _logger;

    public AdminClientAnalyticsController(
        IClientAnalyticsService clientAnalytics,
        IClientChurnAnalyticsService clientChurnAnalytics,
        ILogger<AdminClientAnalyticsController> logger)
    {
        _clientAnalytics = clientAnalytics;
        _clientChurnAnalytics = clientChurnAnalytics;
        _logger = logger;
    }

    [HttpGet("overview")]
    [ProducesResponseType(typeof(ClientAnalyticsOverviewDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOverview()
    {
        var data = await _clientAnalytics.GetOverviewAsync();
        return Ok(data);
    }

    [HttpGet("top-clients")]
    [ProducesResponseType(typeof(TopClientsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTopClients([FromQuery] int limit = 10)
    {
        var data = await _clientAnalytics.GetTopClientsAsync(limit);
        return Ok(data);
    }

    [HttpGet("revenue-trend")]
    [ProducesResponseType(typeof(ClientRevenueTrendDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRevenueTrend([FromQuery] int months = 6)
    {
        var data = await _clientAnalytics.GetClientRevenueTrendAsync(months);
        return Ok(data);
    }

    [HttpGet("monthly-revenue")]
    [ProducesResponseType(typeof(ClientMonthlyRevenueDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMonthlyRevenue([FromQuery] Guid clientId, [FromQuery] int months = 12)
    {
        var data = await _clientAnalytics.GetClientMonthlyRevenueAsync(clientId, months);
        return Ok(data);
    }

    [HttpGet("inactive-clients")]
    [ProducesResponseType(typeof(InactiveClientsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInactiveClients([FromQuery] int inactiveDaysThreshold = 30)
    {
        var data = await _clientAnalytics.GetInactiveClientsAsync(inactiveDaysThreshold);
        return Ok(data);
    }

    [HttpGet("lifetime-value")]
    [ProducesResponseType(typeof(ClientLifetimeValueDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLifetimeValue()
    {
        var data = await _clientAnalytics.GetClientLifetimeValueAsync();
        return Ok(data);
    }

    [HttpGet("client-growth")]
    [ProducesResponseType(typeof(ClientGrowthDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetClientGrowth()
    {
        var data = await _clientAnalytics.GetClientGrowthMetricsAsync();
        return Ok(data);
    }

    [HttpGet("client-activity")]
    [ProducesResponseType(typeof(ClientActivityDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetClientActivity()
    {
        var data = await _clientAnalytics.GetClientActivityStatusAsync();
        return Ok(data);
    }

    [HttpGet("alerts")]
    [ProducesResponseType(typeof(ClientAlertsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAlerts()
    {
        var data = await _clientAnalytics.GetClientAlertsAsync();
        return Ok(data);
    }

    [HttpGet("clients-dropdown")]
    [ProducesResponseType(typeof(List<ClientDropdownItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetClientsForDropdown()
    {
        var data = await _clientAnalytics.GetClientsForDropdownAsync();
        return Ok(data);
    }

    // Churn Prediction endpoints
    [HttpGet("churn-risk")]
    [ProducesResponseType(typeof(ClientChurnRiskScoresDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetChurnRisk()
    {
        var data = await _clientChurnAnalytics.GetClientRiskScoresAsync();
        return Ok(data);
    }

    [HttpGet("churn-alerts")]
    [ProducesResponseType(typeof(ClientChurnAlertsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetChurnAlerts([FromQuery] int inactiveDaysThreshold = 30)
    {
        var data = await _clientChurnAnalytics.GetClientChurnAlertsAsync(inactiveDaysThreshold);
        return Ok(data);
    }

    [HttpGet("retention-stats")]
    [ProducesResponseType(typeof(ClientRetentionStatsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRetentionStats([FromQuery] int months = 12)
    {
        var data = await _clientChurnAnalytics.GetClientRetentionStatsAsync(months);
        return Ok(data);
    }
}
