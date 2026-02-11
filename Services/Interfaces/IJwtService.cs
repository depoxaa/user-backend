using backend.Entities;

namespace backend.Services.Interfaces;

public interface IJwtService
{
    string GenerateUserToken(User user);
    string GenerateArtistToken(Artist artist);
    Guid? ValidateToken(string token);
}
