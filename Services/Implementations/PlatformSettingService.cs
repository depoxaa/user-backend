using backend.Entities;
using backend.Repositories.Interfaces;
using backend.Services.Interfaces;

namespace backend.Services.Implementations;

public class PlatformSettingService : IPlatformSettingService
{
    private readonly IPlatformSettingRepository _settingRepository;

    public PlatformSettingService(IPlatformSettingRepository settingRepository)
    {
        _settingRepository = settingRepository;
    }

    public async Task<int> GetPlaylistLimitAsync(string userRole)
    {
        var key = userRole == "Premium"
            ? "playlist_limit_premium"
            : "playlist_limit_ordinary";
        return await _settingRepository.GetIntValueAsync(key, userRole == "Premium" ? 50 : 5);
    }

    public async Task<int> GetStreamViewerLimitAsync(string hostRole)
    {
        // Artists are treated as ordinary unless they also hold premium
        var key = hostRole == "Premium"
            ? "stream_viewer_limit_premium"
            : "stream_viewer_limit_ordinary";
        return await _settingRepository.GetIntValueAsync(key, hostRole == "Premium" ? 500 : 10);
    }

    public async Task<decimal> GetPremiumPriceAsync()
    {
        return await _settingRepository.GetDecimalValueAsync("premium_price_usd", 9.99m);
    }

    public async Task<IEnumerable<PlatformSetting>> GetAllSettingsAsync()
    {
        return await _settingRepository.GetAllAsync();
    }

    public async Task UpdateSettingAsync(string key, string value, Guid adminId)
    {
        await _settingRepository.UpsertAsync(new PlatformSetting
        {
            Key = key,
            Value = value,
            UpdatedBy = adminId
        });
    }
}
