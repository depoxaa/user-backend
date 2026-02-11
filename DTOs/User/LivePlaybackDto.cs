namespace backend.DTOs.User;

public class LivePlaybackDto
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public bool IsLive { get; set; }
    public Guid? SongId { get; set; }
    public string? SongTitle { get; set; }
    public string? SongArtist { get; set; }
    public string? SongCoverArt { get; set; }
    public double Position { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsPaused { get; set; }
}
