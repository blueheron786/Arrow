namespace Arrow.Blazor.Services;

public interface IPasswordResetService
{
    Task<bool> RequestPasswordResetAsync(string email);
    Task<bool> ResetPasswordAsync(Guid token, string newPassword);
    Task<bool> ValidateResetTokenAsync(Guid token);
}
