using LogoDesignPortal.Application.DTOs.AuditLogs;
using LogoDesignPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LogoDesignPortal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<AuditLogsController> _logger;

    public AuditLogsController(IAuditLogService auditLogService, ILogger<AuditLogsController> logger)
    {
        _auditLogService = auditLogService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<AuditLogResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuditLogs([FromQuery] string? entityType = null, [FromQuery] Guid? entityId = null, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
    {
        var logs = await _auditLogService.GetAuditLogsAsync(entityType, entityId, pageNumber, pageSize);
        return Ok(logs);
    }

    [HttpGet("{entityType}/{entityId}")]
    [ProducesResponseType(typeof(List<AuditLogResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEntityAuditLogs(string entityType, Guid entityId)
    {
        var logs = await _auditLogService.GetEntityAuditLogsAsync(entityType, entityId);
        return Ok(logs);
    }
}
