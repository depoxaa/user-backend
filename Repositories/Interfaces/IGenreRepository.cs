using backend.Entities;

namespace backend.Repositories.Interfaces;

public interface IGenreRepository : IRepository<Genre>
{
    Task<Genre?> GetByNameAsync(string name);
}
