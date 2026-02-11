using backend.DTOs.Song;

namespace backend.Services.Interfaces;

public interface ISongService
{
    Task<SongDto?> GetByIdAsync(Guid id, Guid? userId = null);
    Task<IEnumerable<SongDto>> GetByArtistAsync(Guid artistId, Guid? userId = null);
    Task<IEnumerable<SongDto>> GetByAlbumAsync(Guid albumId, Guid? userId = null);
    Task<IEnumerable<SongDto>> GetByGenreAsync(Guid genreId, Guid? userId = null);
    Task<IEnumerable<SongDto>> SearchAsync(string query, Guid? userId = null);
    Task<IEnumerable<SongDto>> GetTopSongsAsync(int take = 10, Guid? userId = null);
    Task<IEnumerable<SongDto>> GetRecentSongsAsync(int take = 20, Guid? userId = null);
    Task<SongDto> CreateAsync(Guid artistId, CreateSongDto dto, Stream audioStream, string audioFileName, Stream? coverStream = null, string? coverFileName = null);
    Task<SongDto> UpdateAsync(Guid id, Guid artistId, UpdateSongDto dto);
    Task<string?> UpdateCoverAsync(Guid id, Guid artistId, Stream fileStream, string fileName);
    Task DeleteAsync(Guid id, Guid artistId);
    Task<bool> ToggleLikeAsync(Guid songId, Guid userId);
    Task RecordPlayAsync(Guid songId, Guid userId, int listeningSeconds);
    Task<IEnumerable<SongDto>> GetLikedSongsAsync(Guid userId);
    Task<Stream?> GetAudioStreamAsync(Guid songId);
}
