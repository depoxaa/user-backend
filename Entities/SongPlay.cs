namespace backend.Entities;

public class SongPlay : BaseEntity
{
    public DateTime PlayedAt { get; set; } = DateTime.UtcNow;
    public int ListeningSeconds { get; set; }
    
    // Foreign keys
    public Guid UserId { get; set; }
    public Guid SongId { get; set; }
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Song Song { get; set; } = null!;
}
