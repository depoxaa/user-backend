using backend.Entities;

namespace backend.Repositories.Interfaces;

public interface IPlatformSettingRepository
{
    Task<PlatformSetting?> GetByKeyAsync(string key);
    Task<IEnumerable<PlatformSetting>> GetAllAsync();
    Task UpsertAsync(PlatformSetting setting);
    Task<int> GetIntValueAsync(string key, int defaultValue);
    Task<decimal> GetDecimalValueAsync(string key, decimal defaultValue);
}
