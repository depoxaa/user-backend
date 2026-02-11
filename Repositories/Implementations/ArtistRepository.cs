using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Entities;
using backend.Repositories.Interfaces;

namespace backend.Repositories.Implementations;

public class ArtistRepository : Repository<Artist>, IArtistRepository
{
    public ArtistRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Artist?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(a => a.Email.ToLower() == email.ToLower());
    }

    public async Task<Artist?> GetByNameAsync(string name)
    {
        return await _dbSet.FirstOrDefaultAsync(a => a.Name.ToLower() == name.ToLower());
    }

    public async Task<Artist?> GetWithSongsAsync(Guid id)
    {
        return await _dbSet
            .Include(a => a.Songs)
                .ThenInclude(s => s.Genre)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Artist?> GetWithAlbumsAsync(Guid id)
    {
        return await _dbSet
            .Include(a => a.Albums)
                .ThenInclude(al => al.Songs)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<Artist>> GetLiveArtistsAsync()
    {
        return await _dbSet
            .Where(a => a.IsLive)
            .OrderByDescending(a => a.ListenersCount)
            .ToListAsync();
    }

    public async Task<IEnumerable<Artist>> SearchArtistsAsync(string query, int take = 20)
    {
        var lowerQuery = query.ToLower();
        return await _dbSet
            .Where(a => a.Name.ToLower().Contains(lowerQuery))
            .Take(take)
            .ToListAsync();
    }

    public async Task<IEnumerable<Artist>> GetTopArtistsAsync(int take = 10)
    {
        return await _dbSet
            .Include(a => a.Subscribers)
            .OrderByDescending(a => a.Subscribers.Count)
            .Take(take)
            .ToListAsync();
    }
}
