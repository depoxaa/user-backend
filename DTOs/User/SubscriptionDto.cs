namespace backend.DTOs.User;

public class SubscriptionDto
{
    public string Tier { get; set; } = string.Empty; // User, Premium, Artist
    public string ArtistVerified { get; set; } = "none";
    public List<PaymentHistoryDto> PaymentHistory { get; set; } = new();
}

public class PaymentHistoryDto
{
    public Guid Id { get; set; }
    public string Provider { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class GooglePayUpgradeDto
{
    public string PaymentToken { get; set; } = string.Empty;
}
