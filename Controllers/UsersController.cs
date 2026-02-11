using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.DTOs.Auth;
using backend.DTOs.User;
using backend.DTOs.Common;
using backend.Services.Interfaces;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "User")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetCurrentUser()
    {
        var userId = GetCurrentUserId();
        var user = await _userService.GetByIdAsync(userId);
        
        if (user == null)
            return NotFound(ApiResponse<UserDto>.ErrorResponse("User not found"));
            
        return Ok(ApiResponse<UserDto>.SuccessResponse(user));
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetById(Guid id)
    {
        var user = await _userService.GetByIdAsync(id);
        
        if (user == null)
            return NotFound(ApiResponse<UserDto>.ErrorResponse("User not found"));
            
        return Ok(ApiResponse<UserDto>.SuccessResponse(user));
    }

    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<IEnumerable<UserDto>>>> Search([FromQuery] string query)
    {
        var userId = GetCurrentUserId();
        var users = await _userService.SearchUsersAsync(query, userId);
        return Ok(ApiResponse<IEnumerable<UserDto>>.SuccessResponse(users));
    }

    [HttpPut("me")]
    public async Task<ActionResult<ApiResponse<UserDto>>> Update([FromBody] UpdateUserDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var user = await _userService.UpdateAsync(userId, dto);
            return Ok(ApiResponse<UserDto>.SuccessResponse(user, "Profile updated successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<UserDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPut("me/password")]
    public async Task<ActionResult<ApiResponse>> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _userService.ChangePasswordAsync(userId, dto);
            return Ok(ApiResponse.SuccessResponse("Password changed successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("me/avatar")]
    public async Task<ActionResult<ApiResponse<string>>> UpdateAvatar(IFormFile file)
    {
        try
        {
            var userId = GetCurrentUserId();
            using var stream = file.OpenReadStream();
            var path = await _userService.UpdateAvatarAsync(userId, stream, file.FileName);
            return Ok(ApiResponse<string>.SuccessResponse(path, "Avatar updated successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("me/status")]
    public async Task<ActionResult<ApiResponse>> UpdateListeningStatus([FromBody] UpdateStatusDto dto)
    {
        var userId = GetCurrentUserId();
        await _userService.UpdateListeningStatusAsync(userId, dto.Status);
        return Ok(ApiResponse.SuccessResponse("Status updated"));
    }

    [HttpPost("me/online")]
    public async Task<ActionResult<ApiResponse>> SetOnline([FromBody] bool isOnline)
    {
        var userId = GetCurrentUserId();
        await _userService.SetOnlineStatusAsync(userId, isOnline);
        return Ok(ApiResponse.SuccessResponse("Online status updated"));
    }

    [HttpPost("me/playback")]
    public async Task<ActionResult<ApiResponse>> UpdatePlayback([FromBody] UpdatePlaybackDto dto)
    {
        var userId = GetCurrentUserId();
        await _userService.UpdatePlaybackAsync(userId, dto);
        return Ok(ApiResponse.SuccessResponse("Playback updated"));
    }

    [HttpGet("{id:guid}/playback")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<LivePlaybackDto>>> GetLivePlayback(Guid id)
    {
        var playback = await _userService.GetLivePlaybackAsync(id);
        
        if (playback == null)
            return NotFound(ApiResponse<LivePlaybackDto>.ErrorResponse("User not found"));
            
        return Ok(ApiResponse<LivePlaybackDto>.SuccessResponse(playback));
    }
}

