namespace Arrow.Blazor.Models;

public sealed class AuthResult
{
    private AuthResult(bool succeeded, IReadOnlyList<string> errors)
    {
        Succeeded = succeeded;
        Errors = errors;
    }

    public bool Succeeded { get; }

    public IReadOnlyList<string> Errors { get; }

    public static AuthResult Success() => new(true, Array.Empty<string>());

    public static AuthResult Failure(params string[] errors) => new(false, errors.Length == 0 ? new[] { "Unknown error" } : errors);
}
