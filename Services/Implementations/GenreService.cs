using AutoMapper;
using backend.DTOs.Song;
using backend.Repositories.Interfaces;
using backend.Services.Interfaces;

namespace backend.Services.Implementations;

public class GenreService : IGenreService
{
    private readonly IGenreRepository _genreRepository;
    private readonly IMapper _mapper;

    public GenreService(IGenreRepository genreRepository, IMapper mapper)
    {
        _genreRepository = genreRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<GenreInfoDto>> GetAllAsync()
    {
        var genres = await _genreRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<GenreInfoDto>>(genres);
    }

    public async Task<GenreInfoDto?> GetByIdAsync(Guid id)
    {
        var genre = await _genreRepository.GetByIdAsync(id);
        return genre == null ? null : _mapper.Map<GenreInfoDto>(genre);
    }

    public async Task<GenreInfoDto?> GetByNameAsync(string name)
    {
        var genre = await _genreRepository.GetByNameAsync(name);
        return genre == null ? null : _mapper.Map<GenreInfoDto>(genre);
    }
}
