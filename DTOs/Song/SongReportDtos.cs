namespace backend.DTOs.Song;

public class CreateSongReportDto
{
    public string Reason { get; set; } = string.Empty; // copyright, explicit, fake, spam, other
    public string Description { get; set; } = string.Empty;
    public string? EvidenceUrl { get; set; }
}

public class SongReportDto
{
    public Guid Id { get; set; }
    public Guid SongId { get; set; }
    public string SongTitle { get; set; } = string.Empty;
    public string ArtistName { get; set; } = string.Empty;
    public Guid ReportedByUserId { get; set; }
    public string ReportedByUsername { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? EvidenceUrl { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Guid? ReviewedByAdminId { get; set; }
    public DateTime? ReviewedAt { get; set; }
}
