using System.ComponentModel.DataAnnotations;

namespace LearningLab.Data.Models.Auth;

public class LoginRequest
{
    [Required]
    [EmailAddress]
    public required string Email { get; init; }

    [Required]
    [MinLength(6)]
    public required string Password { get; init; }
}
