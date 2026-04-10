namespace backend.Entities;

public class PremiumPayment : BaseEntity
{
    public Guid UserId { get; set; }
    public string Provider { get; set; } = "google_pay";
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string Status { get; set; } = "completed";
    public string TokenHash { get; set; } = string.Empty;

    // Navigation properties
    public virtual User User { get; set; } = null!;
}
