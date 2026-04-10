using backend.Entities;

namespace backend.Repositories.Interfaces;

public interface IPremiumPaymentRepository : IRepository<PremiumPayment>
{
    Task<IEnumerable<PremiumPayment>> GetByUserAsync(Guid userId);
}
