using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Entities;
using backend.Repositories.Interfaces;

namespace backend.Repositories.Implementations;

public class PlaylistSongRepository : Repository<PlaylistSong>, IPlaylistSongRepository
{
    public PlaylistSongRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<PlaylistSong>> GetByPlaylistAsync(Guid playlistId)
    {
        return await _dbSet
            .Include(ps => ps.Song)
                .ThenInclude(s => s.Artist)
            .Where(ps => ps.PlaylistId == playlistId)
            .OrderBy(ps => ps.Order)
            .ToListAsync();
    }

    public async Task<int> GetMaxOrderAsync(Guid playlistId)
    {
        var maxOrder = await _dbSet
            .Where(ps => ps.PlaylistId == playlistId)
            .MaxAsync(ps => (int?)ps.Order);
        return maxOrder ?? 0;
    }

    public async Task<bool> IsSongInPlaylistAsync(Guid playlistId, Guid songId)
    {
        return await _dbSet.AnyAsync(ps => ps.PlaylistId == playlistId && ps.SongId == songId);
    }
}
