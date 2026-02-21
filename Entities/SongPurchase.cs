namespace backend.Entities;

public class SongPurchase : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid SongId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string? GooglePayTransactionId { get; set; }
    public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Song Song { get; set; } = null!;
}
