using Dapper;
using SecureLink.Core.Entities;
using SecureLink.Infrastructure.Contracts;

namespace SecureLink.Infrastructure.Repositories;

public class RefreshTokensRepository(IDapperContext dapperContext)
    : RepositoryBase(dapperContext),
        IRefreshTokensRepository
{
    private readonly string _selectColumns =
        "id, user_id, value, expires_at, revoked_at, created_at";

    public async Task<bool> AddToken(RefreshToken token)
    {
        var sql = """
                insert into refresh_tokens
                (
                    id,
                    user_id,
                    value,
                    expires_at,
                    created_at
                )
                values
                (
                    @Id,
                    @UserId,
                    @Value,
                    @ExpiresAt,
                    CURRENT_TIMESTAMP
                );
            """;
        var variables = new
        {
            token.Id,
            token.UserId,
            token.Value,
            token.ExpiresAt,
        };

        using var connection = DbContext.CreateConnection();
        var affected = await connection.ExecuteAsync(sql, variables);
        return affected > 0;
    }

    public async Task<RefreshToken?> GetToken(string token)
    {
        var sql = $"""
                select {_selectColumns} from refresh_tokens
                where value = @Value; 
            """;

        using var connection = DbContext.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<RefreshToken>(sql, new { Value = token });
    }

    public async Task<bool> RevokeToken(string token)
    {
        var sql = """
                update refresh_tokens
                set
                    revoked_at = CURRENT_TIMESTAMP
                where value = @Value;
            """;

        using var connection = DbContext.CreateConnection();
        var affected = await connection.ExecuteAsync(sql, new { Value = token });
        return affected > 0;
    }

    public async Task<bool> RevokeAllTokens(Guid userId)
    {
        var sql = """
                update refresh_tokens
                set
                    revoked_at = CURRENT_TIMESTAMP
                where user_id = @UserId
            """;

        using var connection = DbContext.CreateConnection();
        var affected = await connection.ExecuteAsync(sql, new { UserId = userId });
        // TODO: add a GetActiveDevices method
        // TODO: add a check affected == numberOfLoggedInDevices
        return affected > 0;
    }
}
