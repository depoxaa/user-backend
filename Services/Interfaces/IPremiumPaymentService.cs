using backend.DTOs.User;

namespace backend.Services.Interfaces;

public interface IPremiumPaymentService
{
    Task<bool> UpgradeToPremiumnAsync(Guid userId, string paymentToken);
    Task<SubscriptionDto> GetSubscriptionAsync(Guid userId);
}
