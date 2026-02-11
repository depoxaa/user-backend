using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Song;

public class UpdateSongDto
{
    [StringLength(200)]
    public string? Title { get; set; }

    public Guid? GenreId { get; set; }

    public Guid? AlbumId { get; set; }

    public DateTime? ReleaseDate { get; set; }
}
