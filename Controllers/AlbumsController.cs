using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.DTOs.Album;
using backend.DTOs.Common;
using backend.Services.Interfaces;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlbumsController : ControllerBase
{
    private readonly IAlbumService _albumService;

    public AlbumsController(IAlbumService albumService)
    {
        _albumService = albumService;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<AlbumDetailDto>>> GetById(Guid id)
    {
        var album = await _albumService.GetDetailAsync(id);
        
        if (album == null)
            return NotFound(ApiResponse<AlbumDetailDto>.ErrorResponse("Album not found"));
            
        return Ok(ApiResponse<AlbumDetailDto>.SuccessResponse(album));
    }

    [HttpGet("artist/{artistId:guid}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<AlbumDto>>>> GetByArtist(Guid artistId)
    {
        var albums = await _albumService.GetByArtistAsync(artistId);
        return Ok(ApiResponse<IEnumerable<AlbumDto>>.SuccessResponse(albums));
    }

    [HttpGet("my")]
    [Authorize(Roles = "Artist")]
    public async Task<ActionResult<ApiResponse<IEnumerable<AlbumDto>>>> GetMyAlbums()
    {
        var artistId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var albums = await _albumService.GetByArtistAsync(artistId);
        return Ok(ApiResponse<IEnumerable<AlbumDto>>.SuccessResponse(albums));
    }

    [HttpPost]
    [Authorize(Roles = "Artist")]
    public async Task<ActionResult<ApiResponse<AlbumDto>>> Create(
        [FromForm] CreateAlbumDto dto,
        [FromForm] IFormFile? coverFile = null)
    {
        try
        {
            var artistId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            
            Stream? coverStream = null;
            string? coverFileName = null;
            
            if (coverFile != null)
            {
                coverStream = coverFile.OpenReadStream();
                coverFileName = coverFile.FileName;
            }

            var album = await _albumService.CreateAsync(artistId, dto, coverStream, coverFileName);
            
            coverStream?.Dispose();
            
            return CreatedAtAction(nameof(GetById), new { id = album.Id },
                ApiResponse<AlbumDto>.SuccessResponse(album, "Album created successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<AlbumDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Artist")]
    public async Task<ActionResult<ApiResponse<AlbumDto>>> Update(Guid id, [FromBody] CreateAlbumDto dto)
    {
        try
        {
            var artistId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var album = await _albumService.UpdateAsync(id, artistId, dto);
            return Ok(ApiResponse<AlbumDto>.SuccessResponse(album, "Album updated successfully"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<AlbumDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("{id:guid}/cover")]
    [Authorize(Roles = "Artist")]
    public async Task<ActionResult<ApiResponse<string>>> UpdateCover(Guid id, IFormFile file)
    {
        try
        {
            var artistId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            using var stream = file.OpenReadStream();
            var path = await _albumService.UpdateCoverAsync(id, artistId, stream, file.FileName);
            return Ok(ApiResponse<string>.SuccessResponse(path, "Cover updated successfully"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Artist")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id)
    {
        try
        {
            var artistId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            await _albumService.DeleteAsync(id, artistId);
            return Ok(ApiResponse.SuccessResponse("Album deleted successfully"));
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
