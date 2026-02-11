namespace backend.Entities;

public class Artist : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? ProfileImage { get; set; }
    public bool IsEmailConfirmed { get; set; }
    public string? EmailConfirmationCode { get; set; }
    public DateTime? EmailConfirmationCodeExpiry { get; set; }
    public bool IsLive { get; set; }
    public string? LiveStreamGenre { get; set; }
    public int ListenersCount { get; set; }
    
    // Navigation properties
    public virtual ICollection<Song> Songs { get; set; } = new List<Song>();
    public virtual ICollection<Album> Albums { get; set; } = new List<Album>();
    public virtual ICollection<ArtistSubscription> Subscribers { get; set; } = new List<ArtistSubscription>();
}
