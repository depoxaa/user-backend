using backend.Entities;

namespace backend.Repositories.Interfaces;

public interface IArtistRepository : IRepository<Artist>
{
    Task<Artist?> GetByEmailAsync(string email);
    Task<Artist?> GetByNameAsync(string name);
    Task<Artist?> GetWithSongsAsync(Guid id);
    Task<Artist?> GetWithAlbumsAsync(Guid id);
    Task<IEnumerable<Artist>> GetLiveArtistsAsync();
    Task<IEnumerable<Artist>> SearchArtistsAsync(string query, int take = 20);
    Task<IEnumerable<Artist>> GetTopArtistsAsync(int take = 10);
}
