using backend.DTOs.Song;

namespace backend.Services.Interfaces;

public interface IGenreService
{
    Task<IEnumerable<GenreInfoDto>> GetAllAsync();
    Task<GenreInfoDto?> GetByIdAsync(Guid id);
    Task<GenreInfoDto?> GetByNameAsync(string name);
}
