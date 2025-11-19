using Arrow.Blazor.Models;
using Dapper;

namespace Arrow.Blazor.Data;

public class PasswordResetRepository : IPasswordResetRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public PasswordResetRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Guid> CreateResetTokenAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var token = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        
        const string sql = """
            INSERT INTO password_reset_tokens (token, user_id, created_at_utc, expires_at_utc, is_used)
            VALUES (@Token, @UserId, @CreatedAtUtc, @ExpiresAtUtc, @IsUsed)
            """;

        var parameters = new
        {
            Token = token,
            UserId = userId,
            CreatedAtUtc = now,
            ExpiresAtUtc = now.AddHours(24),
            IsUsed = false
        };

        var definition = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);
        await connection.ExecuteAsync(definition);
        
        return token;
    }

    public async Task<PasswordResetToken?> GetValidTokenAsync(Guid token, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = """
            SELECT token AS Token, user_id AS UserId, created_at_utc AS CreatedAtUtc, 
                   expires_at_utc AS ExpiresAtUtc, is_used AS IsUsed
            FROM password_reset_tokens
            WHERE token = @Token
                AND is_used = false
                AND expires_at_utc > @Now
            LIMIT 1
            """;

        var definition = new CommandDefinition(sql, new { Token = token, Now = DateTimeOffset.UtcNow }, cancellationToken: cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<PasswordResetToken>(definition);
    }

    public async Task MarkTokenAsUsedAsync(Guid token, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = """
            UPDATE password_reset_tokens
            SET is_used = true
            WHERE token = @Token
            """;

        var definition = new CommandDefinition(sql, new { Token = token }, cancellationToken: cancellationToken);
        await connection.ExecuteAsync(definition);
    }

    public async Task DeleteExpiredTokensAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = """
            DELETE FROM password_reset_tokens
            WHERE expires_at_utc < @Now
            """;

        var definition = new CommandDefinition(sql, new { Now = DateTimeOffset.UtcNow }, cancellationToken: cancellationToken);
        var deleted = await connection.ExecuteAsync(definition);
        
        if (deleted > 0)
        {
            Console.WriteLine($"Deleted {deleted} expired password reset tokens");
        }
    }
}
