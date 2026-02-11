using AutoMapper;
using backend.DTOs.Auth;
using backend.DTOs.User;
using backend.Repositories.Interfaces;
using backend.Services.Interfaces;

namespace backend.Services.Implementations;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ISongRepository _songRepository;
    private readonly IFileService _fileService;
    private readonly IMapper _mapper;
    private readonly ISseConnectionManager _sseManager;
    private readonly IFriendshipRepository _friendshipRepository;

    public UserService(
        IUserRepository userRepository,
        ISongRepository songRepository,
        IFileService fileService,
        IMapper mapper,
        ISseConnectionManager sseManager,
        IFriendshipRepository friendshipRepository)
    {
        _userRepository = userRepository;
        _songRepository = songRepository;
        _fileService = fileService;
        _mapper = mapper;
        _sseManager = sseManager;
        _friendshipRepository = friendshipRepository;
    }

    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        var user = await _userRepository.GetWithPlaylistsAsync(id);
        return user == null ? null : _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto?> GetByUsernameAsync(string username)
    {
        var user = await _userRepository.GetByUsernameWithDetailsAsync(username);
        return user == null ? null : _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto> UpdateAsync(Guid id, UpdateUserDto dto)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        if (!string.IsNullOrEmpty(dto.Username) && dto.Username != user.Username)
        {
            var existingUser = await _userRepository.GetByUsernameAsync(dto.Username);
            if (existingUser != null)
            {
                throw new InvalidOperationException("Username already exists");
            }
            user.Username = dto.Username;
        }

        if (!string.IsNullOrEmpty(dto.Email) && dto.Email != user.Email)
        {
            var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("Email already exists");
            }
            user.Email = dto.Email;
            user.IsEmailConfirmed = false;
        }

        if (dto.CurrentlyListeningStatus != null)
        {
            user.CurrentlyListeningStatus = dto.CurrentlyListeningStatus;
        }

        await _userRepository.UpdateAsync(user);
        return _mapper.Map<UserDto>(user);
    }

    public async Task<bool> ChangePasswordAsync(Guid id, ChangePasswordDto dto)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
        {
            throw new InvalidOperationException("Current password is incorrect");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _userRepository.UpdateAsync(user);

        return true;
    }

    public async Task<string?> UpdateAvatarAsync(Guid id, Stream fileStream, string fileName)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        // Delete old avatar if exists
        if (!string.IsNullOrEmpty(user.Avatar))
        {
            await _fileService.DeleteFileAsync(user.Avatar);
        }

        var filePath = await _fileService.SaveImageFileAsync(fileStream, fileName, "avatars");
        user.Avatar = filePath;
        await _userRepository.UpdateAsync(user);

        return filePath;
    }

    public async Task<IEnumerable<UserDto>> SearchUsersAsync(string query, Guid currentUserId)
    {
        var users = await _userRepository.SearchUsersAsync(query);
        return users.Where(u => u.Id != currentUserId).Select(u => _mapper.Map<UserDto>(u));
    }

    public async Task SetOnlineStatusAsync(Guid id, bool isOnline)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return;

        user.IsOnline = isOnline;
        user.LastSeen = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);
    }

    public async Task UpdateListeningStatusAsync(Guid id, string? status)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return;

        var wasLive = !string.IsNullOrEmpty(user.CurrentlyListeningStatus) && 
                      user.CurrentlyListeningStatus.Contains("LIVE");
        var isLive = !string.IsNullOrEmpty(status) && status.Contains("LIVE");

        user.CurrentlyListeningStatus = status;
        
        // Clear playback info if going offline (stopping live)
        if (string.IsNullOrEmpty(status) || !status.Contains("LIVE"))
        {
            user.CurrentSongId = null;
            user.CurrentSongPosition = 0;
            user.CurrentSongUpdatedAt = null;
        }
        
        await _userRepository.UpdateAsync(user);

        // Notify friends about live status change via SSE
        if (wasLive != isLive)
        {
            var friends = await _friendshipRepository.GetFriendsAsync(id);
            var friendIds = friends.Select(f => f.Id);
            await _sseManager.SendEventToUsersAsync(friendIds, "liveUsers", new 
            { 
                userId = id, 
                username = user.Username,
                action = isLive ? "started" : "stopped",
                genre = status 
            });
        }
    }

    public async Task UpdatePlaybackAsync(Guid id, UpdatePlaybackDto dto)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return;

        user.CurrentSongId = dto.SongId;
        user.CurrentSongPosition = dto.Position;
        user.CurrentSongUpdatedAt = DateTime.UtcNow;
        user.IsPlaybackPaused = dto.IsPaused;
        await _userRepository.UpdateAsync(user);
    }

    public async Task<LivePlaybackDto?> GetLivePlaybackAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return null;

        var isLive = !string.IsNullOrEmpty(user.CurrentlyListeningStatus) && 
                     user.CurrentlyListeningStatus.Contains("LIVE");

        var result = new LivePlaybackDto
        {
            UserId = user.Id,
            Username = user.Username,
            IsLive = isLive,
            SongId = user.CurrentSongId,
            Position = user.CurrentSongPosition,
            UpdatedAt = user.CurrentSongUpdatedAt,
            IsPaused = user.IsPlaybackPaused
        };

        // Get song info if there's a current song
        if (user.CurrentSongId.HasValue)
        {
            var song = await _songRepository.GetWithDetailsAsync(user.CurrentSongId.Value);
            if (song != null)
            {
                result.SongTitle = song.Title;
                result.SongArtist = song.Artist?.Name;
                result.SongCoverArt = song.CoverArt;
            }
        }

        return result;
    }
}

