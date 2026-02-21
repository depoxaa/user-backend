namespace backend.Configuration;

public class GooglePaySettings
{
    public string MerchantId { get; set; } = string.Empty;
    public string MerchantName { get; set; } = string.Empty;
    public string Environment { get; set; } = "TEST";
    public bool IsProduction { get; set; } = false;
}
