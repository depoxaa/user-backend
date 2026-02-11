using backend.Entities;

namespace backend.Repositories.Interfaces;

public interface IArtistSubscriptionRepository : IRepository<ArtistSubscription>
{
    Task<bool> IsUserSubscribedAsync(Guid userId, Guid artistId);
    Task<ArtistSubscription?> GetSubscriptionAsync(Guid userId, Guid artistId);
    Task<IEnumerable<Artist>> GetSubscribedArtistsAsync(Guid userId);
    Task<int> GetSubscriberCountAsync(Guid artistId);
}
