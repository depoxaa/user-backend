using backend.Entities;

namespace backend.Repositories.Interfaces;

public interface IPlaylistRepository : IRepository<Playlist>
{
    Task<Playlist?> GetWithSongsAsync(Guid id);
    Task<IEnumerable<Playlist>> GetByUserAsync(Guid userId);
    Task<IEnumerable<Playlist>> GetPublicPlaylistsAsync(int take = 20);
    Task<IEnumerable<Playlist>> SearchPlaylistsAsync(string query, int take = 20);
}
