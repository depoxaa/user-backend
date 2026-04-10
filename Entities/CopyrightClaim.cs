namespace backend.Entities;

public class CopyrightClaim : BaseEntity
{
    public Guid? OriginalSongId { get; set; }
    public Guid? InfringingSongId { get; set; }
    public Guid OriginalArtistId { get; set; }
    public Guid InfringingArtistId { get; set; }
    public string InfringingSongTitle { get; set; } = string.Empty;
    public string OriginalSongTitle { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending"; // Pending, Confirmed, Dismissed
    public DateTime? ResolvedAt { get; set; }

    // Navigation properties
    public virtual Song? OriginalSong { get; set; }
    public virtual Song? InfringingSong { get; set; }
    public virtual Artist OriginalArtist { get; set; } = null!;
    public virtual Artist InfringingArtist { get; set; } = null!;
}
