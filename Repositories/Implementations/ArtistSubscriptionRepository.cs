using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Entities;
using backend.Repositories.Interfaces;

namespace backend.Repositories.Implementations;

public class ArtistSubscriptionRepository : Repository<ArtistSubscription>, IArtistSubscriptionRepository
{
    public ArtistSubscriptionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<bool> IsUserSubscribedAsync(Guid userId, Guid artistId)
    {
        return await _dbSet.AnyAsync(sub => sub.UserId == userId && sub.ArtistId == artistId);
    }

    public async Task<ArtistSubscription?> GetSubscriptionAsync(Guid userId, Guid artistId)
    {
        return await _dbSet.FirstOrDefaultAsync(sub => sub.UserId == userId && sub.ArtistId == artistId);
    }

    public async Task<IEnumerable<Artist>> GetSubscribedArtistsAsync(Guid userId)
    {
        return await _dbSet
            .Include(sub => sub.Artist)
                .ThenInclude(a => a.Subscribers)
            .Where(sub => sub.UserId == userId)
            .Select(sub => sub.Artist)
            .ToListAsync();
    }

    public async Task<int> GetSubscriberCountAsync(Guid artistId)
    {
        return await _dbSet.CountAsync(sub => sub.ArtistId == artistId);
    }
}
