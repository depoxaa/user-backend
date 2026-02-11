namespace backend.Entities;

public enum PlaylistStatus
{
    Public,
    Private
}

public class Playlist : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Icon { get; set; } = "🎵";
    public string Color { get; set; } = "bg-blue-500";
    public string? CoverImage { get; set; }
    public PlaylistStatus Status { get; set; } = PlaylistStatus.Public;
    public long ViewCount { get; set; }
    
    // Foreign keys
    public Guid UserId { get; set; }
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual ICollection<PlaylistSong> PlaylistSongs { get; set; } = new List<PlaylistSong>();
    public virtual ICollection<PlaylistView> Views { get; set; } = new List<PlaylistView>();
}
