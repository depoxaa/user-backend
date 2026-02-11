using backend.DTOs.Song;

namespace backend.DTOs.Album;

public class AlbumDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? CoverArt { get; set; }
    public DateTime ReleaseDate { get; set; }
    public ArtistInfoDto Artist { get; set; } = null!;
    public int SongsCount { get; set; }
}

public class AlbumDetailDto : AlbumDto
{
    public List<SongDto> Songs { get; set; } = new();
}
