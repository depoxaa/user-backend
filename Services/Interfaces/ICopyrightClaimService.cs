using backend.Entities;

namespace backend.Services.Interfaces;

public interface ICopyrightClaimService
{
    Task<CopyrightClaim> CreateClaimAsync(Guid originalSongId, Guid infringingSongId);
    Task ConfirmClaimAsync(Guid claimId, Guid artistId);
    Task DismissClaimAsync(Guid claimId, Guid artistId);
    Task<IEnumerable<CopyrightClaim>> GetPendingClaimsForArtistAsync(Guid artistId);
    Task<IEnumerable<CopyrightClaim>> GetClaimHistoryForArtistAsync(Guid artistId);
    Task<IEnumerable<CopyrightClaim>> GetBannedSongsForArtistAsync(Guid artistId);
}
