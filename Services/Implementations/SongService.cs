using AutoMapper;
using backend.DTOs.Song;
using backend.Entities;
using backend.Repositories.Interfaces;
using backend.Services.Interfaces;

namespace backend.Services.Implementations;

public class SongService : ISongService
{
    private readonly ISongRepository _songRepository;
    private readonly ISongLikeRepository _songLikeRepository;
    private readonly ISongPurchaseRepository _songPurchaseRepository;
    private readonly IFileService _fileService;
    private readonly IMapper _mapper;

    public SongService(
        ISongRepository songRepository,
        ISongLikeRepository songLikeRepository,
        ISongPurchaseRepository songPurchaseRepository,
        IFileService fileService,
        IMapper mapper)
    {
        _songRepository = songRepository;
        _songLikeRepository = songLikeRepository;
        _songPurchaseRepository = songPurchaseRepository;
        _fileService = fileService;
        _mapper = mapper;
    }

    public async Task<SongDto?> GetByIdAsync(Guid id, Guid? userId = null)
    {
        var song = await _songRepository.GetWithDetailsAsync(id);
        if (song == null) return null;

        var dto = _mapper.Map<SongDto>(song);
        if (userId.HasValue)
        {
            dto.IsLiked = await _songLikeRepository.HasUserLikedSongAsync(userId.Value, id);
            dto.IsPurchased = song.IsFree || await _songPurchaseRepository.HasUserPurchasedSongAsync(userId.Value, id);
        }
        else
        {
            dto.IsPurchased = song.IsFree;
        }
        return dto;
    }

    public async Task<IEnumerable<SongDto>> GetByArtistAsync(Guid artistId, Guid? userId = null)
    {
        var songs = await _songRepository.GetByArtistAsync(artistId);
        return await MapSongsWithLikesAndPurchases(songs, userId);
    }

    public async Task<IEnumerable<SongDto>> GetByAlbumAsync(Guid albumId, Guid? userId = null)
    {
        var songs = await _songRepository.GetByAlbumAsync(albumId);
        return await MapSongsWithLikesAndPurchases(songs, userId);
    }

    public async Task<IEnumerable<SongDto>> GetByGenreAsync(Guid genreId, Guid? userId = null)
    {
        var songs = await _songRepository.GetByGenreAsync(genreId);
        return await MapSongsWithLikesAndPurchases(songs, userId);
    }

    public async Task<IEnumerable<SongDto>> SearchAsync(string query, Guid? userId = null)
    {
        var songs = await _songRepository.SearchSongsAsync(query);
        return await MapSongsWithLikesAndPurchases(songs, userId);
    }

    public async Task<IEnumerable<SongDto>> GetTopSongsAsync(int take = 10, Guid? userId = null)
    {
        var songs = await _songRepository.GetTopSongsAsync(take);
        return await MapSongsWithLikesAndPurchases(songs, userId);
    }

    public async Task<IEnumerable<SongDto>> GetRecentSongsAsync(int take = 20, Guid? userId = null)
    {
        var songs = await _songRepository.GetRecentSongsAsync(take);
        return await MapSongsWithLikesAndPurchases(songs, userId);
    }

    public async Task<SongDto> CreateAsync(Guid artistId, CreateSongDto dto, Stream audioStream, string audioFileName, Stream? coverStream = null, string? coverFileName = null)
    {
        var audioPath = await _fileService.SaveAudioFileAsync(audioStream, audioFileName, artistId);
        var duration = _fileService.GetAudioDuration(audioPath);

        string? coverPath = null;
        if (coverStream != null && coverFileName != null)
        {
            coverPath = await _fileService.SaveImageFileAsync(coverStream, coverFileName, "covers");
        }

        var song = new Song
        {
            Title = dto.Title,
            ArtistId = artistId,
            GenreId = dto.GenreId,
            AlbumId = dto.AlbumId,
            FilePath = audioPath,
            CoverArt = coverPath,
            Duration = duration,
            ReleaseDate = dto.ReleaseDate,
            Price = dto.Price,
            IsFree = dto.Price <= 0
        };

        await _songRepository.AddAsync(song);
        return await GetByIdAsync(song.Id) ?? throw new InvalidOperationException("Failed to create song");
    }

    public async Task<SongDto> UpdateAsync(Guid id, Guid artistId, UpdateSongDto dto)
    {
        var song = await _songRepository.GetByIdAsync(id);
        if (song == null)
        {
            throw new InvalidOperationException("Song not found");
        }

        if (song.ArtistId != artistId)
        {
            throw new UnauthorizedAccessException("You don't have permission to update this song");
        }

        if (!string.IsNullOrEmpty(dto.Title))
            song.Title = dto.Title;
        if (dto.GenreId.HasValue)
            song.GenreId = dto.GenreId.Value;
        if (dto.AlbumId.HasValue)
            song.AlbumId = dto.AlbumId.Value;
        if (dto.ReleaseDate.HasValue)
            song.ReleaseDate = dto.ReleaseDate.Value;
        if (dto.Price.HasValue)
        {
            song.Price = dto.Price.Value;
            song.IsFree = dto.Price.Value <= 0;
        }

        await _songRepository.UpdateAsync(song);
        return await GetByIdAsync(id) ?? throw new InvalidOperationException("Failed to update song");
    }

