using LogoDesignPortal.Application.DTOs.Settings;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Interfaces.Persistence;
using LogoDesignPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace LogoDesignPortal.Application.Services;

public class SettingsService : ISettingsService
{
    private readonly IApplicationDbContext _context;

    public SettingsService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SettingsResponseDto> GetSettingsAsync()
    {
        var settings = await _context.Settings
            .Where(s => !s.IsDeleted)
            .ToListAsync();

        var response = new SettingsResponseDto();

        foreach (var setting in settings)
        {
            switch (setting.Category)
            {
                case "Business":
                    response.Business[setting.Key] = setting.Value;
                    break;
                case "Brand":
                    response.Brand[setting.Key] = setting.Value;
                    break;
                case "InvoiceTemplate":
                    response.InvoiceTemplate[setting.Key] = setting.Value;
                    break;
                case "Invoice":
                    response.Invoice[setting.Key] = setting.Value;
                    break;
                case "PaymentMethods":
                    try
                    {
                        var methods = JsonSerializer.Deserialize<List<PaymentMethodDto>>(setting.Value);
                        if (methods != null)
                        {
                            response.PaymentMethods = methods;
                        }
                    }
                    catch
                    {
                        // Ignore deserialization errors
                    }
                    break;
                case "Notifications":
                    if (bool.TryParse(setting.Value, out var boolValue))
                    {
                        response.Notifications[setting.Key] = boolValue;
                    }
                    break;
            }
        }

        return response;
    }

    public async Task UpdateSettingsAsync(string category, Dictionary<string, object> data, Guid updatedBy)
    {
        foreach (var kvp in data)
        {
            var existing = await _context.Settings
                .FirstOrDefaultAsync(s => s.Key == kvp.Key && s.Category == category && !s.IsDeleted);

            var value = kvp.Value switch
            {
                string str => str,
                bool b => b.ToString(),
                _ => JsonSerializer.Serialize(kvp.Value)
            };

            if (existing != null)
            {
                existing.Value = value;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.UpdatedBy = updatedBy;
            }
            else
            {
                var setting = new Settings
                {
                    Id = Guid.NewGuid(),
                    Key = kvp.Key,
                    Value = value,
                    Category = category,
                    CreatedBy = updatedBy
                };
                _context.Settings.Add(setting);
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task<string?> GetSettingAsync(string key)
    {
        var setting = await _context.Settings
            .FirstOrDefaultAsync(s => s.Key == key && !s.IsDeleted);

        return setting?.Value;
    }

    public async Task SetSettingAsync(string key, string value, string category, Guid updatedBy)
    {
        var existing = await _context.Settings
            .FirstOrDefaultAsync(s => s.Key == key && s.Category == category && !s.IsDeleted);

        if (existing != null)
        {
            existing.Value = value;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedBy = updatedBy;
        }
        else
        {
            var setting = new Settings
            {
                Id = Guid.NewGuid(),
                Key = key,
                Value = value,
                Category = category,
                CreatedBy = updatedBy
            };
            _context.Settings.Add(setting);
        }

        await _context.SaveChangesAsync();
    }
}
