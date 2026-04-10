using backend.DTOs.Song;
using backend.Entities;

namespace backend.Services.Interfaces;

public interface ISongReportService
{
    Task<SongReportDto> CreateReportAsync(Guid userId, Guid songId, CreateSongReportDto dto);
    Task<IEnumerable<SongReportDto>> GetReportsByStatusAsync(ReportStatus status, int page = 1, int pageSize = 20);
    Task<int> GetPendingCountAsync();
    Task BanSongAsync(Guid reportId, Guid adminId);
    Task DismissReportAsync(Guid reportId, Guid adminId);
}
