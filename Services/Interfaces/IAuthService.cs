using backend.DTOs.Auth;

namespace backend.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterUserAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginUserAsync(LoginDto dto);
    Task<bool> ConfirmEmailAsync(ConfirmEmailDto dto);
    Task<bool> ResendConfirmationCodeAsync(string email);
    Task<ArtistAuthResponseDto> RegisterArtistAsync(ArtistRegisterDto dto);
    Task<ArtistAuthResponseDto> LoginArtistAsync(LoginDto dto);
    Task<bool> ConfirmArtistEmailAsync(ConfirmEmailDto dto);
}
