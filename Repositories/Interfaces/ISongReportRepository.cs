using backend.Entities;

namespace backend.Repositories.Interfaces;

public interface ISongReportRepository : IRepository<SongReport>
{
    Task<bool> HasPendingReportAsync(Guid userId, Guid songId);
    Task<IEnumerable<SongReport>> GetByStatusAsync(ReportStatus status, int page = 1, int pageSize = 20);
    Task<int> GetCountByStatusAsync(ReportStatus status);
    Task<SongReport?> GetWithDetailsAsync(Guid id);
}
