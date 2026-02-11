using backend.Entities;

namespace backend.Repositories.Interfaces;

public interface IPlaylistSongRepository : IRepository<PlaylistSong>
{
    Task<IEnumerable<PlaylistSong>> GetByPlaylistAsync(Guid playlistId);
    Task<int> GetMaxOrderAsync(Guid playlistId);
    Task<bool> IsSongInPlaylistAsync(Guid playlistId, Guid songId);
}
