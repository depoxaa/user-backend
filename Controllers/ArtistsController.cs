using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.DTOs.Auth;
using backend.DTOs.Artist;
using backend.DTOs.Common;
using backend.DTOs.User;
using backend.Services.Interfaces;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArtistsController : ControllerBase
{
    private readonly IArtistService _artistService;
    private readonly ISubscriptionService _subscriptionService;

    public ArtistsController(IArtistService artistService, ISubscriptionService subscriptionService)
    {
        _artistService = artistService;
        _subscriptionService = subscriptionService;
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
}
