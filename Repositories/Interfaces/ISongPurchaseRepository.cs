using backend.Entities;

namespace backend.Repositories.Interfaces;

public interface ISongPurchaseRepository : IRepository<SongPurchase>
{
    Task<bool> HasUserPurchasedSongAsync(Guid userId, Guid songId);
    Task<SongPurchase?> GetPurchaseAsync(Guid userId, Guid songId);
    Task<IEnumerable<Song>> GetPurchasedSongsByUserAsync(Guid userId);
    Task<IEnumerable<Guid>> GetPurchasedSongIdsByUserAsync(Guid userId);
    Task<decimal> GetTotalRevenueForArtistAsync(Guid artistId);
}
