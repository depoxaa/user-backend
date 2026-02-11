using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.User;

public class UpdateUserDto
{
    [StringLength(50, MinimumLength = 3)]
    public string? Username { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    public string? CurrentlyListeningStatus { get; set; }
}
