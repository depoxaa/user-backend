using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.DTOs.Playlist;
using backend.DTOs.Common;
using backend.Services.Interfaces;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlaylistsController : ControllerBase
{
    private readonly IPlaylistService _playlistService;
    private readonly IFileService _fileService;

    public PlaylistsController(IPlaylistService playlistService, IFileService fileService)
    {
        _playlistService = playlistService;
        _fileService = fileService;
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return userIdClaim != null ? Guid.Parse(userIdClaim) : null;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<PlaylistDetailDto>>> GetById(Guid id)
    {
        var playlist = await _playlistService.GetDetailAsync(id, GetCurrentUserId());
        
        if (playlist == null)
            return NotFound(ApiResponse<PlaylistDetailDto>.ErrorResponse("Playlist not found"));
            
        return Ok(ApiResponse<PlaylistDetailDto>.SuccessResponse(playlist));
    }

    [HttpGet("user/{userId:guid}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PlaylistDto>>>> GetByUser(Guid userId)
    {
        var viewerId = GetCurrentUserId();
        var playlists = await _playlistService.GetByUserAsync(userId, viewerId);
        return Ok(ApiResponse<IEnumerable<PlaylistDto>>.SuccessResponse(playlists));
    }

    [HttpGet("my")]
    [Authorize(Roles = "User")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PlaylistDto>>>> GetMyPlaylists()
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var playlists = await _playlistService.GetByUserAsync(userId, userId);
        return Ok(ApiResponse<IEnumerable<PlaylistDto>>.SuccessResponse(playlists));
    }

    [HttpGet("public")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PlaylistDto>>>> GetPublic([FromQuery] int take = 20)
    {
        var playlists = await _playlistService.GetPublicPlaylistsAsync(take);
        return Ok(ApiResponse<IEnumerable<PlaylistDto>>.SuccessResponse(playlists));
    }

    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PlaylistDto>>>> Search([FromQuery] string query)
    {
        var playlists = await _playlistService.SearchAsync(query);
        return Ok(ApiResponse<IEnumerable<PlaylistDto>>.SuccessResponse(playlists));
    }

    [HttpPost]
    [Authorize(Roles = "User")]
    public async Task<ActionResult<ApiResponse<PlaylistDto>>> Create([FromBody] CreatePlaylistDto dto)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var playlist = await _playlistService.CreateAsync(userId, dto);
        return CreatedAtAction(nameof(GetById), new { id = playlist.Id },
            ApiResponse<PlaylistDto>.SuccessResponse(playlist, "Playlist created successfully"));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "User")]
    public async Task<ActionResult<ApiResponse<PlaylistDto>>> Update(Guid id, [FromBody] UpdatePlaylistDto dto)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var playlist = await _playlistService.UpdateAsync(id, userId, dto);
            return Ok(ApiResponse<PlaylistDto>.SuccessResponse(playlist, "Playlist updated successfully"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<PlaylistDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "User")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            await _playlistService.DeleteAsync(id, userId);
            return Ok(ApiResponse.SuccessResponse("Playlist deleted successfully"));
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

    [HttpPost("{id:guid}/songs")]
    [Authorize(Roles = "User")]
    public async Task<ActionResult<ApiResponse>> AddSong(Guid id, [FromBody] AddSongToPlaylistDto dto)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            await _playlistService.AddSongAsync(id, userId, dto.SongId);
            return Ok(ApiResponse.SuccessResponse("Song added to playlist"));
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

    [HttpDelete("{id:guid}/songs/{songId:guid}")]
    [Authorize(Roles = "User")]
    public async Task<ActionResult<ApiResponse>> RemoveSong(Guid id, Guid songId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            await _playlistService.RemoveSongAsync(id, userId, songId);
            return Ok(ApiResponse.SuccessResponse("Song removed from playlist"));
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

    [HttpPut("{id:guid}/songs/reorder")]
    [Authorize(Roles = "User")]
    public async Task<ActionResult<ApiResponse>> ReorderSongs(Guid id, [FromBody] List<Guid> songIds)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            await _playlistService.ReorderSongsAsync(id, userId, songIds);
            return Ok(ApiResponse.SuccessResponse("Songs reordered"));
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

    [HttpPost("{id:guid}/cover")]
    [Authorize(Roles = "User")]
    public async Task<ActionResult<ApiResponse<string>>> UploadCover(Guid id, IFormFile file)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            
            // Validate file
            if (file == null || file.Length == 0)
                return BadRequest(ApiResponse<string>.ErrorResponse("No file provided"));

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
                return BadRequest(ApiResponse<string>.ErrorResponse("Invalid file type. Allowed: jpg, jpeg, png, gif, webp"));

            // Save file using stream
            using var stream = file.OpenReadStream();
            var fileName = await _fileService.SaveImageFileAsync(stream, file.FileName, "playlist-covers");
            
            // Update playlist
            var playlist = await _playlistService.UpdateCoverAsync(id, userId, fileName);
            
            return Ok(ApiResponse<string>.SuccessResponse(fileName, "Cover uploaded successfully"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<string>.ErrorResponse(ex.Message));
        }
    }
}
