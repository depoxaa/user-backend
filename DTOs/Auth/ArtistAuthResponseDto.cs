namespace backend.DTOs.Auth;

public class ArtistAuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public ArtistDto Artist { get; set; } = null!;
}

public class ArtistDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? ProfileImage { get; set; }
    public bool IsEmailConfirmed { get; set; }
    public bool IsLive { get; set; }
    public string? LiveStreamGenre { get; set; }
    public int ListenersCount { get; set; }
    public int TotalSongs { get; set; }
    public long TotalPlays { get; set; }
    public long TotalLikes { get; set; }
    public int SubscribersCount { get; set; }
}
