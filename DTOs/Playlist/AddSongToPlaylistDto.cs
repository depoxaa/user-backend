using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Playlist;

public class AddSongToPlaylistDto
{
    [Required]
    public Guid SongId { get; set; }
}
