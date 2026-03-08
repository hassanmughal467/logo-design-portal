using AutoMapper;
using LogoDesignPortal.Application.DTOs.Notifications;
using LogoDesignPortal.Application.Helpers;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Interfaces.Persistence;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace LogoDesignPortal.Application.Services;

public class NotificationService : INotificationService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IEmailService _emailService;
    private readonly ISettingsService _settingsService;
    private readonly IRealtimeNotificationSender _realtimeSender;
    private readonly IConfiguration _configuration;

    public NotificationService(IApplicationDbContext context, IMapper mapper, IEmailService emailService, ISettingsService settingsService, IRealtimeNotificationSender realtimeSender, IConfiguration configuration)
    {
        _context = context;
        _mapper = mapper;
        _emailService = emailService;
        _settingsService = settingsService;
        _realtimeSender = realtimeSender;
        _configuration = configuration;
    }

    private static readonly TimeSpan DeduplicationWindow = TimeSpan.FromMinutes(5);

    public async Task<NotificationResponseDto> CreateNotificationAsync(Guid userId, string title, string message, NotificationType type, Guid? orderId = null, NotificationReferenceType referenceType = NotificationReferenceType.Order, Guid? referenceId = null, Guid? createdBy = null)
    {
        var refId = referenceId ?? orderId;
        var now = DateTime.UtcNow;
        var windowStart = now - DeduplicationWindow;

        var existing = await _context.Notifications
            .Where(n => n.UserId == userId
                && n.ReferenceType == referenceType
                && n.Type == type
                && !n.IsDeleted
                && (n.LastOccurrenceAt ?? n.CreatedAt) >= windowStart)
            .Where(n => (refId == null && n.ReferenceId == null) || (refId != null && n.ReferenceId == refId))
            .OrderByDescending(n => n.LastOccurrenceAt ?? n.CreatedAt)
            .FirstOrDefaultAsync();

        if (existing != null)
        {
            existing.AggregationCount += 1;
            existing.LastOccurrenceAt = now;
            existing.Message = NotificationMessageAggregator.BuildAggregatedMessage(message, existing.AggregationCount);
            existing.UpdatedAt = now;
            existing.RedirectUrl ??= NotificationRedirectHelper.BuildRedirectUrl(referenceType, refId, orderId);

            await _context.SaveChangesAsync();

            var dto = _mapper.Map<NotificationResponseDto>(existing);
            await _realtimeSender.SendNotificationToUserAsync(userId, dto);
            return dto;
        }

        var redirectUrl = NotificationRedirectHelper.BuildRedirectUrl(referenceType, refId, orderId);

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            OrderId = orderId,
            Title = title,
            Message = message,
            Type = type,
            ReferenceType = referenceType,
            ReferenceId = refId,
            RedirectUrl = redirectUrl,
            IsRead = false,
            CreatedAt = now,
            CreatedBy = createdBy,
            AggregationCount = 1
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        var newDto = _mapper.Map<NotificationResponseDto>(notification);

        await SendEmailIfEnabledAsync(userId, title, message, newDto);
        await _realtimeSender.SendNotificationToUserAsync(userId, newDto);

        return newDto;
    }

    public async Task<NotificationResponseDto> CreateNotificationForUserAsync(Guid userId, string title, string message, NotificationType type, NotificationReferenceType referenceType = NotificationReferenceType.Order, Guid? referenceId = null, Guid? createdBy = null)
    {
        var orderId = referenceType == NotificationReferenceType.Order ? referenceId : null;
        return await CreateNotificationAsync(userId, title, message, type, orderId, referenceType, referenceId, createdBy);
    }

    public async Task<List<NotificationResponseDto>> CreateNotificationForUsersAsync(IEnumerable<Guid> userIds, string title, string message, NotificationType type, NotificationReferenceType referenceType = NotificationReferenceType.Order, Guid? referenceId = null, Guid? createdBy = null)
    {
        var distinctUserIds = userIds.Distinct().ToList();
        var results = new List<NotificationResponseDto>();

        foreach (var userId in distinctUserIds)
        {
            var dto = await CreateNotificationAsync(userId, title, message, type, referenceType == NotificationReferenceType.Order ? referenceId : null, referenceType, referenceId, createdBy);
            results.Add(dto);
        }

        return results;
    }

    public async Task<List<NotificationResponseDto>> CreateNotificationForRoleAsync(string roleName, string title, string message, NotificationType type, NotificationReferenceType referenceType = NotificationReferenceType.Order, Guid? referenceId = null, Guid? createdBy = null)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
        if (role == null)
        {
            return new List<NotificationResponseDto>();
        }

        var userIds = await _context.Users
            .Where(u => !u.IsDeleted && u.RoleId == role.Id)
            .Select(u => u.Id)
            .ToListAsync();

        return await CreateNotificationForUsersAsync(userIds, title, message, type, referenceType, referenceId, createdBy);
    }

    private async Task SendEmailIfEnabledAsync(Guid userId, string title, string message, NotificationResponseDto dto)
    {
        try
        {
            var settings = await _settingsService.GetSettingsAsync();
            var emailNotificationsEnabled = settings.Notifications.TryGetValue("EmailNotifications", out var emailEnabled) && emailEnabled
                || settings.Notifications.TryGetValue("emailNotifications", out var emailEnabledCamel) && emailEnabledCamel;

            if (emailNotificationsEnabled)
            {
                var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
                if (user != null && !string.IsNullOrEmpty(user.Email))
                {
                    var frontendUrl = _configuration["Email:FrontendUrl"] ?? "http://localhost:4200";
                    var path = !string.IsNullOrEmpty(dto.RedirectUrl) ? dto.RedirectUrl.TrimStart('/') : "notifications";
                    var link = $"{frontendUrl.TrimEnd('/')}/{path}";
                    var htmlBody = $@"
<!DOCTYPE html>
<html>
<head><style>body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}</style></head>
<body>
<p>{message}</p>
<p><a href=""{link}"">View in portal</a></p>
<p><small>This is an automated notification from Hawk Merchandising Web Portal.</small></p>
</body>
</html>";
                    await _emailService.SendEmailAsync(user.Email, title, htmlBody, true);
                }
            }
        }
        catch
        {
            // Email/settings failure must never affect notification persistence
        }
    }

    public async Task<List<NotificationResponseDto>> GetUserNotificationsAsync(Guid userId, bool unreadOnly = false)
    {
        IQueryable<Notification> query = _context.Notifications
            .Where(n => n.UserId == userId && !n.IsDeleted)
            .OrderByDescending(n => n.LastOccurrenceAt ?? n.CreatedAt);

        if (unreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }

        var notifications = await query.ToListAsync();
        return _mapper.Map<List<NotificationResponseDto>>(notifications);
    }

    public async Task<NotificationListResponseDto> GetUserNotificationsPaginatedAsync(
        Guid userId,
        int limit = 20,
        int offset = 0,
        bool unreadOnly = false,
        string? search = null,
        string? referenceType = null,
        string? type = null)
    {
        var query = _context.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId && !n.IsDeleted);

        if (unreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.Trim().ToLower();
            query = query.Where(n =>
                (n.Title != null && n.Title.ToLower().Contains(searchTerm)) ||
                (n.Message != null && n.Message.ToLower().Contains(searchTerm)));
        }

        if (!string.IsNullOrWhiteSpace(referenceType) && Enum.TryParse<NotificationReferenceType>(referenceType.Trim(), true, out var refType))
        {
            query = query.Where(n => n.ReferenceType == refType);
        }

        if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse<NotificationType>(type.Trim(), true, out var notifType))
        {
            query = query.Where(n => n.Type == notifType);
        }

        var totalCount = await query.CountAsync();

        var notifications = await query
            .OrderByDescending(n => n.LastOccurrenceAt ?? n.CreatedAt)
            .Skip(offset)
            .Take(Math.Min(limit, 100))
            .ToListAsync();

        var items = _mapper.Map<List<NotificationResponseDto>>(notifications);

        return new NotificationListResponseDto
        {
            Items = items,
            TotalCount = totalCount,
            Limit = Math.Min(limit, 100),
            Offset = offset
        };
    }

    public async Task<NotificationResponseDto?> GetNotificationByIdAsync(Guid notificationId, Guid userId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId && !n.IsDeleted);

        return notification == null ? null : _mapper.Map<NotificationResponseDto>(notification);
    }

    public async Task<bool> MarkNotificationAsReadAsync(Guid notificationId, Guid userId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId && !n.IsDeleted);

        if (notification == null)
        {
            return false;
        }

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> MarkAllNotificationsAsReadAsync(Guid userId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead && !n.IsDeleted)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetUnreadNotificationCountAsync(Guid userId)
    {
        return await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead && !n.IsDeleted);
    }
}
