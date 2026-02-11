namespace backend.Entities;

public class ArtistSubscription : BaseEntity
{
    // Foreign keys
    public Guid UserId { get; set; }
    public Guid ArtistId { get; set; }
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Artist Artist { get; set; } = null!;
}
