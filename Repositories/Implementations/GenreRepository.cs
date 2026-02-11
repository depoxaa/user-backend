using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Entities;
using backend.Repositories.Interfaces;

namespace backend.Repositories.Implementations;

public class GenreRepository : Repository<Genre>, IGenreRepository
{
    public GenreRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Genre?> GetByNameAsync(string name)
    {
        return await _dbSet.FirstOrDefaultAsync(g => g.Name.ToLower() == name.ToLower());
    }
}
