namespace backend.Entities;

public class Album : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? CoverArt { get; set; }
    public DateTime ReleaseDate { get; set; }
    
    // Foreign keys
    public Guid ArtistId { get; set; }
    
    // Navigation properties
    public virtual Artist Artist { get; set; } = null!;
    public virtual ICollection<Song> Songs { get; set; } = new List<Song>();
}
