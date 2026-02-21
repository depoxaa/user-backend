using backend.DTOs.Song;

namespace backend.Services.Interfaces;

public interface IPaymentService
{
    Task<SongPurchaseDto> PurchaseSongAsync(Guid userId, Guid songId, string paymentToken);
    Task<bool> HasPurchasedAsync(Guid userId, Guid songId);
    Task<IEnumerable<SongPurchaseDto>> GetUserPurchasesAsync(Guid userId);
}
