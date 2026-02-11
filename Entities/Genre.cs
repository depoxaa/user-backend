namespace backend.Entities;

public class Genre : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    // Navigation properties
    public virtual ICollection<Song> Songs { get; set; } = new List<Song>();
}
