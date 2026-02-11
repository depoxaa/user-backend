using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using backend.Entities;

namespace backend.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ConfigureWarnings(warnings =>
            warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Artist> Artists => Set<Artist>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<Album> Albums => Set<Album>();
    public DbSet<Song> Songs => Set<Song>();
    public DbSet<Playlist> Playlists => Set<Playlist>();
    public DbSet<PlaylistSong> PlaylistSongs => Set<PlaylistSong>();
    public DbSet<PlaylistView> PlaylistViews => Set<PlaylistView>();
    public DbSet<Friendship> Friendships => Set<Friendship>();
    public DbSet<FriendRequest> FriendRequests => Set<FriendRequest>();
    public DbSet<SongLike> SongLikes => Set<SongLike>();
    public DbSet<SongPlay> SongPlays => Set<SongPlay>();
    public DbSet<ArtistSubscription> ArtistSubscriptions => Set<ArtistSubscription>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Username).IsUnique();
            entity.HasIndex(u => u.Email).IsUnique();
        });

        // Artist configuration
        modelBuilder.Entity<Artist>(entity =>
        {
            entity.HasIndex(a => a.Email).IsUnique();
        });

        // Genre configuration
        modelBuilder.Entity<Genre>(entity =>
        {
            entity.HasIndex(g => g.Name).IsUnique();
        });

        // Song configuration
        modelBuilder.Entity<Song>(entity =>
        {
            entity.HasOne(s => s.Artist)
                .WithMany(a => a.Songs)
                .HasForeignKey(s => s.ArtistId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(s => s.Album)
                .WithMany(a => a.Songs)
                .HasForeignKey(s => s.AlbumId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(s => s.Genre)
                .WithMany(g => g.Songs)
                .HasForeignKey(s => s.GenreId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Album configuration
        modelBuilder.Entity<Album>(entity =>
        {
            entity.HasOne(a => a.Artist)
                .WithMany(ar => ar.Albums)
                .HasForeignKey(a => a.ArtistId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Playlist configuration
        modelBuilder.Entity<Playlist>(entity =>
        {
            entity.HasOne(p => p.User)
                .WithMany(u => u.Playlists)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // PlaylistSong configuration
        modelBuilder.Entity<PlaylistSong>(entity =>
        {
            entity.HasOne(ps => ps.Playlist)
                .WithMany(p => p.PlaylistSongs)
                .HasForeignKey(ps => ps.PlaylistId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ps => ps.Song)
                .WithMany(s => s.PlaylistSongs)
                .HasForeignKey(ps => ps.SongId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(ps => new { ps.PlaylistId, ps.SongId }).IsUnique();
        });

        // PlaylistView configuration
        modelBuilder.Entity<PlaylistView>(entity =>
        {
            entity.HasOne(pv => pv.Playlist)
                .WithMany(p => p.Views)
                .HasForeignKey(pv => pv.PlaylistId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(pv => pv.User)
                .WithMany(u => u.PlaylistViews)
                .HasForeignKey(pv => pv.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Friendship configuration
        modelBuilder.Entity<Friendship>(entity =>
        {
            entity.HasOne(f => f.User)
                .WithMany(u => u.FriendshipsInitiated)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(f => f.Friend)
                .WithMany(u => u.FriendshipsReceived)
                .HasForeignKey(f => f.FriendId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(f => new { f.UserId, f.FriendId }).IsUnique();
        });

        // FriendRequest configuration
        modelBuilder.Entity<FriendRequest>(entity =>
        {
            entity.HasOne(fr => fr.Sender)
                .WithMany(u => u.SentFriendRequests)
                .HasForeignKey(fr => fr.SenderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(fr => fr.Receiver)
                .WithMany(u => u.ReceivedFriendRequests)
                .HasForeignKey(fr => fr.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(fr => new { fr.SenderId, fr.ReceiverId }).IsUnique();
        });

        // SongLike configuration
        modelBuilder.Entity<SongLike>(entity =>
        {
            entity.HasOne(sl => sl.User)
                .WithMany(u => u.LikedSongs)
                .HasForeignKey(sl => sl.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(sl => sl.Song)
                .WithMany(s => s.Likes)
                .HasForeignKey(sl => sl.SongId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(sl => new { sl.UserId, sl.SongId }).IsUnique();
        });

        // SongPlay configuration
        modelBuilder.Entity<SongPlay>(entity =>
        {
            entity.HasOne(sp => sp.User)
                .WithMany(u => u.SongPlays)
                .HasForeignKey(sp => sp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(sp => sp.Song)
                .WithMany(s => s.Plays)
                .HasForeignKey(sp => sp.SongId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ArtistSubscription configuration
        modelBuilder.Entity<ArtistSubscription>(entity =>
        {
            entity.HasOne(asub => asub.User)
                .WithMany(u => u.SubscribedArtists)
                .HasForeignKey(asub => asub.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(asub => asub.Artist)
                .WithMany(a => a.Subscribers)
                .HasForeignKey(asub => asub.ArtistId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(asub => new { asub.UserId, asub.ArtistId }).IsUnique();
        });

        // Seed genres
        var genres = new[]
        {
            new Genre { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Pop", Description = "Popular music" },
            new Genre { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "Rock", Description = "Rock music" },
            new Genre { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "Hip Hop", Description = "Hip hop and rap music" },
            new Genre { Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), Name = "R&B", Description = "Rhythm and blues" },
            new Genre { Id = Guid.Parse("55555555-5555-5555-5555-555555555555"), Name = "Electronic", Description = "Electronic dance music" },
            new Genre { Id = Guid.Parse("66666666-6666-6666-6666-666666666666"), Name = "Jazz", Description = "Jazz music" },
            new Genre { Id = Guid.Parse("77777777-7777-7777-7777-777777777777"), Name = "Classical", Description = "Classical music" },
            new Genre { Id = Guid.Parse("88888888-8888-8888-8888-888888888888"), Name = "Country", Description = "Country music" },
            new Genre { Id = Guid.Parse("99999999-9999-9999-9999-999999999999"), Name = "Indie", Description = "Independent music" },
            new Genre { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Name = "Nature Sounds", Description = "Ambient nature sounds" }
        };
        modelBuilder.Entity<Genre>().HasData(genres);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<BaseEntity>();
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
