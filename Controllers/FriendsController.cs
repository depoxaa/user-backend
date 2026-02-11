using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.DTOs.Friend;
using backend.DTOs.Common;
using backend.Services.Interfaces;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "User")]
public class FriendsController : ControllerBase
{
    private readonly IFriendService _friendService;

    public FriendsController(IFriendService friendService)
    {
        _friendService = friendService;
    }

    private Guid GetCurrentUserId()
    {
        return Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<FriendDto>>>> GetFriends()
    {
        var userId = GetCurrentUserId();
        var friends = await _friendService.GetFriendsAsync(userId);
        return Ok(ApiResponse<IEnumerable<FriendDto>>.SuccessResponse(friends));
    }

    [HttpGet("live")]
    public async Task<ActionResult<ApiResponse<IEnumerable<FriendDto>>>> GetLiveFriends()
    {
        var userId = GetCurrentUserId();
        var liveFriends = await _friendService.GetLiveFriendsAsync(userId);
        return Ok(ApiResponse<IEnumerable<FriendDto>>.SuccessResponse(liveFriends));
    }

    [HttpGet("requests")]
    public async Task<ActionResult<ApiResponse<IEnumerable<FriendRequestDto>>>> GetPendingRequests()
    {
        var userId = GetCurrentUserId();
        var requests = await _friendService.GetPendingRequestsAsync(userId);
        return Ok(ApiResponse<IEnumerable<FriendRequestDto>>.SuccessResponse(requests));
    }

    [HttpGet("requests/sent")]
    public async Task<ActionResult<ApiResponse<IEnumerable<FriendRequestDto>>>> GetSentRequests()
    {
        var userId = GetCurrentUserId();
        var requests = await _friendService.GetSentRequestsAsync(userId);
        return Ok(ApiResponse<IEnumerable<FriendRequestDto>>.SuccessResponse(requests));
    }

    [HttpPost("requests/{receiverId:guid}")]
    public async Task<ActionResult<ApiResponse>> SendRequest(Guid receiverId)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _friendService.SendRequestAsync(userId, receiverId);
            return Ok(ApiResponse.SuccessResponse("Friend request sent"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("requests/{requestId:guid}/accept")]
    public async Task<ActionResult<ApiResponse>> AcceptRequest(Guid requestId)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _friendService.AcceptRequestAsync(requestId, userId);
            return Ok(ApiResponse.SuccessResponse("Friend request accepted"));
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

    [HttpPost("requests/{requestId:guid}/reject")]
    public async Task<ActionResult<ApiResponse>> RejectRequest(Guid requestId)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _friendService.RejectRequestAsync(requestId, userId);
            return Ok(ApiResponse.SuccessResponse("Friend request rejected"));
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

    [HttpDelete("{friendId:guid}")]
    public async Task<ActionResult<ApiResponse>> RemoveFriend(Guid friendId)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _friendService.RemoveFriendAsync(userId, friendId);
            return Ok(ApiResponse.SuccessResponse("Friend removed"));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("check/{userId:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> AreFriends(Guid userId)
    {
        var currentUserId = GetCurrentUserId();
        var areFriends = await _friendService.AreFriendsAsync(currentUserId, userId);
        return Ok(ApiResponse<bool>.SuccessResponse(areFriends));
    }
}
