using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.DTOs.Song;
using backend.DTOs.Common;
using backend.Services.Interfaces;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "User,Premium,Artist")]
public class RecommendationsController : ControllerBase
{
    private readonly IRecommendationService _recommendationService;

    public RecommendationsController(IRecommendationService recommendationService)
    {
        _recommendationService = recommendationService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<SongDto>>>> GetRecommendations(
        [FromQuery] int limit = 20,
        [FromQuery] int offset = 0)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var playCount = await _recommendationService.GetPlayEventCountAsync(userId);

        if (playCount < 5)
        {
            return Ok(ApiResponse<IEnumerable<SongDto>>.SuccessResponse(
                Enumerable.Empty<SongDto>(),
                "Play some tracks to unlock your recommendations"));
        }

        var songs = await _recommendationService.GetRecommendationsAsync(userId, limit, offset);
        return Ok(ApiResponse<IEnumerable<SongDto>>.SuccessResponse(songs));
    }

    [HttpGet("count")]
    public async Task<ActionResult<ApiResponse<int>>> GetPlayEventCount()
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var count = await _recommendationService.GetPlayEventCountAsync(userId);
        return Ok(ApiResponse<int>.SuccessResponse(count));
    }
}
