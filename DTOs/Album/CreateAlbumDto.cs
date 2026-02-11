using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Album;

public class CreateAlbumDto
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public DateTime ReleaseDate { get; set; }
}
