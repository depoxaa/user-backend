using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Entities;
using backend.Repositories.Interfaces;

namespace backend.Repositories.Implementations;

public class PremiumPaymentRepository : Repository<PremiumPayment>, IPremiumPaymentRepository
{
    public PremiumPaymentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<PremiumPayment>> GetByUserAsync(Guid userId)
    {
        return await _dbSet
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }
}
