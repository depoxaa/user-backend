namespace backend.Entities;

public class PlaylistSong : BaseEntity
{
    public int Order { get; set; }
    
    // Foreign keys
    public Guid PlaylistId { get; set; }
    public Guid SongId { get; set; }
    
    // Navigation properties
    public virtual Playlist Playlist { get; set; } = null!;
    public virtual Song Song { get; set; } = null!;
}
