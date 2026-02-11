namespace backend.Entities;

public enum FriendRequestStatus
{
    Pending,
    Accepted,
    Rejected
}

public class FriendRequest : BaseEntity
{
    public FriendRequestStatus Status { get; set; } = FriendRequestStatus.Pending;
    
    // Foreign keys
    public Guid SenderId { get; set; }
    public Guid ReceiverId { get; set; }
    
    // Navigation properties
    public virtual User Sender { get; set; } = null!;
    public virtual User Receiver { get; set; } = null!;
}
