using backend.Entities;

namespace backend.Services.Interfaces;

public interface IPlatformSettingService
{
    Task<int> GetPlaylistLimitAsync(string userRole);
    Task<int> GetStreamViewerLimitAsync(string hostRole);
    Task<decimal> GetPremiumPriceAsync();
    Task<IEnumerable<PlatformSetting>> GetAllSettingsAsync();
    Task UpdateSettingAsync(string key, string value, Guid adminId);
}
