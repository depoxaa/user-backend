using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Entities;
using backend.Repositories.Interfaces;

namespace backend.Repositories.Implementations;

public class SongLikeRepository : Repository<SongLike>, ISongLikeRepository
{
    public SongLikeRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<bool> HasUserLikedSongAsync(Guid userId, Guid songId)
    {
        return await _dbSet.AnyAsync(sl => sl.UserId == userId && sl.SongId == songId);
    }

    public async Task<SongLike?> GetLikeAsync(Guid userId, Guid songId)
    {
        return await _dbSet.FirstOrDefaultAsync(sl => sl.UserId == userId && sl.SongId == songId);
    }

    public async Task<IEnumerable<Song>> GetLikedSongsByUserAsync(Guid userId)
    {
        return await _dbSet
            .Include(sl => sl.Song)
                .ThenInclude(s => s.Artist)
            .Where(sl => sl.UserId == userId)
            .Select(sl => sl.Song)
            .ToListAsync();
    }
}
