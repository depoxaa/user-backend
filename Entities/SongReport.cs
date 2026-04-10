namespace backend.Entities;

public enum ReportReason
{
    Copyright,
    Explicit,
    Fake,
    Spam,
    Other
}

public enum ReportStatus
{
    Pending,
    Banned,
    Dismissed
}

public class SongReport : BaseEntity
{
    public Guid ReportedByUserId { get; set; }
    public Guid SongId { get; set; }
    public ReportReason Reason { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? EvidenceUrl { get; set; }
    public ReportStatus Status { get; set; } = ReportStatus.Pending;
    public Guid? ReviewedByAdminId { get; set; }
    public DateTime? ReviewedAt { get; set; }

    // Navigation properties
    public virtual User ReportedByUser { get; set; } = null!;
    public virtual Song Song { get; set; } = null!;
}
