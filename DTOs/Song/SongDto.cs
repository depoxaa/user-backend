namespace backend.DTOs.Song;

public class SongDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? CoverArt { get; set; }
    public string Duration { get; set; } = string.Empty;
    public DateTime ReleaseDate { get; set; }
    public long TotalPlays { get; set; }
    public long TotalLikes { get; set; }
    public string TotalListeningTime { get; set; } = string.Empty;
    public ArtistInfoDto Artist { get; set; } = null!;
    public AlbumInfoDto? Album { get; set; }
    public GenreInfoDto Genre { get; set; } = null!;
    public bool IsLiked { get; set; }
}

public class ArtistInfoDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ProfileImage { get; set; }
    public string? Bio { get; set; }
}

public class AlbumInfoDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? CoverArt { get; set; }
}

public class GenreInfoDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
