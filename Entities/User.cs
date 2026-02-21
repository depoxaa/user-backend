namespace backend.Entities;

public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public string Role { get; set; } = "User";
    public bool IsEmailConfirmed { get; set; }
    public string? EmailConfirmationCode { get; set; }
    public DateTime? EmailConfirmationCodeExpiry { get; set; }
    public bool IsOnline { get; set; }
    public DateTime? LastSeen { get; set; }
    public string? CurrentlyListeningStatus { get; set; }
    
    // Live streaming playback tracking
    public Guid? CurrentSongId { get; set; }
    public double CurrentSongPosition { get; set; }
    public DateTime? CurrentSongUpdatedAt { get; set; }
    public bool IsPlaybackPaused { get; set; }
    
    // Navigation properties
    public virtual ICollection<Playlist> Playlists { get; set; } = new List<Playlist>();
    public virtual ICollection<Friendship> FriendshipsInitiated { get; set; } = new List<Friendship>();
    public virtual ICollection<Friendship> FriendshipsReceived { get; set; } = new List<Friendship>();
    public virtual ICollection<FriendRequest> SentFriendRequests { get; set; } = new List<FriendRequest>();
    public virtual ICollection<FriendRequest> ReceivedFriendRequests { get; set; } = new List<FriendRequest>();
    public virtual ICollection<SongLike> LikedSongs { get; set; } = new List<SongLike>();
    public virtual ICollection<ArtistSubscription> SubscribedArtists { get; set; } = new List<ArtistSubscription>();
    public virtual ICollection<PlaylistView> PlaylistViews { get; set; } = new List<PlaylistView>();
    public virtual ICollection<SongPlay> SongPlays { get; set; } = new List<SongPlay>();
    public virtual ICollection<SongPurchase> PurchasedSongs { get; set; } = new List<SongPurchase>();
}

