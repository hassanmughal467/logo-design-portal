using LogoDesignPortal.Application.DTOs.AuditLogs;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Interfaces.Persistence;
using LogoDesignPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LogoDesignPortal.Application.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IApplicationDbContext _context;

    public AuditLogService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task LogActionAsync(string entityType, Guid entityId, string action, Guid? performedByUserId, string? performedByRole, string? previousValue = null, string? newValue = null, string? notes = null)
    {
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            PreviousValue = previousValue,
            NewValue = newValue,
            PerformedByUserId = performedByUserId,
            PerformedByRole = performedByRole,
            Timestamp = DateTime.UtcNow,
            Notes = notes,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = performedByUserId
        };

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
    }

    public async Task<List<AuditLogResponseDto>> GetAuditLogsAsync(string? entityType = null, Guid? entityId = null, int pageNumber = 1, int pageSize = 50)
    {
        var query = _context.AuditLogs
            .Where(a => !a.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrEmpty(entityType))
        {
            query = query.Where(a => a.EntityType == entityType);
        }

        if (entityId.HasValue)
        {
            query = query.Where(a => a.EntityId == entityId.Value);
        }

        var logs = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return logs.Select(log =>
        {
            var dto = new AuditLogResponseDto
            {
                Id = log.Id,
                EntityType = log.EntityType,
                EntityId = log.EntityId,
                Action = log.Action,
                PreviousValue = log.PreviousValue,
                NewValue = log.NewValue,
                PerformedByUserId = log.PerformedByUserId,
                PerformedByRole = log.PerformedByRole,
                Timestamp = log.Timestamp,
                Notes = log.Notes
            };

            // Get performer name if available
            if (log.PerformedByUserId.HasValue)
            {
                var user = _context.Users.FirstOrDefault(u => u.Id == log.PerformedByUserId.Value);
                if (user != null)
                {
                    dto.PerformedByName = $"{user.FirstName} {user.LastName}";
                }
            }

            return dto;
        }).ToList();
    }

    public async Task<List<AuditLogResponseDto>> GetEntityAuditLogsAsync(string entityType, Guid entityId)
    {
        var logs = await _context.AuditLogs
            .Where(a => !a.IsDeleted && a.EntityType == entityType && a.EntityId == entityId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();

        return logs.Select(log =>
        {
            var dto = new AuditLogResponseDto
            {
                Id = log.Id,
                EntityType = log.EntityType,
                EntityId = log.EntityId,
                Action = log.Action,
                PreviousValue = log.PreviousValue,
                NewValue = log.NewValue,
                PerformedByUserId = log.PerformedByUserId,
                PerformedByRole = log.PerformedByRole,
                Timestamp = log.Timestamp,
                Notes = log.Notes
            };

            // Get performer name if available
            if (log.PerformedByUserId.HasValue)
            {
                var user = _context.Users.FirstOrDefault(u => u.Id == log.PerformedByUserId.Value);
                if (user != null)
                {
                    dto.PerformedByName = $"{user.FirstName} {user.LastName}";
                }
            }

            return dto;
        }).ToList();
    }
}
