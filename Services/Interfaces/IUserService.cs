using backend.DTOs.Auth;
using backend.DTOs.User;

namespace backend.Services.Interfaces;

public interface IUserService
{
    Task<UserDto?> GetByIdAsync(Guid id);
    Task<UserDto?> GetByUsernameAsync(string username);
    Task<UserDto> UpdateAsync(Guid id, UpdateUserDto dto);
    Task<bool> ChangePasswordAsync(Guid id, ChangePasswordDto dto);
    Task<string?> UpdateAvatarAsync(Guid id, Stream fileStream, string fileName);
    Task<IEnumerable<UserDto>> SearchUsersAsync(string query, Guid currentUserId);
    Task SetOnlineStatusAsync(Guid id, bool isOnline);
    Task UpdateListeningStatusAsync(Guid id, string? status);
    Task UpdatePlaybackAsync(Guid id, UpdatePlaybackDto dto);
    Task<LivePlaybackDto?> GetLivePlaybackAsync(Guid userId);
}

