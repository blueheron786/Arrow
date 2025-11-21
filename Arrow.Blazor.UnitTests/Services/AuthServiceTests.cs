using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Arrow.Blazor.Data;
using Arrow.Blazor.Models;
using Arrow.Blazor.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace Arrow.Blazor.UnitTests.Services;

public class AuthServiceTests
{
    [Test]
    public async Task RegisterAsync_ReturnsFailure_WhenPasswordsDoNotMatch()
    {
        var service = CreateAuthService(out _);

        var result = await service.RegisterAsync("user@example.com", "Password123", "Different123");

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Errors, Does.Contain("Passwords do not match."));
    }

    [Test]
    public async Task RegisterAsync_ReturnsFailure_WhenEmailIsInvalid()
    {
        var service = CreateAuthService(out _);

        var result = await service.RegisterAsync("not-an-email", "Password123", "Password123");

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Errors, Does.Contain("Please provide a valid email address."));
    }

    [Test]
    public async Task LoginAsync_InvalidPassword_DoesNotSignIn()
    {
        var repository = Substitute.For<IUserRepository>();
        repository
            .GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new UserAccount { Email = "user@example.com", PasswordHash = "hashed" });

        var hasher = Substitute.For<IPasswordHasher>();
        hasher.Verify("Password123", "hashed").Returns(false);

        var service = CreateAuthService(out var authService, repository, hasher);

        var result = await service.LoginAsync("user@example.com", "Password123");

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.Errors, Does.Contain("Invalid credentials."));
        Assert.That(authService.Principal, Is.Null);
    }

    [Test]
    public async Task LoginAsync_ValidCredentials_SignsIn()
    {
        var user = new UserAccount { Email = "user@example.com", PasswordHash = "hashed" };
        var repository = Substitute.For<IUserRepository>();
        repository
            .GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(user);

        var hasher = Substitute.For<IPasswordHasher>();
        hasher.Verify("Password123", "hashed").Returns(true);

        var service = CreateAuthService(out var authService, repository, hasher);

        var result = await service.LoginAsync("user@example.com", "Password123");

        Assert.That(result.Succeeded, Is.True);
        Assert.That(authService.Principal?.Identity?.IsAuthenticated, Is.True);
        Assert.That(authService.Scheme, Is.EqualTo(CookieAuthenticationDefaults.AuthenticationScheme));
    }

    [Test]
    public async Task RegisterAsync_ValidInput_CreatesUserAndSignsIn()
    {
        var repository = Substitute.For<IUserRepository>();
        repository
            .GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<UserAccount?>(null));

        var createdId = Guid.NewGuid();
        repository
            .CreateAsync(Arg.Any<UserAccount>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(createdId));

        var hasher = Substitute.For<IPasswordHasher>();
        hasher.Hash("Password123").Returns("hashed-value");

        var service = CreateAuthService(out var authService, repository, hasher);

        var result = await service.RegisterAsync(" user@example.com ", "Password123", "Password123");

        Assert.That(result.Succeeded, Is.True);
        await repository.Received(1).CreateAsync(Arg.Is<UserAccount>(u => u.Email == "user@example.com" && u.PasswordHash == "hashed-value"));
        Assert.That(authService.Principal, Is.Not.Null);
        Assert.That(authService.Scheme, Is.EqualTo(CookieAuthenticationDefaults.AuthenticationScheme));
    }

    private static AuthService CreateAuthService(
        out TestAuthenticationService authenticationService,
        IUserRepository? userRepository = null,
        IPasswordHasher? passwordHasher = null,
        ILogger<AuthService>? logger = null)
    {
        authenticationService = new TestAuthenticationService();
        var contextAccessor = CreateHttpContextAccessor(authenticationService);

        return new AuthService(
            userRepository ?? Substitute.For<IUserRepository>(),
            passwordHasher ?? Substitute.For<IPasswordHasher>(),
            contextAccessor,
            logger ?? Substitute.For<ILogger<AuthService>>());
    }

    private static IHttpContextAccessor CreateHttpContextAccessor(TestAuthenticationService authenticationService)
    {
        var services = new ServiceCollection()
            .AddSingleton<IAuthenticationService>(authenticationService)
            .BuildServiceProvider();

        var context = new DefaultHttpContext { RequestServices = services };
        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns(context);
        return accessor;
    }

    private sealed class TestAuthenticationService : IAuthenticationService
    {
        public string? Scheme { get; private set; }
        public ClaimsPrincipal? Principal { get; private set; }
        public AuthenticationProperties? Properties { get; private set; }

        public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string? scheme)
            => Task.FromResult(AuthenticateResult.NoResult());

        public Task ChallengeAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
            => Task.CompletedTask;

        public Task ForbidAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
            => Task.CompletedTask;

        public Task SignInAsync(HttpContext context, string? scheme, ClaimsPrincipal principal, AuthenticationProperties? properties)
        {
            Scheme = scheme;
            Principal = principal;
            Properties = properties;
            return Task.CompletedTask;
        }

        public Task SignOutAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
            => Task.CompletedTask;
    }
}