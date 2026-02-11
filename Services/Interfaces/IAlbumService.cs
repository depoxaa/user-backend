using backend.DTOs.Album;

namespace backend.Services.Interfaces;

public interface IAlbumService
{
    Task<AlbumDto?> GetByIdAsync(Guid id);
    Task<AlbumDetailDto?> GetDetailAsync(Guid id);
    Task<IEnumerable<AlbumDto>> GetByArtistAsync(Guid artistId);
    Task<AlbumDto> CreateAsync(Guid artistId, CreateAlbumDto dto, Stream? coverStream = null, string? coverFileName = null);
    Task<AlbumDto> UpdateAsync(Guid id, Guid artistId, CreateAlbumDto dto);
    Task<string?> UpdateCoverAsync(Guid id, Guid artistId, Stream fileStream, string fileName);
    Task DeleteAsync(Guid id, Guid artistId);
}
