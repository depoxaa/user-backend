using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Playlist;

public class CreatePlaylistDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public string Icon { get; set; } = "🎵";

    public string Color { get; set; } = "bg-blue-500";

    public string Status { get; set; } = "Public";
}
