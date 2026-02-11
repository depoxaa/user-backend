using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Entities;
using backend.Repositories.Interfaces;

namespace backend.Repositories.Implementations;

public class AlbumRepository : Repository<Album>, IAlbumRepository
{
    public AlbumRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Album?> GetWithSongsAsync(Guid id)
    {
        return await _dbSet
            .Include(a => a.Songs)
            .Include(a => a.Artist)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<Album>> GetByArtistAsync(Guid artistId)
    {
        return await _dbSet
            .Include(a => a.Songs)
            .Where(a => a.ArtistId == artistId)
            .OrderByDescending(a => a.ReleaseDate)
            .ToListAsync();
    }
}
