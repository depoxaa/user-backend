using AutoMapper;
using backend.DTOs.Auth;
using backend.Entities;
using backend.Repositories.Interfaces;
using backend.Services.Interfaces;

namespace backend.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IArtistRepository _artistRepository;
    private readonly IJwtService _jwtService;
    private readonly IEmailService _emailService;
    private readonly IMapper _mapper;

    public AuthService(
        IUserRepository userRepository,
        IArtistRepository artistRepository,
        IJwtService jwtService,
        IEmailService emailService,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _artistRepository = artistRepository;
        _jwtService = jwtService;
        _emailService = emailService;
        _mapper = mapper;
    }

    public async Task<AuthResponseDto> RegisterUserAsync(RegisterDto dto)
    {
        // Check if username exists
        if (await _userRepository.GetByUsernameAsync(dto.Username) != null)
        {
            throw new InvalidOperationException("Username already exists");
        }

        // Check if email exists
        if (await _userRepository.GetByEmailAsync(dto.Email) != null)
        {
            throw new InvalidOperationException("Email already exists");
        }

        var confirmationCode = GenerateConfirmationCode();

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            IsEmailConfirmed = false,
            EmailConfirmationCode = confirmationCode,
            EmailConfirmationCodeExpiry = DateTime.UtcNow.AddMinutes(15)
        };

        await _userRepository.AddAsync(user);

        // Send confirmation email
        try
        {
            await _emailService.SendConfirmationEmailAsync(dto.Email, confirmationCode);
        }
        catch
        {
            // Log but don't fail registration
        }

        var token = _jwtService.GenerateUserToken(user);
        var userDto = _mapper.Map<UserDto>(user);

        return new AuthResponseDto
        {
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60),
            User = userDto
        };
    }

    public async Task<AuthResponseDto> LoginUserAsync(LoginDto dto)
    {
        var user = await _userRepository.GetByUsernameWithDetailsAsync(dto.Username);
        
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid username or password");
        }

        user.IsOnline = true;
        user.LastSeen = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        var token = _jwtService.GenerateUserToken(user);
        var userDto = _mapper.Map<UserDto>(user);

        return new AuthResponseDto
        {
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60),
            User = userDto
        };
    }

    public async Task<bool> ConfirmEmailAsync(ConfirmEmailDto dto)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email);
        
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        if (user.IsEmailConfirmed)
        {
            return true;
        }

        if (user.EmailConfirmationCode != dto.Code)
        {
            throw new InvalidOperationException("Invalid confirmation code");
        }

        if (user.EmailConfirmationCodeExpiry < DateTime.UtcNow)
        {
            throw new InvalidOperationException("Confirmation code has expired");
        }

        user.IsEmailConfirmed = true;
        user.EmailConfirmationCode = null;
        user.EmailConfirmationCodeExpiry = null;
        await _userRepository.UpdateAsync(user);

        return true;
    }

    public async Task<bool> ResendConfirmationCodeAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        if (user.IsEmailConfirmed)
        {
            throw new InvalidOperationException("Email already confirmed");
        }

        var confirmationCode = GenerateConfirmationCode();
        user.EmailConfirmationCode = confirmationCode;
        user.EmailConfirmationCodeExpiry = DateTime.UtcNow.AddMinutes(15);
        await _userRepository.UpdateAsync(user);

        await _emailService.SendConfirmationEmailAsync(email, confirmationCode);

        return true;
    }

    public async Task<ArtistAuthResponseDto> RegisterArtistAsync(ArtistRegisterDto dto)
    {
        // Check if name exists
        if (await _artistRepository.GetByNameAsync(dto.Name) != null)
        {
            throw new InvalidOperationException("Artist name already exists");
        }

        // Check if email exists
        if (await _artistRepository.GetByEmailAsync(dto.Email) != null)
        {
            throw new InvalidOperationException("Email already exists");
        }

        var confirmationCode = GenerateConfirmationCode();

        var artist = new Artist
        {
            Name = dto.Name,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Bio = dto.Bio,
            IsEmailConfirmed = false,
            EmailConfirmationCode = confirmationCode,
            EmailConfirmationCodeExpiry = DateTime.UtcNow.AddMinutes(15)
        };

        await _artistRepository.AddAsync(artist);

        // Send confirmation email
        try
        {
            await _emailService.SendConfirmationEmailAsync(dto.Email, confirmationCode);
        }
        catch
        {
            // Log but don't fail registration
        }

        var token = _jwtService.GenerateArtistToken(artist);
        var artistDto = _mapper.Map<ArtistDto>(artist);

        return new ArtistAuthResponseDto
        {
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60),
            Artist = artistDto
        };
    }

    public async Task<ArtistAuthResponseDto> LoginArtistAsync(LoginDto dto)
    {
        var artist = await _artistRepository.GetByEmailAsync(dto.Username);
        
        if (artist == null || !BCrypt.Net.BCrypt.Verify(dto.Password, artist.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        var token = _jwtService.GenerateArtistToken(artist);
        var artistDto = _mapper.Map<ArtistDto>(artist);

        return new ArtistAuthResponseDto
        {
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60),
            Artist = artistDto
        };
    }

    public async Task<bool> ConfirmArtistEmailAsync(ConfirmEmailDto dto)
    {
        var artist = await _artistRepository.GetByEmailAsync(dto.Email);
        
        if (artist == null)
        {
            throw new InvalidOperationException("Artist not found");
        }

        if (artist.IsEmailConfirmed)
        {
            return true;
        }

        if (artist.EmailConfirmationCode != dto.Code)
        {
            throw new InvalidOperationException("Invalid confirmation code");
        }

        if (artist.EmailConfirmationCodeExpiry < DateTime.UtcNow)
        {
            throw new InvalidOperationException("Confirmation code has expired");
        }

        artist.IsEmailConfirmed = true;
        artist.EmailConfirmationCode = null;
        artist.EmailConfirmationCodeExpiry = null;
        await _artistRepository.UpdateAsync(artist);

        return true;
    }

    private static string GenerateConfirmationCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }
}
