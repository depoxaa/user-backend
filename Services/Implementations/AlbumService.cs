using AutoMapper;
using backend.DTOs.Album;
using backend.Entities;
using backend.Repositories.Interfaces;
using backend.Services.Interfaces;

namespace backend.Services.Implementations;

public class AlbumService : IAlbumService
{
    private readonly IAlbumRepository _albumRepository;
    private readonly IFileService _fileService;
    private readonly IMapper _mapper;

    public AlbumService(
        IAlbumRepository albumRepository,
        IFileService fileService,
        IMapper mapper)
    {
        _albumRepository = albumRepository;
        _fileService = fileService;
        _mapper = mapper;
    }

    public async Task<AlbumDto?> GetByIdAsync(Guid id)
    {
        var album = await _albumRepository.GetByIdAsync(id);
        return album == null ? null : _mapper.Map<AlbumDto>(album);
    }

    public async Task<AlbumDetailDto?> GetDetailAsync(Guid id)
    {
        var album = await _albumRepository.GetWithSongsAsync(id);
        return album == null ? null : _mapper.Map<AlbumDetailDto>(album);
    }

    public async Task<IEnumerable<AlbumDto>> GetByArtistAsync(Guid artistId)
    {
        var albums = await _albumRepository.GetByArtistAsync(artistId);
        return _mapper.Map<IEnumerable<AlbumDto>>(albums);
    }

    public async Task<AlbumDto> CreateAsync(Guid artistId, CreateAlbumDto dto, Stream? coverStream = null, string? coverFileName = null)
    {
        string? coverPath = null;
        if (coverStream != null && coverFileName != null)
        {
            coverPath = await _fileService.SaveImageFileAsync(coverStream, coverFileName, "covers");
        }

        var album = new Album
        {
            Title = dto.Title,
            ArtistId = artistId,
            ReleaseDate = dto.ReleaseDate,
            CoverArt = coverPath
        };

        await _albumRepository.AddAsync(album);
        return _mapper.Map<AlbumDto>(album);
    }

    public async Task<AlbumDto> UpdateAsync(Guid id, Guid artistId, CreateAlbumDto dto)
    {
        var album = await _albumRepository.GetByIdAsync(id);
        if (album == null)
        {
            throw new InvalidOperationException("Album not found");
        }

        if (album.ArtistId != artistId)
        {
            throw new UnauthorizedAccessException("You don't have permission to update this album");
        }

        album.Title = dto.Title;
        album.ReleaseDate = dto.ReleaseDate;

        await _albumRepository.UpdateAsync(album);
        return _mapper.Map<AlbumDto>(album);
    }

    public async Task<string?> UpdateCoverAsync(Guid id, Guid artistId, Stream fileStream, string fileName)
    {
        var album = await _albumRepository.GetByIdAsync(id);
        if (album == null)
        {
            throw new InvalidOperationException("Album not found");
        }

        if (album.ArtistId != artistId)
        {
            throw new UnauthorizedAccessException("You don't have permission to update this album");
        }

        // Delete old cover if exists
        if (!string.IsNullOrEmpty(album.CoverArt))
        {
            await _fileService.DeleteFileAsync(album.CoverArt);
        }

        var filePath = await _fileService.SaveImageFileAsync(fileStream, fileName, "covers");
        album.CoverArt = filePath;
        await _albumRepository.UpdateAsync(album);

        return filePath;
    }

    public async Task DeleteAsync(Guid id, Guid artistId)
    {
        var album = await _albumRepository.GetByIdAsync(id);
        if (album == null)
        {
            throw new InvalidOperationException("Album not found");
        }

        if (album.ArtistId != artistId)
        {
            throw new UnauthorizedAccessException("You don't have permission to delete this album");
        }

        // Delete cover
        if (!string.IsNullOrEmpty(album.CoverArt))
        {
            await _fileService.DeleteFileAsync(album.CoverArt);
        }

        await _albumRepository.DeleteAsync(album);
    }
}
