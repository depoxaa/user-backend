using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.DTOs.Common;
using backend.Services.Interfaces;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StreamsController : ControllerBase
{
    private readonly IStreamViewerService _streamViewerService;
    private readonly IPlatformSettingService _settingService;
    private readonly ISseConnectionManager _sseManager;

    public StreamsController(
        IStreamViewerService streamViewerService,
        IPlatformSettingService settingService,
        ISseConnectionManager sseManager)
    {
        _streamViewerService = streamViewerService;
        _settingService = settingService;
        _sseManager = sseManager;
    }

    [HttpPost("{id:guid}/join")]
    [Authorize(Roles = "User,Premium")]
    public async Task<ActionResult<ApiResponse>> JoinStream(Guid id)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            await _streamViewerService.JoinStreamAsync(id, userId);

            // Push viewer count update
            var count = await _streamViewerService.GetActiveViewerCountAsync(id);
            var hostRole = "User";
            var limit = await _settingService.GetStreamViewerLimitAsync(hostRole);
            await _sseManager.BroadcastEventAsync("viewer_count", new { streamId = id, count, limit });

            return Ok(ApiResponse.SuccessResponse("Joined stream"));
        }
        catch (InvalidOperationException ex) when (ex.Message.StartsWith("STREAM_FULL"))
        {
            var limitStr = ex.Message.Split(':').LastOrDefault();
            int.TryParse(limitStr, out var limit);
            return StatusCode(403, new { error = "stream_full", limit });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("{id:guid}/leave")]
    [Authorize(Roles = "User,Premium")]
    public async Task<ActionResult<ApiResponse>> LeaveStream(Guid id)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        await _streamViewerService.LeaveStreamAsync(id, userId);

        // Push viewer count update
        var count = await _streamViewerService.GetActiveViewerCountAsync(id);
        var hostRole = "User";
        var limit = await _settingService.GetStreamViewerLimitAsync(hostRole);
        await _sseManager.BroadcastEventAsync("viewer_count", new { streamId = id, count, limit });

        return Ok(ApiResponse.SuccessResponse("Left stream"));
    }

    [HttpGet("{id:guid}/viewers")]
    public async Task<ActionResult<ApiResponse<int>>> GetViewerCount(Guid id)
    {
        var count = await _streamViewerService.GetActiveViewerCountAsync(id);
        return Ok(ApiResponse<int>.SuccessResponse(count));
    }
}
