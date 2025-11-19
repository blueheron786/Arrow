namespace Arrow.Blazor.Models;

public class PasswordResetToken
{
    public Guid Token { get; set; }
    public Guid UserId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset ExpiresAtUtc { get; set; }
    public bool IsUsed { get; set; }
}
