namespace backend.Entities;

public class Friendship : BaseEntity
{
    // Foreign keys
    public Guid UserId { get; set; }
    public Guid FriendId { get; set; }
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual User Friend { get; set; } = null!;
}
