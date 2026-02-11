namespace backend.DTOs.User;

public class UpdatePlaybackDto
{
    public Guid? SongId { get; set; }
    public double Position { get; set; }
    public bool IsPaused { get; set; }
}
