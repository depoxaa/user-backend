namespace backend.Entities;

public class StreamViewer : BaseEntity
{
    public Guid StreamHostId { get; set; }
    public Guid UserId { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LeftAt { get; set; }

    // Navigation properties
    public virtual Artist StreamHost { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}
