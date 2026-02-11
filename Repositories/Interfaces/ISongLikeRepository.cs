using backend.Entities;

namespace backend.Repositories.Interfaces;

public interface ISongLikeRepository : IRepository<SongLike>
{
    Task<bool> HasUserLikedSongAsync(Guid userId, Guid songId);
    Task<SongLike?> GetLikeAsync(Guid userId, Guid songId);
    Task<IEnumerable<Song>> GetLikedSongsByUserAsync(Guid userId);
}
