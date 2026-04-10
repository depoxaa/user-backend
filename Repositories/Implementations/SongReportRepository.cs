using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Entities;
using backend.Repositories.Interfaces;

namespace backend.Repositories.Implementations;

public class SongReportRepository : Repository<SongReport>, ISongReportRepository
{
    public SongReportRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<bool> HasPendingReportAsync(Guid userId, Guid songId)
    {
        return await _dbSet.AnyAsync(r =>
            r.ReportedByUserId == userId &&
            r.SongId == songId &&
            r.Status == ReportStatus.Pending);
    }

    public async Task<IEnumerable<SongReport>> GetByStatusAsync(ReportStatus status, int page = 1, int pageSize = 20)
    {
        return await _dbSet
            .Include(r => r.ReportedByUser)
            .Include(r => r.Song)
                .ThenInclude(s => s.Artist)
            .Where(r => r.Status == status)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetCountByStatusAsync(ReportStatus status)
    {
        return await _dbSet.CountAsync(r => r.Status == status);
    }

    public async Task<SongReport?> GetWithDetailsAsync(Guid id)
    {
        return await _dbSet
            .Include(r => r.ReportedByUser)
            .Include(r => r.Song)
                .ThenInclude(s => s.Artist)
            .FirstOrDefaultAsync(r => r.Id == id);
    }
}
