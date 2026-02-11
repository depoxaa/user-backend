using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Entities;
using backend.Repositories.Interfaces;

namespace backend.Repositories.Implementations;

public class PlaylistRepository : Repository<Playlist>, IPlaylistRepository
{
    public PlaylistRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Playlist?> GetWithSongsAsync(Guid id)
    {
        return await _dbSet
            .Include(p => p.PlaylistSongs.OrderBy(ps => ps.Order))
                .ThenInclude(ps => ps.Song)
                    .ThenInclude(s => s.Artist)
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Playlist>> GetByUserAsync(Guid userId)
    {
        return await _dbSet
            .Include(p => p.PlaylistSongs)
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Playlist>> GetPublicPlaylistsAsync(int take = 20)
    {
        return await _dbSet
            .Include(p => p.User)
            .Include(p => p.PlaylistSongs)
            .Where(p => p.Status == PlaylistStatus.Public)
            .OrderByDescending(p => p.ViewCount)
            .Take(take)
            .ToListAsync();
    }

    public async Task<IEnumerable<Playlist>> SearchPlaylistsAsync(string query, int take = 20)
    {
        var lowerQuery = query.ToLower();
        return await _dbSet
            .Include(p => p.User)
            .Include(p => p.PlaylistSongs)
            .Where(p => p.Status == PlaylistStatus.Public && 
                       p.Name.ToLower().Contains(lowerQuery))
            .Take(take)
            .ToListAsync();
    }
}
