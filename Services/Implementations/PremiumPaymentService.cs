using System.Security.Cryptography;
using System.Text;
using backend.DTOs.User;
using backend.Entities;
using backend.Repositories.Interfaces;
using backend.Services.Interfaces;

namespace backend.Services.Implementations;

public class PremiumPaymentService : IPremiumPaymentService
{
    private readonly IPremiumPaymentRepository _paymentRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPlatformSettingService _settingService;
    private readonly ILogger<PremiumPaymentService> _logger;

    public PremiumPaymentService(
        IPremiumPaymentRepository paymentRepository,
        IUserRepository userRepository,
        IPlatformSettingService settingService,
        ILogger<PremiumPaymentService> logger)
    {
        _paymentRepository = paymentRepository;
        _userRepository = userRepository;
        _settingService = settingService;
        _logger = logger;
    }

    public async Task<bool> UpgradeToPremiumnAsync(Guid userId, string paymentToken)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new InvalidOperationException("User not found");

        if (user.Role == "Premium")
            throw new InvalidOperationException("User is already premium");

        var price = await _settingService.GetPremiumPriceAsync();

        // TODO: Insert real Google Pay gateway token verification here.
        // In production, parse the payment token JSON, verify the signed message
        // with Google's API, and validate the amount matches.
        // See: https://developers.google.com/pay/api/web/guides/resources/payment-data-cryptography
        _logger.LogWarning("Google Pay running in TEST mode - token accepted without verification");

        // Hash the token — never store the raw token
        var tokenHash = Convert.ToBase64String(
            SHA256.HashData(Encoding.UTF8.GetBytes(paymentToken)));

        var payment = new PremiumPayment
        {
            UserId = userId,
            Provider = "google_pay",
            Amount = price,
            Currency = "USD",
            Status = "completed",
            TokenHash = tokenHash
        };

        await _paymentRepository.AddAsync(payment);

        // Upgrade user role
        user.Role = "Premium";
        await _userRepository.UpdateAsync(user);

        _logger.LogInformation("User {UserId} upgraded to Premium for {Amount} USD", userId, price);
        return true;
    }

    public async Task<SubscriptionDto> GetSubscriptionAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new InvalidOperationException("User not found");

        var payments = await _paymentRepository.GetByUserAsync(userId);

        return new SubscriptionDto
        {
            Tier = user.Role,
            ArtistVerified = user.ArtistVerified,
            PaymentHistory = payments.Select(p => new PaymentHistoryDto
            {
                Id = p.Id,
                Provider = p.Provider,
                Amount = p.Amount,
                Currency = p.Currency,
                Status = p.Status,
                CreatedAt = p.CreatedAt
            }).ToList()
        };
    }
}
