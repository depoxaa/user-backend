using backend.DTOs.Song;

namespace backend.Services.Interfaces;

public interface IRecommendationService
{
    Task<IEnumerable<SongDto>> GetRecommendationsAsync(Guid userId, int limit = 20, int offset = 0);
    Task<int> GetPlayEventCountAsync(Guid userId);
}
