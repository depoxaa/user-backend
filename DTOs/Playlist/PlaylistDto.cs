namespace backend.DTOs.Playlist;

public class PlaylistDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Icon { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string? CoverImage { get; set; }
    public string Status { get; set; } = string.Empty;
    public int TracksCount { get; set; }
    public long ViewCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public UserInfoDto? Owner { get; set; }
}

public class PlaylistDetailDto : PlaylistDto
{
    public List<PlaylistSongDto> Songs { get; set; } = new();
    public List<PlaylistViewerDto> RecentViewers { get; set; } = new();
}

public class PlaylistSongDto
{
    public int Order { get; set; }
    public Guid SongId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public string? CoverArt { get; set; }
    public bool IsLiked { get; set; }
}

public class PlaylistViewerDto
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public string LastViewed { get; set; } = string.Empty;
}

public class UserInfoDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Avatar { get; set; }
}
