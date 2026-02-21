using AutoMapper;
using backend.DTOs.Auth;
using backend.DTOs.Artist;
using backend.Repositories.Interfaces;
using backend.Services.Interfaces;

namespace backend.Services.Implementations;

public class ArtistService : IArtistService
{
    private readonly IArtistRepository _artistRepository;
    private readonly ISongPurchaseRepository _songPurchaseRepository;
    private readonly IFileService _fileService;
    private readonly IMapper _mapper;

    public ArtistService(
        IArtistRepository artistRepository,
        ISongPurchaseRepository songPurchaseRepository,
        IFileService fileService,
        IMapper mapper)
    {
        _artistRepository = artistRepository;
        _songPurchaseRepository = songPurchaseRepository;
        _fileService = fileService;
        _mapper = mapper;
    }

    public async Task<ArtistDto?> GetByIdAsync(Guid id)
    {
        var artist = await _artistRepository.GetWithSongsAsync(id);
        return artist == null ? null : _mapper.Map<ArtistDto>(artist);
    }

    public async Task<ArtistDto> UpdateAsync(Guid id, UpdateArtistDto dto)
    {
        var artist = await _artistRepository.GetByIdAsync(id);
        if (artist == null)
        {
            throw new InvalidOperationException("Artist not found");
        }

        if (!string.IsNullOrEmpty(dto.Name))
            artist.Name = dto.Name;
        if (dto.Bio != null)
            artist.Bio = dto.Bio;

        await _artistRepository.UpdateAsync(artist);
        return _mapper.Map<ArtistDto>(artist);
    }

    public async Task<string?> UpdateProfileImageAsync(Guid id, Stream fileStream, string fileName)
    {
        var artist = await _artistRepository.GetByIdAsync(id);
        if (artist == null)
        {
            throw new InvalidOperationException("Artist not found");
        }

        // Delete old image if exists
        if (!string.IsNullOrEmpty(artist.ProfileImage))
        {
            await _fileService.DeleteFileAsync(artist.ProfileImage);
        }

        var filePath = await _fileService.SaveImageFileAsync(fileStream, fileName, "profiles");
        artist.ProfileImage = filePath;
        await _artistRepository.UpdateAsync(artist);

        return filePath;
    }

    public async Task<ArtistStatsDto> GetStatsAsync(Guid id)
    {
        var artist = await _artistRepository.GetWithSongsAsync(id);
        if (artist == null)
        {
            throw new InvalidOperationException("Artist not found");
        }

        var totalPlays = artist.Songs.Sum(s => s.TotalPlays);
        var totalLikes = artist.Songs.Sum(s => s.TotalLikes);
        var totalListeningSeconds = artist.Songs.Sum(s => s.TotalListeningSeconds);
        var revenue = await _songPurchaseRepository.GetTotalRevenueForArtistAsync(id);

        return new ArtistStatsDto
        {
            TotalSongs = artist.Songs.Count,
            TotalPlays = totalPlays,
            TotalListeningHours = totalListeningSeconds / 3600,
            TotalLikes = totalLikes,
            MonthlyListeners = artist.Subscribers.Count,
            Revenue = revenue
        };
    }

    public async Task<IEnumerable<ArtistDto>> GetLiveArtistsAsync()
    {
        var artists = await _artistRepository.GetLiveArtistsAsync();
        return _mapper.Map<IEnumerable<ArtistDto>>(artists);
    }

    public async Task<IEnumerable<ArtistDto>> GetTopArtistsAsync(int take = 10)
    {
        var artists = await _artistRepository.GetTopArtistsAsync(take);
        return _mapper.Map<IEnumerable<ArtistDto>>(artists);
    }

    public async Task<IEnumerable<ArtistDto>> SearchArtistsAsync(string query)
    {
        var artists = await _artistRepository.SearchArtistsAsync(query);
        return _mapper.Map<IEnumerable<ArtistDto>>(artists);
    }

    public async Task StartLiveStreamAsync(Guid id, string genre)
    {
        var artist = await _artistRepository.GetByIdAsync(id);
        if (artist == null)
        {
            throw new InvalidOperationException("Artist not found");
        }

        artist.IsLive = true;
        artist.LiveStreamGenre = genre;
        artist.ListenersCount = 0;
        await _artistRepository.UpdateAsync(artist);
    }

    public async Task StopLiveStreamAsync(Guid id)
    {
        var artist = await _artistRepository.GetByIdAsync(id);
        if (artist == null)
        {
            throw new InvalidOperationException("Artist not found");
        }

        artist.IsLive = false;
        artist.LiveStreamGenre = null;
        artist.ListenersCount = 0;
        await _artistRepository.UpdateAsync(artist);
    }

    public async Task UpdateListenerCountAsync(Guid id, int count)
    {
        var artist = await _artistRepository.GetByIdAsync(id);
        if (artist == null) return;

        artist.ListenersCount = count;
        await _artistRepository.UpdateAsync(artist);
    }

    public async Task ChangePasswordAsync(Guid id, string currentPassword, string newPassword)
    {
        var artist = await _artistRepository.GetByIdAsync(id);
        if (artist == null)
        {
            throw new InvalidOperationException("Artist not found");
        }

        if (!BCrypt.Net.BCrypt.Verify(currentPassword, artist.PasswordHash))
        {
            throw new InvalidOperationException("Invalid current password");
        }

        artist.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _artistRepository.UpdateAsync(artist);
    }
}
