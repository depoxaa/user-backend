namespace backend.DTOs.Artist;

public class WithdrawRequestDto
{
    public decimal Amount { get; set; }
    public string CardNumber { get; set; } = string.Empty;
}
