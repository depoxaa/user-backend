using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Song;

public class PurchaseSongDto
{
    [Required]
    public string PaymentToken { get; set; } = string.Empty;
}

public class SongPurchaseDto
{
    public Guid Id { get; set; }
    public Guid SongId { get; set; }
    public string SongTitle { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime PurchasedAt { get; set; }
}
