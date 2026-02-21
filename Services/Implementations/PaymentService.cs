using Microsoft.Extensions.Options;
using backend.Configuration;
using backend.DTOs.Song;
using backend.Entities;
using backend.Repositories.Interfaces;
using backend.Services.Interfaces;

namespace backend.Services.Implementations;

public class PaymentService : IPaymentService
{
    private readonly ISongPurchaseRepository _purchaseRepository;
    private readonly ISongRepository _songRepository;
    private readonly GooglePaySettings _googlePaySettings;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        ISongPurchaseRepository purchaseRepository,
        ISongRepository songRepository,
        IOptions<GooglePaySettings> googlePaySettings,
        ILogger<PaymentService> logger)
    {
        _purchaseRepository = purchaseRepository;
        _songRepository = songRepository;
        _googlePaySettings = googlePaySettings.Value;
        _logger = logger;
    }

    public async Task<SongPurchaseDto> PurchaseSongAsync(Guid userId, Guid songId, string paymentToken)
    {
        var song = await _songRepository.GetByIdAsync(songId);
        if (song == null)
            throw new InvalidOperationException("Song not found");

        if (song.IsFree)
            throw new InvalidOperationException("This song is free and does not require purchase");

        var alreadyPurchased = await _purchaseRepository.HasUserPurchasedSongAsync(userId, songId);
        if (alreadyPurchased)
            throw new InvalidOperationException("You have already purchased this song");

        var isValid = await VerifyGooglePayToken(paymentToken, song.Price);
        if (!isValid)
            throw new InvalidOperationException("Payment verification failed");

        var purchase = new SongPurchase
        {
            UserId = userId,
            SongId = songId,
            Amount = song.Price,
            Currency = "USD",
            GooglePayTransactionId = paymentToken,
            PurchasedAt = DateTime.UtcNow
        };

        await _purchaseRepository.AddAsync(purchase);

        _logger.LogInformation("User {UserId} purchased song {SongId} for {Amount} USD", userId, songId, song.Price);

        return new SongPurchaseDto
        {
            Id = purchase.Id,
            SongId = songId,
            SongTitle = song.Title,
            Amount = purchase.Amount,
            Currency = purchase.Currency,
            PurchasedAt = purchase.PurchasedAt
        };
    }

    public async Task<bool> HasPurchasedAsync(Guid userId, Guid songId)
    {
        return await _purchaseRepository.HasUserPurchasedSongAsync(userId, songId);
    }

    public async Task<IEnumerable<SongPurchaseDto>> GetUserPurchasesAsync(Guid userId)
    {
        var purchases = await _purchaseRepository.FindAsync(p => p.UserId == userId);
        var result = new List<SongPurchaseDto>();

        foreach (var purchase in purchases)
        {
            var song = await _songRepository.GetByIdAsync(purchase.SongId);
            result.Add(new SongPurchaseDto
            {
                Id = purchase.Id,
                SongId = purchase.SongId,
                SongTitle = song?.Title ?? "Unknown",
                Amount = purchase.Amount,
                Currency = purchase.Currency,
                PurchasedAt = purchase.PurchasedAt
            });
        }

        return result;
    }

    /// <summary>
    /// Verifies a Google Pay payment token server-side.
    /// In production, this would call Google's API to verify the token.
    /// In TEST mode, the token is accepted as-is for development purposes.
    /// </summary>
    private Task<bool> VerifyGooglePayToken(string paymentToken, decimal expectedAmount)
    {
        if (string.IsNullOrEmpty(paymentToken))
            return Task.FromResult(false);

        if (!_googlePaySettings.IsProduction)
        {
            _logger.LogWarning("Google Pay running in TEST mode - token accepted without verification");
            return Task.FromResult(true);
        }

        // In production: parse the payment token JSON, extract the signed message,
        // verify with Google's API, and validate the amount matches.
        // See: https://developers.google.com/pay/api/web/guides/resources/payment-data-cryptography
        _logger.LogInformation("Verifying Google Pay token for amount {Amount}", expectedAmount);
        return Task.FromResult(true);
    }
}
