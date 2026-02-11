using AutoMapper;
using backend.DTOs.Auth;
using backend.Entities;
using backend.Repositories.Interfaces;
using backend.Services.Interfaces;

namespace backend.Services.Implementations;

public class SubscriptionService : ISubscriptionService
{
    private readonly IArtistSubscriptionRepository _subscriptionRepository;
    private readonly IArtistRepository _artistRepository;
    private readonly IMapper _mapper;

    public SubscriptionService(
        IArtistSubscriptionRepository subscriptionRepository,
        IArtistRepository artistRepository,
        IMapper mapper)
    {
        _subscriptionRepository = subscriptionRepository;
        _artistRepository = artistRepository;
        _mapper = mapper;
    }

    public async Task<bool> ToggleSubscriptionAsync(Guid userId, Guid artistId)
    {
        var artist = await _artistRepository.GetByIdAsync(artistId);
        if (artist == null)
        {
            throw new InvalidOperationException("Artist not found");
        }

        var existingSub = await _subscriptionRepository.GetSubscriptionAsync(userId, artistId);
        
        if (existingSub != null)
        {
            await _subscriptionRepository.DeleteAsync(existingSub);
            return false;
        }
        else
        {
            var subscription = new ArtistSubscription
            {
                UserId = userId,
                ArtistId = artistId
            };
            await _subscriptionRepository.AddAsync(subscription);
            return true;
        }
    }

    public async Task<bool> IsSubscribedAsync(Guid userId, Guid artistId)
    {
        return await _subscriptionRepository.IsUserSubscribedAsync(userId, artistId);
    }

    public async Task<IEnumerable<ArtistDto>> GetSubscribedArtistsAsync(Guid userId)
    {
        var artists = await _subscriptionRepository.GetSubscribedArtistsAsync(userId);
        return _mapper.Map<IEnumerable<ArtistDto>>(artists);
    }

    public async Task<int> GetSubscriberCountAsync(Guid artistId)
    {
        return await _subscriptionRepository.GetSubscriberCountAsync(artistId);
    }
}
