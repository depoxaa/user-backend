using backend.DTOs.Song;
using backend.Entities;
using backend.Repositories.Interfaces;
using backend.Services.Interfaces;

namespace backend.Services.Implementations;

public class SongReportService : ISongReportService
{
    private readonly ISongReportRepository _reportRepository;
    private readonly ISongRepository _songRepository;

    public SongReportService(ISongReportRepository reportRepository, ISongRepository songRepository)
    {
        _reportRepository = reportRepository;
        _songRepository = songRepository;
    }

    public async Task<SongReportDto> CreateReportAsync(Guid userId, Guid songId, CreateSongReportDto dto)
    {
        var song = await _songRepository.GetByIdAsync(songId);
        if (song == null)
            throw new InvalidOperationException("Song not found");

        if (await _reportRepository.HasPendingReportAsync(userId, songId))
            throw new InvalidOperationException("DUPLICATE_PENDING_REPORT");

        if (dto.Description.Length < 20 || dto.Description.Length > 500)
            throw new InvalidOperationException("Description must be between 20 and 500 characters");

        if (!Enum.TryParse<ReportReason>(dto.Reason, true, out var reason))
            throw new InvalidOperationException("Invalid report reason");

        var report = new SongReport
        {
            ReportedByUserId = userId,
            SongId = songId,
            Reason = reason,
            Description = dto.Description,
            EvidenceUrl = dto.EvidenceUrl,
            Status = ReportStatus.Pending
        };

        await _reportRepository.AddAsync(report);

        var created = await _reportRepository.GetWithDetailsAsync(report.Id);
        return MapToDto(created!);
    }

    public async Task<IEnumerable<SongReportDto>> GetReportsByStatusAsync(ReportStatus status, int page = 1, int pageSize = 20)
    {
        var reports = await _reportRepository.GetByStatusAsync(status, page, pageSize);
        return reports.Select(MapToDto);
    }

    public async Task<int> GetPendingCountAsync()
    {
        return await _reportRepository.GetCountByStatusAsync(ReportStatus.Pending);
    }

    public async Task BanSongAsync(Guid reportId, Guid adminId)
    {
        var report = await _reportRepository.GetWithDetailsAsync(reportId);
        if (report == null)
            throw new InvalidOperationException("Report not found");

        // Ban the song
        var song = await _songRepository.GetByIdAsync(report.SongId);
        if (song != null)
        {
            song.IsActive = false;
            await _songRepository.UpdateAsync(song);
        }

        // Update report
        report.Status = ReportStatus.Banned;
        report.ReviewedByAdminId = adminId;
        report.ReviewedAt = DateTime.UtcNow;
        await _reportRepository.UpdateAsync(report);
    }

    public async Task DismissReportAsync(Guid reportId, Guid adminId)
    {
        var report = await _reportRepository.GetWithDetailsAsync(reportId);
        if (report == null)
            throw new InvalidOperationException("Report not found");

        report.Status = ReportStatus.Dismissed;
        report.ReviewedByAdminId = adminId;
        report.ReviewedAt = DateTime.UtcNow;
        await _reportRepository.UpdateAsync(report);
    }

    private static SongReportDto MapToDto(SongReport report)
    {
        return new SongReportDto
        {
            Id = report.Id,
            SongId = report.SongId,
            SongTitle = report.Song?.Title ?? string.Empty,
            ArtistName = report.Song?.Artist?.Name ?? string.Empty,
            ReportedByUserId = report.ReportedByUserId,
            ReportedByUsername = report.ReportedByUser?.Username ?? string.Empty,
            Reason = report.Reason.ToString(),
            Description = report.Description,
            EvidenceUrl = report.EvidenceUrl,
            Status = report.Status.ToString(),
            CreatedAt = report.CreatedAt,
            ReviewedByAdminId = report.ReviewedByAdminId,
            ReviewedAt = report.ReviewedAt
        };
    }
}
