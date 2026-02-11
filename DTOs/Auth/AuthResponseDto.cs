namespace backend.DTOs.Auth;

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserDto User { get; set; } = null!;
}

public class UserDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public bool IsEmailConfirmed { get; set; }
    public bool IsOnline { get; set; }
    public string? CurrentlyListeningStatus { get; set; }
    public int FriendsCount { get; set; }
    public int SubscribedArtistsCount { get; set; }
    public int TotalSongsInPlaylists { get; set; }
}
