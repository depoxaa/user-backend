using backend.Entities;

namespace backend.Repositories.Interfaces;

public interface ISongRepository : IRepository<Song>
{
    Task<Song?> GetWithDetailsAsync(Guid id);
    Task<IEnumerable<Song>> GetByArtistAsync(Guid artistId);
    Task<IEnumerable<Song>> GetByAlbumAsync(Guid albumId);
    Task<IEnumerable<Song>> GetByGenreAsync(Guid genreId);
    Task<IEnumerable<Song>> SearchSongsAsync(string query, int take = 50);
    Task<IEnumerable<Song>> GetTopSongsAsync(int take = 10);
    Task<IEnumerable<Song>> GetRecentSongsAsync(int take = 20);
}
