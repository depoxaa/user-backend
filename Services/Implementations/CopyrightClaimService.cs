using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Entities;
using backend.Services.Interfaces;

namespace backend.Services.Implementations;

public class CopyrightClaimService : ICopyrightClaimService
{
    private readonly ApplicationDbContext _context;
    private readonly ISseConnectionManager _sseManager;
    private readonly IFileService _fileService;
    private readonly IEmailService _emailService;
    private readonly ILogger<CopyrightClaimService> _logger;

    public CopyrightClaimService(
        ApplicationDbContext context,
        ISseConnectionManager sseManager,
        IFileService fileService,
        IEmailService emailService,
        ILogger<CopyrightClaimService> logger)
    {
        _context = context;
        _sseManager = sseManager;
        _fileService = fileService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<CopyrightClaim> CreateClaimAsync(Guid originalSongId, Guid infringingSongId)
    {
        var originalSong = await _context.Songs
            .Include(s => s.Artist)
            .FirstOrDefaultAsync(s => s.Id == originalSongId)
            ?? throw new InvalidOperationException("Original song not found");

        var infringingSong = await _context.Songs
            .Include(s => s.Artist)
            .FirstOrDefaultAsync(s => s.Id == infringingSongId)
            ?? throw new InvalidOperationException("Infringing song not found");

        var claim = new CopyrightClaim
        {
            OriginalSongId = originalSongId,
            InfringingSongId = infringingSongId,
            OriginalArtistId = originalSong.ArtistId,
            InfringingArtistId = infringingSong.ArtistId,
            OriginalSongTitle = originalSong.Title,
            InfringingSongTitle = infringingSong.Title,
            Status = "Pending"
        };

        _context.CopyrightClaims.Add(claim);
        await _context.SaveChangesAsync();

        // Notify the original artist about the copyright match
        await _sseManager.SendEventAsync(originalSong.ArtistId, "copyrightClaim", new
        {
            claimId = claim.Id,
            action = "new",
            originalSong = new { id = originalSong.Id, title = originalSong.Title },
            infringingSong = new { id = infringingSong.Id, title = infringingSong.Title },
            infringingArtist = new { id = infringingSong.Artist.Id, name = infringingSong.Artist.Name }
        });

        _logger.LogInformation(
            "Copyright claim created: original song {OriginalId} by {OriginalArtist}, infringing song {InfringingId} by {InfringingArtist}",
            originalSongId, originalSong.Artist.Name, infringingSongId, infringingSong.Artist.Name);

        return claim;
    }

    public async Task ConfirmClaimAsync(Guid claimId, Guid artistId)
    {
        var claim = await _context.CopyrightClaims
            .Include(c => c.InfringingSong)
            .FirstOrDefaultAsync(c => c.Id == claimId)
            ?? throw new InvalidOperationException("Claim not found");

        if (claim.OriginalArtistId != artistId)
            throw new UnauthorizedAccessException("Only the original artist can confirm this claim");

        if (claim.Status != "Pending")
            throw new InvalidOperationException("Claim is already resolved");

        if (claim.InfringingSong == null || claim.InfringingSongId == null)
            throw new InvalidOperationException("Infringing song no longer exists");

        var songId = claim.InfringingSongId.Value;
        var songFilePath = claim.InfringingSong.FilePath;
        var songCoverArt = claim.InfringingSong.CoverArt;
        var infringingArtistId = claim.InfringingArtistId;

        // Update this claim and all related claims FIRST
        claim.Status = "Confirmed";
        claim.ResolvedAt = DateTime.UtcNow;

        var relatedClaims = await _context.CopyrightClaims
            .Where(c => c.InfringingSongId == songId && c.Id != claimId && c.Status == "Pending")
            .ToListAsync();
        foreach (var rc in relatedClaims)
        {
            rc.Status = "Confirmed";
            rc.ResolvedAt = DateTime.UtcNow;
        }

        // Remove song from all related tables
        _context.PlaylistSongs.RemoveRange(
            await _context.PlaylistSongs.Where(ps => ps.SongId == songId).ToListAsync());
        _context.SongLikes.RemoveRange(
            await _context.SongLikes.Where(sl => sl.SongId == songId).ToListAsync());
        _context.SongPlays.RemoveRange(
            await _context.SongPlays.Where(sp => sp.SongId == songId).ToListAsync());
        _context.SongPurchases.RemoveRange(
            await _context.SongPurchases.Where(sp => sp.SongId == songId).ToListAsync());
        _context.SongReports.RemoveRange(
            await _context.SongReports.Where(sr => sr.SongId == songId).ToListAsync());

        // Remove the song itself
        _context.Songs.Remove(claim.InfringingSong);

        await _context.SaveChangesAsync();

        // Delete files from storage (after DB commit)
        if (!string.IsNullOrEmpty(songFilePath))
        {
            try { await _fileService.DeleteFileAsync(songFilePath); }
            catch (Exception ex) { _logger.LogWarning(ex, "Failed to delete song file"); }
        }
        if (!string.IsNullOrEmpty(songCoverArt))
        {
            try { await _fileService.DeleteFileAsync(songCoverArt); }
            catch (Exception ex) { _logger.LogWarning(ex, "Failed to delete cover file"); }
        }

        // Send copyright ban email to the infringing artist
        var infringingArtist = await _context.Artists.FindAsync(infringingArtistId);
        if (infringingArtist != null)
        {
            try
            {
                var subject = "Aganim - Your song has been removed (Copyright Violation)";
                var body = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif; background-color: #1e293b; color: white; padding: 20px;'>
                        <div style='max-width: 600px; margin: 0 auto; background-color: #0f172a; border-radius: 10px; padding: 30px;'>
                            <h1 style='color: #ef4444; text-align: center;'>Copyright Violation Notice</h1>
                            <p style='font-size: 16px; text-align: center;'>Your song has been removed from the platform.</p>
                            <div style='background-color: #1e293b; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                                <p style='margin: 8px 0;'><strong style='color: #94a3b8;'>Removed song:</strong> <span style='color: #fca5a5;'>{claim.InfringingSongTitle}</span></p>
                                <p style='margin: 8px 0;'><strong style='color: #94a3b8;'>Matches original:</strong> <span style='color: #86efac;'>{claim.OriginalSongTitle}</span></p>
                            </div>
                            <p style='font-size: 14px; color: #94a3b8; text-align: center;'>
                                This song was identified as a duplicate of an existing track on Aganim.
                                The original artist has confirmed the copyright claim and your song has been permanently deleted.
                            </p>
                            <div style='background-color: rgba(239, 68, 68, 0.1); border: 1px solid rgba(239, 68, 68, 0.3); border-radius: 8px; padding: 16px; margin-top: 20px;'>
                                <p style='font-size: 14px; color: #fca5a5; margin: 0; text-align: center;'>
                                    Uploading copyrighted content violates our platform rules. Repeated violations may result in account suspension.
                                </p>
                            </div>
                        </div>
                    </body>
                    </html>";

                await _emailService.SendEmailAsync(infringingArtist.Email, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send copyright ban email to artist {ArtistId}", infringingArtistId);
            }
        }

        _logger.LogInformation("Copyright claim {ClaimId} confirmed. Song {SongId} removed from all tables.", claimId, songId);
    }

    public async Task DismissClaimAsync(Guid claimId, Guid artistId)
    {
        var claim = await _context.CopyrightClaims
            .FirstOrDefaultAsync(c => c.Id == claimId)
            ?? throw new InvalidOperationException("Claim not found");

        if (claim.OriginalArtistId != artistId)
            throw new UnauthorizedAccessException("Only the original artist can dismiss this claim");

        if (claim.Status != "Pending")
            throw new InvalidOperationException("Claim is already resolved");

        claim.Status = "Dismissed";
        claim.ResolvedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Copyright claim {ClaimId} dismissed by artist {ArtistId}", claimId, artistId);
    }

    public async Task<IEnumerable<CopyrightClaim>> GetPendingClaimsForArtistAsync(Guid artistId)
    {
        return await _context.CopyrightClaims
            .Include(c => c.InfringingArtist)
            .Where(c => c.OriginalArtistId == artistId && c.Status == "Pending" && c.InfringingSongId != null)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<CopyrightClaim>> GetClaimHistoryForArtistAsync(Guid artistId)
    {
        return await _context.CopyrightClaims
            .Include(c => c.InfringingArtist)
            .Where(c => c.OriginalArtistId == artistId && c.Status != "Pending")
            .OrderByDescending(c => c.ResolvedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<CopyrightClaim>> GetBannedSongsForArtistAsync(Guid artistId)
    {
        // Songs that were banned because this artist uploaded copyrighted content
        return await _context.CopyrightClaims
            .Include(c => c.OriginalArtist)
            .Where(c => c.InfringingArtistId == artistId && c.Status == "Confirmed")
            .OrderByDescending(c => c.ResolvedAt)
            .ToListAsync();
    }
}
