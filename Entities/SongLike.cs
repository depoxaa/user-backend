namespace backend.Entities;

public class SongLike : BaseEntity
{
    // Foreign keys
    public Guid UserId { get; set; }
    public Guid SongId { get; set; }
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Song Song { get; set; } = null!;
}
