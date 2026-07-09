using System.ComponentModel.DataAnnotations;

namespace LearningLab.Data.Models.DTOs.Auth;

public class LoginUserRequest
{
    [Required]
    public required string Username { get; init; }

    [Required]
    [MinLength(6)]
    public required string Password { get; init; }
}
