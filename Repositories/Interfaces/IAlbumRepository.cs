using backend.Entities;

namespace backend.Repositories.Interfaces;

public interface IAlbumRepository : IRepository<Album>
{
    Task<Album?> GetWithSongsAsync(Guid id);
    Task<IEnumerable<Album>> GetByArtistAsync(Guid artistId);
}