    public async Task<string?> UpdateCoverAsync(Guid id, Guid artistId, Stream fileStream, string fileName)
    {
        var song = await _songRepository.GetByIdAsync(id);
        if (song == null)
        {
            throw new InvalidOperationException("Song not found");
        }

        if (song.ArtistId != artistId)
        {
            throw new UnauthorizedAccessException("You don't have permission to update this song");
        }

        if (!string.IsNullOrEmpty(song.CoverArt))
        {
            await _fileService.DeleteFileAsync(song.CoverArt);
        }

        var filePath = await _fileService.SaveImageFileAsync(fileStream, fileName, "covers");
        song.CoverArt = filePath;
        await _songRepository.UpdateAsync(song);

        return filePath;
    }

    public async Task DeleteAsync(Guid id, Guid artistId)
    {
        var song = await _songRepository.GetByIdAsync(id);
        if (song == null)
        {
            throw new InvalidOperationException("Song not found");
        }

        if (song.ArtistId != artistId)
        {
            throw new UnauthorizedAccessException("You don't have permission to delete this song");
        }

        await _fileService.DeleteFileAsync(song.FilePath);
        if (!string.IsNullOrEmpty(song.CoverArt))
        {
            await _fileService.DeleteFileAsync(song.CoverArt);
        }

        await _songRepository.DeleteAsync(song);
    }

    public async Task<bool> ToggleLikeAsync(Guid songId, Guid userId)
    {
        var song = await _songRepository.GetByIdAsync(songId);
        if (song == null)
        {
            throw new InvalidOperationException("Song not found");
        }

        var existingLike = await _songLikeRepository.GetLikeAsync(userId, songId);
        
        if (existingLike != null)
        {
            await _songLikeRepository.DeleteAsync(existingLike);
            song.TotalLikes--;
            await _songRepository.UpdateAsync(song);
            return false;
        }
        else
        {
            var like = new SongLike
            {
                UserId = userId,
                SongId = songId
            };
            await _songLikeRepository.AddAsync(like);
            song.TotalLikes++;
            await _songRepository.UpdateAsync(song);
            return true;
        }
    }

    public async Task RecordPlayAsync(Guid songId, Guid userId, int listeningSeconds)
    {
        var song = await _songRepository.GetByIdAsync(songId);
        if (song == null) return;

        song.TotalPlays++;
        song.TotalListeningSeconds += listeningSeconds;
        await _songRepository.UpdateAsync(song);
    }

    public async Task<IEnumerable<SongDto>> GetLikedSongsAsync(Guid userId)
    {
        var songs = await _songLikeRepository.GetLikedSongsByUserAsync(userId);
        var dtos = _mapper.Map<IEnumerable<SongDto>>(songs).ToList();
        var purchasedIds = (await _songPurchaseRepository.GetPurchasedSongIdsByUserAsync(userId)).ToHashSet();

        foreach (var dto in dtos)
        {
            dto.IsLiked = true;
            dto.IsPurchased = dto.IsFree || purchasedIds.Contains(dto.Id);
        }
        return dtos;
    }

    public async Task<Stream?> GetAudioStreamAsync(Guid songId, Guid? userId = null)
    {
        var song = await _songRepository.GetByIdAsync(songId);
        if (song == null) return null;

        if (!song.IsFree)
        {
            if (!userId.HasValue)
                return null;

            var hasPurchased = await _songPurchaseRepository.HasUserPurchasedSongAsync(userId.Value, songId);
            if (!hasPurchased)
                return null;
        }
        
        return _fileService.GetFileStream(song.FilePath);
    }

    public async Task<IEnumerable<SongDto>> GetPurchasedSongsAsync(Guid userId)
    {
        var songs = await _songPurchaseRepository.GetPurchasedSongsByUserAsync(userId);
        var dtos = _mapper.Map<IEnumerable<SongDto>>(songs).ToList();
        foreach (var dto in dtos)
        {
            dto.IsPurchased = true;
            dto.IsLiked = await _songLikeRepository.HasUserLikedSongAsync(userId, dto.Id);
        }
        return dtos;
    }

    private async Task<IEnumerable<SongDto>> MapSongsWithLikesAndPurchases(IEnumerable<Song> songs, Guid? userId)
    {
        var dtos = _mapper.Map<IEnumerable<SongDto>>(songs).ToList();
        
        if (userId.HasValue)
        {
            var purchasedIds = (await _songPurchaseRepository.GetPurchasedSongIdsByUserAsync(userId.Value)).ToHashSet();

            foreach (var dto in dtos)
            {
                dto.IsLiked = await _songLikeRepository.HasUserLikedSongAsync(userId.Value, dto.Id);
                dto.IsPurchased = dto.IsFree || purchasedIds.Contains(dto.Id);
            }
        }
        else
        {
            foreach (var dto in dtos)
            {
                dto.IsPurchased = dto.IsFree;
            }
        }
        
        return dtos;
    }
}
