using System.ComponentModel.DataAnnotations;

namespace LearningLab.Data.Models.DTOs.Auth;

public class RegisterUserRequest
{
    [Required]
    public required string Username { get; init; }

    [Required]
    [MinLength(6)]
    public required string Password { get; init; }

    [Required]
    public required string FirstName { get; init; }

    [Required]
    public required string LastName { get; init; }
}
