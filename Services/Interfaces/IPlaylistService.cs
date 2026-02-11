using backend.DTOs.Playlist;

namespace backend.Services.Interfaces;

public interface IPlaylistService
{
    Task<PlaylistDto?> GetByIdAsync(Guid id);
    Task<PlaylistDetailDto?> GetDetailAsync(Guid id, Guid? viewerId = null);
    Task<IEnumerable<PlaylistDto>> GetByUserAsync(Guid userId, Guid? viewerId = null);
    Task<IEnumerable<PlaylistDto>> GetPublicPlaylistsAsync(int take = 20);
    Task<IEnumerable<PlaylistDto>> SearchAsync(string query);
    Task<PlaylistDto> CreateAsync(Guid userId, CreatePlaylistDto dto);
    Task<PlaylistDto> UpdateAsync(Guid id, Guid userId, UpdatePlaylistDto dto);
    Task<PlaylistDto> UpdateCoverAsync(Guid id, Guid userId, string coverImage);
    Task DeleteAsync(Guid id, Guid userId);
    Task AddSongAsync(Guid playlistId, Guid userId, Guid songId);
    Task RemoveSongAsync(Guid playlistId, Guid userId, Guid songId);
    Task ReorderSongsAsync(Guid playlistId, Guid userId, List<Guid> songIds);
}
