using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Arrow.Blazor.Services;

public sealed class AdminAccessService : IAdminAccessService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string? _adminEmail;

    public AdminAccessService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        var configuredEmail = configuration["AdminEmailAddress"];
        _adminEmail = string.IsNullOrWhiteSpace(configuredEmail) ? null : configuredEmail.Trim();
    }

    public bool IsAdmin()
    {
        var email = GetCurrentUserEmail();
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(_adminEmail))
        {
            return false;
        }

        return string.Equals(email, _adminEmail, StringComparison.OrdinalIgnoreCase);
    }

    public string? GetCurrentUserEmail()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        return httpContext?.User?.FindFirstValue(ClaimTypes.Email);
    }
}
