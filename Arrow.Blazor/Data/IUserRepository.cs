using Arrow.Blazor.Models;

namespace Arrow.Blazor.Data;

public interface IUserRepository
{
    Task<UserAccount?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<UserAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(UserAccount user, CancellationToken cancellationToken = default);
}
