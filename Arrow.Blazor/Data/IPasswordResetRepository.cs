using Arrow.Blazor.Models;

namespace Arrow.Blazor.Data;

public interface IPasswordResetRepository
{
    Task<Guid> CreateResetTokenAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<PasswordResetToken?> GetValidTokenAsync(Guid token, CancellationToken cancellationToken = default);
    Task MarkTokenAsUsedAsync(Guid token, CancellationToken cancellationToken = default);
    Task DeleteExpiredTokensAsync(CancellationToken cancellationToken = default);
}
