using backend.DTOs.Auth;
using backend.DTOs.Artist;

namespace backend.Services.Interfaces;

public interface IArtistService
{
    Task<ArtistDto?> GetByIdAsync(Guid id);
    Task<ArtistDto> UpdateAsync(Guid id, UpdateArtistDto dto);
    Task<string?> UpdateProfileImageAsync(Guid id, Stream fileStream, string fileName);
    Task<ArtistStatsDto> GetStatsAsync(Guid id);
    Task<IEnumerable<ArtistDto>> GetLiveArtistsAsync();
    Task<IEnumerable<ArtistDto>> GetTopArtistsAsync(int take = 10);
    Task<IEnumerable<ArtistDto>> SearchArtistsAsync(string query);
    Task StartLiveStreamAsync(Guid id, string genre);
    Task StopLiveStreamAsync(Guid id);
    Task UpdateListenerCountAsync(Guid id, int count);
    Task ChangePasswordAsync(Guid id, string currentPassword, string newPassword);
}
