namespace backend.Entities;

public class ArtistWithdrawal : BaseEntity
{
    public Guid ArtistId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string CardNumber { get; set; } = string.Empty; // Masked: **** **** **** 1234
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, Completed
    public Guid? ReviewedByAdminId { get; set; }
    public DateTime? ReviewedAt { get; set; }

    // Navigation properties
    public virtual Artist Artist { get; set; } = null!;
}
