namespace backend.DTOs.Friend;

public class FriendDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsOnline { get; set; }
    public int PlaylistsCount { get; set; }
}

public class FriendRequestDto
{
    public Guid Id { get; set; }
    public UserInfoForFriendDto From { get; set; } = null!;
    public string Timestamp { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class UserInfoForFriendDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Avatar { get; set; }
}
