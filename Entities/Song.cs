namespace backend.Entities;

public class Song : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? CoverArt { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public DateTime ReleaseDate { get; set; }
    public long TotalPlays { get; set; }
    public long TotalLikes { get; set; }
    public long TotalListeningSeconds { get; set; }
    public decimal Price { get; set; } = 0;
    public bool IsFree { get; set; } = true;
    
    // Foreign keys
    public Guid ArtistId { get; set; }
    public Guid? AlbumId { get; set; }
    public Guid GenreId { get; set; }
    
    // Navigation properties
    public virtual Artist Artist { get; set; } = null!;
    public virtual Album? Album { get; set; }
    public virtual Genre Genre { get; set; } = null!;
    public virtual ICollection<PlaylistSong> PlaylistSongs { get; set; } = new List<PlaylistSong>();
    public virtual ICollection<SongLike> Likes { get; set; } = new List<SongLike>();
    public virtual ICollection<SongPlay> Plays { get; set; } = new List<SongPlay>();
    public virtual ICollection<SongPurchase> Purchases { get; set; } = new List<SongPurchase>();
}
