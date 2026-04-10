namespace backend.Services.Interfaces;

public interface IStreamViewerService
{
    Task JoinStreamAsync(Guid streamHostId, Guid userId);
    Task LeaveStreamAsync(Guid streamHostId, Guid userId);
    Task<int> GetActiveViewerCountAsync(Guid streamHostId);
    Task EndStreamAsync(Guid streamHostId);
}
