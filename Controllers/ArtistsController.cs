using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.DTOs.Auth;
using backend.DTOs.Artist;
using backend.DTOs.Common;
using backend.DTOs.User;
using backend.Entities;
using backend.Services.Interfaces;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArtistsController : ControllerBase
{
    private readonly IArtistService _artistService;
    private readonly ISubscriptionService _subscriptionService;
    private readonly ICopyrightClaimService _copyrightClaimService;
    private readonly ApplicationDbContext _context;

    public ArtistsController(IArtistService artistService, ISubscriptionService subscriptionService, ICopyrightClaimService copyrightClaimService, ApplicationDbContext context)
    {
        _artistService = artistService;
        _subscriptionService = subscriptionService;
        _copyrightClaimService = copyrightClaimService;
        _context = context;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ArtistDto>>> GetById(Guid id)
    {
        var artist = await _artistService.GetByIdAsync(id);
        
        if (artist == null)
            return NotFound(ApiResponse<ArtistDto>.ErrorResponse("Artist not found"));
            
        return Ok(ApiResponse<ArtistDto>.SuccessResponse(artist));
    }

    [HttpGet("me")]
    [Authorize(Roles = "Artist")]
    public async Task<ActionResult<ApiResponse<ArtistDto>>> GetCurrentArtist()
    {
        var artistId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var artist = await _artistService.GetByIdAsync(artistId);
        
        if (artist == null)
            return NotFound(ApiResponse<ArtistDto>.ErrorResponse("Artist not found"));
            
        return Ok(ApiResponse<ArtistDto>.SuccessResponse(artist));
    }

    [HttpGet("me/stats")]
    [Authorize(Roles = "Artist")]
    public async Task<ActionResult<ApiResponse<ArtistStatsDto>>> GetStats()
    {
        try
        {
            var artistId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var stats = await _artistService.GetStatsAsync(artistId);
            return Ok(ApiResponse<ArtistStatsDto>.SuccessResponse(stats));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<ArtistStatsDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("live")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ArtistDto>>>> GetLive()
    {
        var artists = await _artistService.GetLiveArtistsAsync();
        return Ok(ApiResponse<IEnumerable<ArtistDto>>.SuccessResponse(artists));
    }

    [HttpGet("top")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ArtistDto>>>> GetTop([FromQuery] int take = 10)
    {
        var artists = await _artistService.GetTopArtistsAsync(take);
        return Ok(ApiResponse<IEnumerable<ArtistDto>>.SuccessResponse(artists));
    }

    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ArtistDto>>>> Search([FromQuery] string query)
    {
        var artists = await _artistService.SearchArtistsAsync(query);
        return Ok(ApiResponse<IEnumerable<ArtistDto>>.SuccessResponse(artists));
    }

    [HttpPut("me")]
    [Authorize(Roles = "Artist")]
    public async Task<ActionResult<ApiResponse<ArtistDto>>> Update([FromBody] UpdateArtistDto dto)
    {
        try
        {
            var artistId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var artist = await _artistService.UpdateAsync(artistId, dto);
            return Ok(ApiResponse<ArtistDto>.SuccessResponse(artist, "Profile updated successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<ArtistDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPut("me/password")]
    [Authorize(Roles = "Artist")]
    public async Task<ActionResult<ApiResponse>> UpdatePassword([FromBody] ChangePasswordDto dto)
    {
        try
        {
            var artistId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            await _artistService.ChangePasswordAsync(artistId, dto.CurrentPassword, dto.NewPassword);
            return Ok(ApiResponse.SuccessResponse("Password updated successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("me/profile-image")]
    [Authorize(Roles = "Artist")]
    public async Task<ActionResult<ApiResponse<string>>> UpdateProfileImage(IFormFile file)
    {
        try
        {
            var artistId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            using var stream = file.OpenReadStream();
            var path = await _artistService.UpdateProfileImageAsync(artistId, stream, file.FileName);
            return Ok(ApiResponse<string>.SuccessResponse(path, "Profile image updated successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("me/go-live")]
    [Authorize(Roles = "Artist")]
    public async Task<ActionResult<ApiResponse>> GoLive([FromBody] string genre)
    {
        try
        {
            var artistId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            await _artistService.StartLiveStreamAsync(artistId, genre);
            return Ok(ApiResponse.SuccessResponse("Live stream started"));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("me/stop-live")]
    [Authorize(Roles = "Artist")]
    public async Task<ActionResult<ApiResponse>> StopLive()
    {
        try
        {
            var artistId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            await _artistService.StopLiveStreamAsync(artistId);
            return Ok(ApiResponse.SuccessResponse("Live stream stopped"));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("{id:guid}/subscribe")]
    [Authorize(Roles = "User")]
    public async Task<ActionResult<ApiResponse<bool>>> ToggleSubscription(Guid id)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var isSubscribed = await _subscriptionService.ToggleSubscriptionAsync(userId, id);
            return Ok(ApiResponse<bool>.SuccessResponse(isSubscribed, 
                isSubscribed ? "Subscribed to artist" : "Unsubscribed from artist"));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<bool>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("{id:guid}/is-subscribed")]
    [Authorize(Roles = "User")]
    public async Task<ActionResult<ApiResponse<bool>>> IsSubscribed(Guid id)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var isSubscribed = await _subscriptionService.IsSubscribedAsync(userId, id);
        return Ok(ApiResponse<bool>.SuccessResponse(isSubscribed));
    }

    [HttpGet("subscribed")]
    [Authorize(Roles = "User")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ArtistDto>>>> GetSubscribedArtists()
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var artists = await _subscriptionService.GetSubscribedArtistsAsync(userId);
        return Ok(ApiResponse<IEnumerable<ArtistDto>>.SuccessResponse(artists));
    }

    [HttpGet("me/earnings")]
    [Authorize(Roles = "Artist")]
    public async Task<ActionResult<ApiResponse<object>>> GetMyEarnings()
    {
        var artistId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var commissionSetting = await _context.PlatformSettings.FindAsync("platform_commission_percent");
        var commissionPercent = commissionSetting != null && decimal.TryParse(commissionSetting.Value, out var pct) ? pct : 12.00m;

        var totalRevenue = await _context.SongPurchases
            .Where(sp => sp.Song.ArtistId == artistId)
            .SumAsync(sp => sp.Amount);

        var platformCut = totalRevenue * commissionPercent / 100m;
        var netEarnings = totalRevenue - platformCut;

        var withdrawn = await _context.ArtistWithdrawals
            .Where(w => w.ArtistId == artistId && (w.Status == "Approved" || w.Status == "Completed"))
            .SumAsync(w => w.Amount);

        var pendingWithdrawal = await _context.ArtistWithdrawals
            .Where(w => w.ArtistId == artistId && w.Status == "Pending")
            .SumAsync(w => w.Amount);

        var withdrawals = await _context.ArtistWithdrawals
            .Where(w => w.ArtistId == artistId)
            .OrderByDescending(w => w.CreatedAt)
            .Take(20)
            .Select(w => new
            {
                w.Id,
                w.Amount,
                w.Currency,
                w.CardNumber,
                w.Status,
                w.CreatedAt,
                w.ReviewedAt
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            TotalRevenue = totalRevenue,
            CommissionPercent = commissionPercent,
            PlatformCommission = platformCut,
            NetEarnings = netEarnings,
            WithdrawnAmount = withdrawn,
            PendingWithdrawal = pendingWithdrawal,
            AvailableBalance = netEarnings - withdrawn - pendingWithdrawal,
            Withdrawals = withdrawals
        }));
    }

    [HttpPost("me/withdraw")]
    [Authorize(Roles = "Artist")]
    public async Task<ActionResult<ApiResponse<object>>> RequestWithdrawal([FromBody] WithdrawRequestDto dto)
    {
        var artistId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        if (dto.Amount <= 0)
            return BadRequest(ApiResponse<object>.ErrorResponse("Amount must be positive"));

        if (string.IsNullOrWhiteSpace(dto.CardNumber) || dto.CardNumber.Length < 13)
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid card number"));

        // Calculate available balance
        var commissionSetting = await _context.PlatformSettings.FindAsync("platform_commission_percent");
        var commissionPercent = commissionSetting != null && decimal.TryParse(commissionSetting.Value, out var pct) ? pct : 12.00m;

        var totalRevenue = await _context.SongPurchases
            .Where(sp => sp.Song.ArtistId == artistId)
            .SumAsync(sp => sp.Amount);

        var netEarnings = totalRevenue - (totalRevenue * commissionPercent / 100m);
        var withdrawn = await _context.ArtistWithdrawals
            .Where(w => w.ArtistId == artistId && (w.Status == "Approved" || w.Status == "Completed"))
            .SumAsync(w => w.Amount);
        var pending = await _context.ArtistWithdrawals
            .Where(w => w.ArtistId == artistId && w.Status == "Pending")
            .SumAsync(w => w.Amount);

        var available = netEarnings - withdrawn - pending;
        if (dto.Amount > available)
            return BadRequest(ApiResponse<object>.ErrorResponse($"Insufficient balance. Available: ${available:F2}"));

        // Mask card number for storage
        var masked = "**** **** **** " + dto.CardNumber[^4..];

        var withdrawal = new ArtistWithdrawal
        {
            ArtistId = artistId,
            Amount = dto.Amount,
            CardNumber = masked,
            Status = "Pending"
        };

        _context.ArtistWithdrawals.Add(withdrawal);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            withdrawal.Id,
            withdrawal.Amount,
            withdrawal.CardNumber,
            withdrawal.Status,
            withdrawal.CreatedAt
        }, "Withdrawal request submitted"));
    }

    // === COPYRIGHT CLAIMS ===

    [HttpGet("me/copyright-claims")]
    [Authorize(Roles = "Artist")]
    public async Task<ActionResult<ApiResponse<object>>> GetCopyrightClaims()
    {
        var artistId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var claims = await _copyrightClaimService.GetPendingClaimsForArtistAsync(artistId);

        var result = claims.Select(c => new
        {
            c.Id,
            OriginalSong = new { Id = c.OriginalSongId, Title = c.OriginalSongTitle },
            InfringingSong = new { Id = c.InfringingSongId, Title = c.InfringingSongTitle },
            InfringingArtist = new { c.InfringingArtist.Id, c.InfringingArtist.Name },
            c.Status,
            c.CreatedAt
        });

        return Ok(ApiResponse<object>.SuccessResponse(result));
    }

    [HttpPost("me/copyright-claims/{claimId:guid}/confirm")]
    [Authorize(Roles = "Artist")]
    public async Task<ActionResult<ApiResponse>> ConfirmCopyrightClaim(Guid claimId)
    {
        try
        {
            var artistId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            await _copyrightClaimService.ConfirmClaimAsync(claimId, artistId);
            return Ok(ApiResponse.SuccessResponse("Copyright claim confirmed. Infringing song has been removed."));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("me/copyright-claims/{claimId:guid}/dismiss")]
    [Authorize(Roles = "Artist")]
    public async Task<ActionResult<ApiResponse>> DismissCopyrightClaim(Guid claimId)
    {
        try
        {
            var artistId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            await _copyrightClaimService.DismissClaimAsync(claimId, artistId);
            return Ok(ApiResponse.SuccessResponse("Copyright claim dismissed."));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("me/copyright-claims/history")]
    [Authorize(Roles = "Artist")]
    public async Task<ActionResult<ApiResponse<object>>> GetCopyrightClaimHistory()
    {
        var artistId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var claims = await _copyrightClaimService.GetClaimHistoryForArtistAsync(artistId);

        var result = claims.Select(c => new
        {
            c.Id,
            OriginalSongTitle = c.OriginalSongTitle,
            InfringingSongTitle = c.InfringingSongTitle,
            InfringingArtist = new { c.InfringingArtist.Id, c.InfringingArtist.Name },
            c.Status,
            c.CreatedAt,
            c.ResolvedAt
        });

        return Ok(ApiResponse<object>.SuccessResponse(result));
    }

    [HttpGet("me/banned-songs")]
    [Authorize(Roles = "Artist")]
    public async Task<ActionResult<ApiResponse<object>>> GetMyBannedSongs()
    {
        var artistId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var claims = await _copyrightClaimService.GetBannedSongsForArtistAsync(artistId);

        var result = claims.Select(c => new
        {
            c.Id,
            SongTitle = c.InfringingSongTitle,
            OriginalSongTitle = c.OriginalSongTitle,
            OriginalArtist = new { c.OriginalArtist.Id, c.OriginalArtist.Name },
            BannedAt = c.ResolvedAt
        });

        return Ok(ApiResponse<object>.SuccessResponse(result));
    }
}
