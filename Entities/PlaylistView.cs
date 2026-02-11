namespace backend.Entities;

public class PlaylistView : BaseEntity
{
    public DateTime ViewedAt { get; set; } = DateTime.UtcNow;
    
    // Foreign keys
    public Guid PlaylistId { get; set; }
    public Guid UserId { get; set; }
    
    // Navigation properties
    public virtual Playlist Playlist { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}
