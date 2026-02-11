using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using backend.Configuration;
using backend.Services.Interfaces;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SseController : ControllerBase
{
    private readonly ISseConnectionManager _sseManager;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<SseController> _logger;

    public SseController(
        ISseConnectionManager sseManager,
        IOptions<JwtSettings> jwtSettings,
        ILogger<SseController> logger)
    {
        _sseManager = sseManager;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }

    /// <summary>
    /// SSE endpoint for real-time updates. 
    /// Since EventSource doesn't support headers, token is passed via query parameter.
    /// </summary>
    [HttpGet("events")]
    public async Task Events([FromQuery] string token)
    {
        // Validate JWT token manually since EventSource can't send Authorization header
        var userId = ValidateTokenAndGetUserId(token);
        if (userId == null)
        {
            Response.StatusCode = 401;
            await Response.WriteAsync("Unauthorized");
            return;
        }

        // Set up SSE response
        Response.Headers["Content-Type"] = "text/event-stream";
        Response.Headers["Cache-Control"] = "no-cache";
        Response.Headers["Connection"] = "keep-alive";
        Response.Headers["X-Accel-Buffering"] = "no"; // Disable nginx buffering

        _logger.LogInformation("SSE connection request from user {UserId}", userId);

        // Register connection and keep it open
        await _sseManager.RegisterConnectionAsync(userId.Value, Response, HttpContext.RequestAborted);
    }

    private Guid? ValidateTokenAndGetUserId(string token)
    {
        if (string.IsNullOrEmpty(token))
            return null;

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);
            
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (Guid.TryParse(userIdClaim, out var userId))
                return userId;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Invalid SSE token");
        }

        return null;
    }
}
