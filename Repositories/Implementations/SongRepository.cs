using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Entities;
using backend.Repositories.Interfaces;

namespace backend.Repositories.Implementations;

public class SongRepository : Repository<Song>, ISongRepository
{
    public SongRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Song?> GetWithDetailsAsync(Guid id)
    {
        return await _dbSet
            .Include(s => s.Artist)
            .Include(s => s.Album)
            .Include(s => s.Genre)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IEnumerable<Song>> GetByArtistAsync(Guid artistId)
    {
        return await _dbSet
            .Include(s => s.Artist)
            .Include(s => s.Album)
            .Include(s => s.Genre)
            .Where(s => s.ArtistId == artistId)
            .OrderByDescending(s => s.ReleaseDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Song>> GetByAlbumAsync(Guid albumId)
    {
        return await _dbSet
            .Include(s => s.Artist)
            .Include(s => s.Genre)
            .Where(s => s.AlbumId == albumId)
            .OrderBy(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Song>> GetByGenreAsync(Guid genreId)
    {
        return await _dbSet
            .Include(s => s.Artist)
            .Include(s => s.Album)
            .Where(s => s.GenreId == genreId)
            .OrderByDescending(s => s.TotalPlays)
            .ToListAsync();
    }

    public async Task<IEnumerable<Song>> SearchSongsAsync(string query, int take = 50)
    {
        var lowerQuery = query.ToLower();
        return await _dbSet
            .Include(s => s.Artist)
            .Include(s => s.Album)
            .Include(s => s.Genre)
            .Where(s => s.Title.ToLower().Contains(lowerQuery) || 
                       s.Artist.Name.ToLower().Contains(lowerQuery))
            .Take(take)
            .ToListAsync();
    }

    public async Task<IEnumerable<Song>> GetTopSongsAsync(int take = 10)
    {
        return await _dbSet
            .Include(s => s.Artist)
            .Include(s => s.Album)
            .Include(s => s.Genre)
            .OrderByDescending(s => s.TotalPlays)
            .Take(take)
            .ToListAsync();
    }

    public async Task<IEnumerable<Song>> GetRecentSongsAsync(int take = 20)
    {
        return await _dbSet
            .Include(s => s.Artist)
            .Include(s => s.Album)
            .Include(s => s.Genre)
            .OrderByDescending(s => s.ReleaseDate)
            .Take(take)
            .ToListAsync();
    }
}
