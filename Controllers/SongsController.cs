using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.DTOs.Song;
using backend.DTOs.Common;
using backend.Services.Interfaces;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SongsController : ControllerBase
{
    private readonly ISongService _songService;
    private readonly IPaymentService _paymentService;
    private readonly IJwtService _jwtService;
    private readonly ISongReportService _reportService;
    private readonly ApplicationDbContext _context;

    public SongsController(
        ISongService songService,
        IPaymentService paymentService,
        IJwtService jwtService,
        ISongReportService reportService,
        ApplicationDbContext context)
    {
        _songService = songService;
        _paymentService = paymentService;
        _jwtService = jwtService;
        _reportService = reportService;
        _context = context;
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return userIdClaim != null ? Guid.Parse(userIdClaim) : null;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<SongDto>>> GetById(Guid id)
    {
        var song = await _songService.GetByIdAsync(id, GetCurrentUserId());
        
        if (song == null)
            return NotFound(ApiResponse<SongDto>.ErrorResponse("Song not found"));
            
        return Ok(ApiResponse<SongDto>.SuccessResponse(song));
    }

    [HttpGet("artist/{artistId:guid}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<SongDto>>>> GetByArtist(Guid artistId)
    {
        var songs = await _songService.GetByArtistAsync(artistId, GetCurrentUserId());
        return Ok(ApiResponse<IEnumerable<SongDto>>.SuccessResponse(songs));
    }

    [HttpGet("album/{albumId:guid}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<SongDto>>>> GetByAlbum(Guid albumId)
    {
        var songs = await _songService.GetByAlbumAsync(albumId, GetCurrentUserId());
        return Ok(ApiResponse<IEnumerable<SongDto>>.SuccessResponse(songs));
    }

    [HttpGet("genre/{genreId:guid}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<SongDto>>>> GetByGenre(Guid genreId)
    {
        var songs = await _songService.GetByGenreAsync(genreId, GetCurrentUserId());
        return Ok(ApiResponse<IEnumerable<SongDto>>.SuccessResponse(songs));
    }

    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<IEnumerable<SongDto>>>> Search([FromQuery] string query)
    {
        var songs = await _songService.SearchAsync(query, GetCurrentUserId());
        return Ok(ApiResponse<IEnumerable<SongDto>>.SuccessResponse(songs));
    }

    [HttpGet("top")]
    public async Task<ActionResult<ApiResponse<IEnumerable<SongDto>>>> GetTop([FromQuery] int take = 10)
    {
        var songs = await _songService.GetTopSongsAsync(take, GetCurrentUserId());
        return Ok(ApiResponse<IEnumerable<SongDto>>.SuccessResponse(songs));
    }

    [HttpGet("recent")]
    public async Task<ActionResult<ApiResponse<IEnumerable<SongDto>>>> GetRecent([FromQuery] int take = 20)
    {
        var songs = await _songService.GetRecentSongsAsync(take, GetCurrentUserId());
        return Ok(ApiResponse<IEnumerable<SongDto>>.SuccessResponse(songs));
    }

    [HttpGet("top-listened-monthly")]
    public async Task<ActionResult<ApiResponse<IEnumerable<SongDto>>>> GetTopListenedMonthly([FromQuery] int take = 20)
    {
        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var userId = GetCurrentUserId();

        var topSongIds = await _context.SongPlays
            .Where(sp => sp.PlayedAt >= monthStart)
            .GroupBy(sp => sp.SongId)
            .Select(g => new { SongId = g.Key, PlayCount = g.Count() })
            .OrderByDescending(x => x.PlayCount)
            .Take(take)
            .Select(x => x.SongId)
            .ToListAsync();

        var songs = new List<SongDto>();
        foreach (var songId in topSongIds)
        {
            var song = await _songService.GetByIdAsync(songId, userId);
            if (song != null) songs.Add(song);
        }

        return Ok(ApiResponse<IEnumerable<SongDto>>.SuccessResponse(songs));
    }

    [HttpGet("top-liked-monthly")]
    public async Task<ActionResult<ApiResponse<IEnumerable<SongDto>>>> GetTopLikedMonthly([FromQuery] int take = 20)
    {
        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var userId = GetCurrentUserId();

        var topSongIds = await _context.SongLikes
            .Where(sl => sl.CreatedAt >= monthStart)
            .GroupBy(sl => sl.SongId)
            .Select(g => new { SongId = g.Key, LikeCount = g.Count() })
            .OrderByDescending(x => x.LikeCount)
            .Take(take)
            .Select(x => x.SongId)
            .ToListAsync();

        var songs = new List<SongDto>();
        foreach (var songId in topSongIds)
        {
            var song = await _songService.GetByIdAsync(songId, userId);
            if (song != null) songs.Add(song);
        }

        return Ok(ApiResponse<IEnumerable<SongDto>>.SuccessResponse(songs));
    }

    [HttpGet("{id:guid}/stream")]
    public async Task<IActionResult> Stream(Guid id, [FromQuery] string? token = null)
    {
        var userId = GetCurrentUserId();

        if (!userId.HasValue && !string.IsNullOrEmpty(token))
        {
            userId = _jwtService.ValidateToken(token);
        }

        try
        {
            var stream = await _songService.GetAudioStreamAsync(id, userId);
            if (stream == null)
                return StatusCode(402, new { message = "Payment required to access this content" });

            return File(stream, "audio/mpeg", enableRangeProcessing: true);
        }
        catch (InvalidOperationException ex) when (ex.Message == "TRACK_BANNED")
        {
            return StatusCode(403, new { error = "track_banned" });
        }
    }

    [HttpPost("{id:guid}/purchase")]
    [Authorize(Roles = "User,Premium")]
    public async Task<ActionResult<ApiResponse<SongPurchaseDto>>> PurchaseSong(Guid id, [FromBody] PurchaseSongDto dto)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var purchase = await _paymentService.PurchaseSongAsync(userId, id, dto.PaymentToken);
            return Ok(ApiResponse<SongPurchaseDto>.SuccessResponse(purchase, "Song purchased successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<SongPurchaseDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("purchased")]
    [Authorize(Roles = "User,Premium")]
    public async Task<ActionResult<ApiResponse<IEnumerable<SongDto>>>> GetPurchasedSongs()
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var songs = await _songService.GetPurchasedSongsAsync(userId);
        return Ok(ApiResponse<IEnumerable<SongDto>>.SuccessResponse(songs));
    }

    [HttpPost("{id:guid}/like")]
    [Authorize(Roles = "User,Premium")]
    public async Task<ActionResult<ApiResponse<bool>>> ToggleLike(Guid id)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var isLiked = await _songService.ToggleLikeAsync(id, userId);
            return Ok(ApiResponse<bool>.SuccessResponse(isLiked, isLiked ? "Song liked" : "Song unliked"));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<bool>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("{id:guid}/play")]
    [Authorize(Roles = "User,Premium")]
    public async Task<ActionResult<ApiResponse>> RecordPlay(Guid id, [FromBody] int listeningSeconds)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        await _songService.RecordPlayAsync(id, userId, listeningSeconds);
        return Ok(ApiResponse.SuccessResponse("Play recorded"));
    }

    [HttpGet("liked")]
    [Authorize(Roles = "User,Premium")]
    public async Task<ActionResult<ApiResponse<IEnumerable<SongDto>>>> GetLikedSongs()
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var songs = await _songService.GetLikedSongsAsync(userId);
        return Ok(ApiResponse<IEnumerable<SongDto>>.SuccessResponse(songs));
    }

    [HttpGet("me")]
    [Authorize(Roles = "Artist")]
    public async Task<ActionResult<ApiResponse<IEnumerable<SongDto>>>> GetMySongs()
    {
        var artistId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var songs = await _songService.GetByArtistAsync(artistId, null);
        return Ok(ApiResponse<IEnumerable<SongDto>>.SuccessResponse(songs));
    }

    [HttpPost]
    [Authorize(Roles = "Artist")]
    public async Task<ActionResult<ApiResponse<SongDto>>> Create(
        [FromForm] CreateSongDto dto,
        [FromForm] IFormFile audioFile,
        [FromForm] IFormFile? coverFile = null)
    {
        try
        {
            var artistId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            
            using var audioStream = audioFile.OpenReadStream();
            Stream? coverStream = null;
            string? coverFileName = null;
            
            if (coverFile != null)
            {
                coverStream = coverFile.OpenReadStream();
                coverFileName = coverFile.FileName;
            }

            var song = await _songService.CreateAsync(artistId, dto, audioStream, audioFile.FileName, coverStream, coverFileName);
            
            coverStream?.Dispose();
            
            return CreatedAtAction(nameof(GetById), new { id = song.Id }, 
                ApiResponse<SongDto>.SuccessResponse(song, "Song uploaded successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<SongDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Artist")]
    public async Task<ActionResult<ApiResponse<SongDto>>> Update(Guid id, [FromBody] UpdateSongDto dto)
    {
        try
        {
            var artistId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var song = await _songService.UpdateAsync(id, artistId, dto);
            return Ok(ApiResponse<SongDto>.SuccessResponse(song, "Song updated successfully"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<SongDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Artist")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id)
    {
        try
        {
            var artistId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            await _songService.DeleteAsync(id, artistId);
            return Ok(ApiResponse.SuccessResponse("Song deleted successfully"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("{id:guid}/report")]
    [Authorize(Roles = "Artist,User,Premium")]
    public async Task<ActionResult<ApiResponse<SongReportDto>>> ReportSong(Guid id, [FromBody] CreateSongReportDto dto)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "User";

            // Only artists (separate Artist role) or admin can report
            // Users with ArtistVerified=approved in the User table can also report
            // For simplicity: allow Artist role and any user role to report (requirement says artist or admin)
            if (userRole != "Artist" && userRole != "SuperAdmin")
            {
                return StatusCode(403, ApiResponse<SongReportDto>.ErrorResponse("Only artists can report songs"));
            }

            var report = await _reportService.CreateReportAsync(userId, id, dto);
            return StatusCode(201, ApiResponse<SongReportDto>.SuccessResponse(report, "Report submitted"));
        }
        catch (InvalidOperationException ex) when (ex.Message == "DUPLICATE_PENDING_REPORT")
        {
            return StatusCode(409, ApiResponse<SongReportDto>.ErrorResponse("You already have a pending report for this song"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<SongReportDto>.ErrorResponse(ex.Message));
        }
    }
}
