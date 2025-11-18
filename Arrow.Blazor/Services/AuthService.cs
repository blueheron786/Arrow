using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using Arrow.Blazor.Data;
using Arrow.Blazor.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Npgsql;

namespace Arrow.Blazor.Services;

public sealed class AuthService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IHttpContextAccessor httpContextAccessor,
    ILogger<AuthService> logger) : IAuthService
{
    private const int MinimumPasswordLength = 8;

    public async Task<AuthResult> RegisterAsync(string email, string password, string confirmPassword, CancellationToken cancellationToken = default)
    {
        if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
        {
            return AuthResult.Failure("Passwords do not match.");
        }

        if (!IsPasswordStrong(password))
        {
            return AuthResult.Failure($"Password must be at least {MinimumPasswordLength} characters long and contain letters and numbers.");
        }

        var sanitizedEmail = email.Trim();
        if (!IsValidEmail(sanitizedEmail))
        {
            return AuthResult.Failure("Please provide a valid email address.");
        }
        var existingUser = await userRepository.GetByEmailAsync(sanitizedEmail, cancellationToken);
        if (existingUser is not null)
        {
            return AuthResult.Failure("An account with this email already exists.");
        }

        var user = new UserAccount
        {
            Id = Guid.NewGuid(),
            Email = sanitizedEmail,
            PasswordHash = passwordHasher.Hash(password),
            CreatedAtUtc = DateTime.UtcNow
        };

        try
        {
            await userRepository.CreateAsync(user, cancellationToken);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            logger.LogWarning(ex, "Unique constraint violation when creating user {Email}", sanitizedEmail);
            return AuthResult.Failure("An account with this email already exists.");
        }

        await SignInAsync(user);
        return AuthResult.Success();
    }

    public async Task<AuthResult> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var sanitizedEmail = email.Trim();
        if (!IsValidEmail(sanitizedEmail))
        {
            return AuthResult.Failure("Invalid credentials.");
        }
        var user = await userRepository.GetByEmailAsync(sanitizedEmail, cancellationToken);

        if (user is null || !passwordHasher.Verify(password, user.PasswordHash))
        {
            return AuthResult.Failure("Invalid credentials.");
        }

        await SignInAsync(user);
        return AuthResult.Success();
    }

    public async Task LogoutAsync()
    {
        var httpContext = GetHttpContext();
        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    private async Task SignInAsync(UserAccount user)
    {
        var httpContext = GetHttpContext();
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.Email)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        var properties = new AuthenticationProperties
        {
            IsPersistent = false,
            IssuedUtc = DateTimeOffset.UtcNow
        };

        await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, properties);
    }

    private HttpContext GetHttpContext()
    {
        var context = httpContextAccessor.HttpContext;
        if (context is null)
        {
            throw new InvalidOperationException("No active HTTP context is available.");
        }

        return context;
    }

    private static bool IsPasswordStrong(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < MinimumPasswordLength)
        {
            return false;
        }

        var hasLetter = password.Any(char.IsLetter);
        var hasDigit = password.Any(char.IsDigit);
        return hasLetter && hasDigit;
    }

    private static bool IsValidEmail(string email) => !string.IsNullOrWhiteSpace(email) && MailAddress.TryCreate(email, out _);
}
