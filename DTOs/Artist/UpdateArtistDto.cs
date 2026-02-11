using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Artist;

public class UpdateArtistDto
{
    [StringLength(100, MinimumLength = 2)]
    public string? Name { get; set; }

    [StringLength(1000)]
    public string? Bio { get; set; }
}
