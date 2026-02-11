using Microsoft.AspNetCore.Mvc;
using backend.DTOs.Auth;
using backend.DTOs.Common;
using backend.Services.Interfaces;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register([FromBody] RegisterDto dto)
    {
        try
        {
            var result = await _authService.RegisterUserAsync(dto);
            return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(result, "Registration successful"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<AuthResponseDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] LoginDto dto)
    {
        try
        {
            var result = await _authService.LoginUserAsync(dto);
            return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(result, "Login successful"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<AuthResponseDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("confirm-email")]
    public async Task<ActionResult<ApiResponse>> ConfirmEmail([FromBody] ConfirmEmailDto dto)
    {
        try
        {
            await _authService.ConfirmEmailAsync(dto);
            return Ok(ApiResponse.SuccessResponse("Email confirmed successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("resend-confirmation")]
    public async Task<ActionResult<ApiResponse>> ResendConfirmation([FromBody] string email)
    {
        try
        {
            await _authService.ResendConfirmationCodeAsync(email);
            return Ok(ApiResponse.SuccessResponse("Confirmation code sent"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("artist/register")]
    public async Task<ActionResult<ApiResponse<ArtistAuthResponseDto>>> RegisterArtist([FromBody] ArtistRegisterDto dto)
    {
        try
        {
            var result = await _authService.RegisterArtistAsync(dto);
            return Ok(ApiResponse<ArtistAuthResponseDto>.SuccessResponse(result, "Artist registration successful"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<ArtistAuthResponseDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("artist/login")]
    public async Task<ActionResult<ApiResponse<ArtistAuthResponseDto>>> LoginArtist([FromBody] LoginDto dto)
    {
        try
        {
            var result = await _authService.LoginArtistAsync(dto);
            return Ok(ApiResponse<ArtistAuthResponseDto>.SuccessResponse(result, "Login successful"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<ArtistAuthResponseDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("artist/confirm-email")]
    public async Task<ActionResult<ApiResponse>> ConfirmArtistEmail([FromBody] ConfirmEmailDto dto)
    {
        try
        {
            await _authService.ConfirmArtistEmailAsync(dto);
            return Ok(ApiResponse.SuccessResponse("Email confirmed successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.ErrorResponse(ex.Message));
        }
    }
}
