namespace Arrow.Blazor.Services;

public interface IAdminAccessService
{
    bool IsAdmin();
    string? GetCurrentUserEmail();
}
