using LogoDesignPortal.API.Extensions;
using LogoDesignPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LogoDesignPortal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Client")]
public class ClientController : ControllerBase
{
    private readonly IClientFinancialInsightsService _financialInsightsService;
    private readonly ILogger<ClientController> _logger;

    public ClientController(
        IClientFinancialInsightsService financialInsightsService,
        ILogger<ClientController> logger)
    {
        _financialInsightsService = financialInsightsService;
        _logger = logger;
    }

    [HttpGet("financial-insights")]
    [ProducesResponseType(typeof(LogoDesignPortal.Application.DTOs.Client.FinancialInsightsResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFinancialInsights()
    {
        var userId = User.GetUserIdOrThrow();
        var data = await _financialInsightsService.GetFinancialInsightsAsync(userId);
        return Ok(data);
    }
}
