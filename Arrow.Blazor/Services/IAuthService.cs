using Arrow.Blazor.Models;

namespace Arrow.Blazor.Services;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(string email, string password, string confirmPassword, CancellationToken cancellationToken = default);
    Task<AuthResult> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
    Task LogoutAsync();
}
