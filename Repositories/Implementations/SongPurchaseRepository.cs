using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Entities;
using backend.Repositories.Interfaces;

namespace backend.Repositories.Implementations;

public class SongPurchaseRepository : Repository<SongPurchase>, ISongPurchaseRepository
{
    public SongPurchaseRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<bool> HasUserPurchasedSongAsync(Guid userId, Guid songId)
    {
        return await _dbSet.AnyAsync(sp => sp.UserId == userId && sp.SongId == songId);
    }

    public async Task<SongPurchase?> GetPurchaseAsync(Guid userId, Guid songId)
    {
        return await _dbSet.FirstOrDefaultAsync(sp => sp.UserId == userId && sp.SongId == songId);
    }

    public async Task<IEnumerable<Song>> GetPurchasedSongsByUserAsync(Guid userId)
    {
        return await _dbSet
            .Where(sp => sp.UserId == userId)
            .Include(sp => sp.Song)
                .ThenInclude(s => s.Artist)
            .Include(sp => sp.Song)
                .ThenInclude(s => s.Album)
            .Include(sp => sp.Song)
                .ThenInclude(s => s.Genre)
            .Select(sp => sp.Song)
            .ToListAsync();
    }

    public async Task<IEnumerable<Guid>> GetPurchasedSongIdsByUserAsync(Guid userId)
    {
        return await _dbSet
            .Where(sp => sp.UserId == userId)
            .Select(sp => sp.SongId)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalRevenueForArtistAsync(Guid artistId)
    {
        return await _dbSet
            .Include(sp => sp.Song)
            .Where(sp => sp.Song.ArtistId == artistId)
            .SumAsync(sp => sp.Amount);
    }
}
