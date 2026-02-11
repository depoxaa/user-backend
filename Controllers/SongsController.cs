using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.DTOs.Song;
using backend.DTOs.Common;
using backend.Services.Interfaces;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SongsController : ControllerBase
{
    private readonly ISongService _songService;

    public SongsController(ISongService songService)
    {
        _songService = songService;
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

    [HttpGet("{id:guid}/stream")]
    public async Task<IActionResult> Stream(Guid id)
    {
        var stream = await _songService.GetAudioStreamAsync(id);
        if (stream == null)
            return NotFound();
            
        return File(stream, "audio/mpeg", enableRangeProcessing: true);
    }

    [HttpPost("{id:guid}/like")]
    [Authorize(Roles = "User")]
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
    [Authorize(Roles = "User")]
    public async Task<ActionResult<ApiResponse>> RecordPlay(Guid id, [FromBody] int listeningSeconds)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        await _songService.RecordPlayAsync(id, userId, listeningSeconds);
        return Ok(ApiResponse.SuccessResponse("Play recorded"));
    }

    [HttpGet("liked")]
    [Authorize(Roles = "User")]
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
}
