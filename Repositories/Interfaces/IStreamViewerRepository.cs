using backend.Entities;

namespace backend.Repositories.Interfaces;

public interface IStreamViewerRepository : IRepository<StreamViewer>
{
    Task<int> GetActiveViewerCountAsync(Guid streamHostId);
    Task<StreamViewer?> GetActiveViewerAsync(Guid streamHostId, Guid userId);
    Task MarkViewerLeftAsync(Guid streamHostId, Guid userId);
    Task CleanupStaleViewersAsync(TimeSpan timeout);
    Task MarkAllViewersLeftAsync(Guid streamHostId);
}
