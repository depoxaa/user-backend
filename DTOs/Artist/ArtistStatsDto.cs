namespace backend.DTOs.Artist;

public class ArtistStatsDto
{
    public int TotalSongs { get; set; }
    public long TotalPlays { get; set; }
    public long TotalListeningHours { get; set; }
    public long TotalLikes { get; set; }
    public int MonthlyListeners { get; set; }
    public decimal Revenue { get; set; }
}
