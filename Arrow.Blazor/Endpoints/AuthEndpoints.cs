using Arrow.Blazor.Contracts.Auth;
using Arrow.Blazor.Filters;
using Arrow.Blazor.Models;
using Arrow.Blazor.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Arrow.Blazor.Endpoints;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth");

        group.MapPost("/register", RegisterAsync)
            .WithName("Register");

        group.MapPost("/login", LoginAsync)
            .WithName("Login");

        group.MapPost("/logout", LogoutAsync)
            .WithName("Logout")
            .RequireAuthorization();

        group.AddEndpointFilter<AntiforgeryEndpointFilter>();

        return group;
    }

    private static async Task<IResult> RegisterAsync(
        [FromBody] RegisterRequest request,
        IAuthService authService,
        CancellationToken cancellationToken)
    {
        var result = await authService.RegisterAsync(request.Email, request.Password, request.ConfirmPassword, cancellationToken);
        return ToResult(result);
    }

    private static async Task<IResult> LoginAsync(
        [FromBody] LoginRequest request,
        IAuthService authService,
        CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request.Email, request.Password, cancellationToken);
        return ToResult(result);
    }

    private static async Task<IResult> LogoutAsync(IAuthService authService)
    {
        await authService.LogoutAsync();
        return Results.Json(AuthApiResponse.FromResult(AuthResult.Success()));
    }

    private static IResult ToResult(AuthResult result)
    {
        var payload = AuthApiResponse.FromResult(result);
        return result.Succeeded
            ? Results.Json(payload)
            : Results.Json(payload, statusCode: StatusCodes.Status400BadRequest);
    }
}
