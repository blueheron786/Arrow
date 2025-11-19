using System.Data;
using Arrow.Blazor.Models;
using Dapper;
using Npgsql;

namespace Arrow.Blazor.Data;

public sealed class UserRepository(IDbConnectionFactory connectionFactory, ILogger<UserRepository> logger) : IUserRepository
{
    private const string UserColumns = "id AS Id, email AS Email, password_hash AS PasswordHash, created_at_utc AS CreatedAtUtc";

    public async Task<UserAccount?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        const string query = $"SELECT {UserColumns} FROM users WHERE LOWER(email) = LOWER(@Email) LIMIT 1";
        return await QuerySingleAsync(query, new { Email = email }, cancellationToken);
    }

    public async Task<UserAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string query = $"SELECT {UserColumns} FROM users WHERE id = @Id LIMIT 1";
        return await QuerySingleAsync(query, new { Id = id }, cancellationToken);
    }

    public async Task<Guid> CreateAsync(UserAccount user, CancellationToken cancellationToken = default)
    {
        const string command = "INSERT INTO users (id, email, password_hash, created_at_utc) VALUES (@Id, @Email, @PasswordHash, @CreatedAtUtc)";

        using var connection = CreateConnection();
        var parameters = new
        {
            user.Id,
            user.Email,
            user.PasswordHash,
            user.CreatedAtUtc
        };

        var definition = new CommandDefinition(command, parameters, cancellationToken: cancellationToken);
        await connection.ExecuteAsync(definition);
        return user.Id;
    }

    public async Task UpdatePasswordAsync(Guid userId, string passwordHash, CancellationToken cancellationToken = default)
    {
        const string command = "UPDATE users SET password_hash = @PasswordHash WHERE id = @UserId";

        using var connection = CreateConnection();
        var definition = new CommandDefinition(command, new { UserId = userId, PasswordHash = passwordHash }, cancellationToken: cancellationToken);
        await connection.ExecuteAsync(definition);
    }

    private async Task<UserAccount?> QuerySingleAsync(string sql, object parameters, CancellationToken cancellationToken)
    {
        try
        {
            using var connection = CreateConnection();
            var definition = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);
            return await connection.QuerySingleOrDefaultAsync<UserAccount>(definition);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UndefinedTable)
        {
            logger.LogError(ex, "Users table is missing. Ensure the database schema is created.");
            throw;
        }
    }

    private IDbConnection CreateConnection() => connectionFactory.CreateConnection();
}
