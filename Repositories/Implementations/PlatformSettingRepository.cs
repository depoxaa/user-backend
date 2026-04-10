using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Entities;
using backend.Repositories.Interfaces;

namespace backend.Repositories.Implementations;

public class PlatformSettingRepository : IPlatformSettingRepository
{
    private readonly ApplicationDbContext _context;

    public PlatformSettingRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PlatformSetting?> GetByKeyAsync(string key)
    {
        return await _context.PlatformSettings.FindAsync(key);
    }

    public async Task<IEnumerable<PlatformSetting>> GetAllAsync()
    {
        return await _context.PlatformSettings.ToListAsync();
    }

    public async Task UpsertAsync(PlatformSetting setting)
    {
        var existing = await _context.PlatformSettings.FindAsync(setting.Key);
        if (existing != null)
        {
            existing.Value = setting.Value;
            existing.UpdatedBy = setting.UpdatedBy;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            setting.UpdatedAt = DateTime.UtcNow;
            await _context.PlatformSettings.AddAsync(setting);
        }
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetIntValueAsync(string key, int defaultValue)
    {
        var setting = await _context.PlatformSettings.FindAsync(key);
        return setting != null && int.TryParse(setting.Value, out var val) ? val : defaultValue;
    }

    public async Task<decimal> GetDecimalValueAsync(string key, decimal defaultValue)
    {
        var setting = await _context.PlatformSettings.FindAsync(key);
        return setting != null && decimal.TryParse(setting.Value, out var val) ? val : defaultValue;
    }
}
