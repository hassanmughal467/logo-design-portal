using LogoDesignPortal.Application.DTOs.AuditLogs;

namespace LogoDesignPortal.Application.Interfaces;

public interface IAuditLogService
{
    Task LogActionAsync(string entityType, Guid entityId, string action, Guid? performedByUserId, string? performedByRole, string? previousValue = null, string? newValue = null, string? notes = null);
    Task<List<AuditLogResponseDto>> GetAuditLogsAsync(string? entityType = null, Guid? entityId = null, int pageNumber = 1, int pageSize = 50);
    Task<List<AuditLogResponseDto>> GetEntityAuditLogsAsync(string entityType, Guid entityId);
}
