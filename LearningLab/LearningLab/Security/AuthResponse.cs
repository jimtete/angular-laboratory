namespace LearningLab.Security;

public class AuthResponse
{
    public required string AccessToken { get; init; }

    public string TokenType { get; init; } = "Bearer";

    public DateTime ExpiresAtUtc { get; init; }
}
