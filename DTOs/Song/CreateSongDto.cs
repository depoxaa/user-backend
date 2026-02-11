using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Song;

public class CreateSongDto
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public Guid GenreId { get; set; }

    public Guid? AlbumId { get; set; }

    [Required]
    public DateTime ReleaseDate { get; set; }
}
