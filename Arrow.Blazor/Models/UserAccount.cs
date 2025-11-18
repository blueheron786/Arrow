namespace Arrow.Blazor.Models;

public sealed class UserAccount
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string PasswordHash { get; init; } = string.Empty;
    public DateTime CreatedAtUtc { get; init; }
}
