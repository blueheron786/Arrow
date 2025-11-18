using Arrow.Blazor.Models;

namespace Arrow.Blazor.Contracts.Auth;

public sealed class AuthApiResponse
{
    public bool Succeeded { get; set; }
    public List<string> Errors { get; set; } = new();

    public static AuthApiResponse FromResult(AuthResult result)
    {
        return new AuthApiResponse
        {
            Succeeded = result.Succeeded,
            Errors = result.Errors.ToList()
        };
    }
}
