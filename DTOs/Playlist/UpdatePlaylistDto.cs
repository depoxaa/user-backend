using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Playlist;

public class UpdatePlaylistDto
{
    [StringLength(100)]
    public string? Name { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public string? Icon { get; set; }

    public string? Color { get; set; }

    public string? Status { get; set; }
}
