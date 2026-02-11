using backend.DTOs.Auth;

namespace backend.Services.Interfaces;

public interface ISubscriptionService
{
    Task<bool> ToggleSubscriptionAsync(Guid userId, Guid artistId);
    Task<bool> IsSubscribedAsync(Guid userId, Guid artistId);
    Task<IEnumerable<ArtistDto>> GetSubscribedArtistsAsync(Guid userId);
    Task<int> GetSubscriberCountAsync(Guid artistId);
}
