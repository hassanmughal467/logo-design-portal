using LogoDesignPortal.Application.DTOs.Settings;

namespace LogoDesignPortal.Application.Interfaces;

public interface ISettingsService
{
    Task<SettingsResponseDto> GetSettingsAsync();
    Task UpdateSettingsAsync(string category, Dictionary<string, object> data, Guid updatedBy);
    Task<string?> GetSettingAsync(string key);
    Task SetSettingAsync(string key, string value, string category, Guid updatedBy);
}
