using Dapper;
using SecureLink.Core.Contracts;
using SecureLink.Core.Entities;
using SecureLink.Infrastructure.Contracts;

namespace SecureLink.Infrastructure.Repositories;

public class UsersRepository(IDapperContext dapperContext)
    : RepositoryBase(dapperContext),
        IUsersRepository
{
    private const string _selectAllColumns = """
            users.id,
            users.name,
            users.username,
            users.email,
            users.password_hash,
            users.created_at,
            users.last_modified_at
        """;

    private const string _selectColumns = """
            users.id,
            users.name,
            users.username,
            users.email,
            users.created_at,
            users.last_modified_at
        """;

    public async Task<UserResponse> Create(CreateUserRepoRequest request)
    {
        var sql = $"""
                insert into users
                (
                    id,
                    name,
                    username,
                    email,
                    password_hash,
                    created_at,
                    last_modified_at
                )
                values
                (
                    @Id,
                    @Name,
                    @Username,
                    @Email,
                    @PasswordHash,
                    CURRENT_TIMESTAMP,
                    CURRENT_TIMESTAMP
                )
                returning {_selectColumns};
            """;

        var variables = new
        {
            request.Id,
            request.Name,
            request.Username,
            request.Email,
            request.PasswordHash,
        };

        using var connection = DbContext.CreateConnection();
        return await connection.QuerySingleAsync<UserResponse>(sql, variables);
    }

    public async Task<bool> Delete(DeleteUserRepoRequest request)
    {
        var sql = "DELETE FROM users where users.id = @Id";

        using var connection = DbContext.CreateConnection();
        var affected = await connection.ExecuteAsync(sql, new { request.Id });
        return affected > 0;
    }

    public async Task<User?> GetById(Guid id)
    {
        var sql = $"""
                SELECT {_selectAllColumns}
                FROM users
                WHERE users.id = @Id;
            """;

        using var connection = DbContext.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<User?>(sql, new { Id = id });
    }

    public async Task<User?> GetByUsername(string username)
    {
        var sql = $"""
                SELECT {_selectAllColumns}
                FROM users
                WHERE users.username = @Username;
            """;

        using var connection = DbContext.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<User?>(sql, new { Username = username });
    }

    public async Task<List<UserResponse>> List()
    {
        var sql = $"SELECT {_selectColumns} FROM users;";

        using var connection = DbContext.CreateConnection();
        var users = await connection.QueryAsync<UserResponse>(sql);
        return users.AsList();
    }

    public async Task<UserResponse> Update(UpdateUserRepoRequest request)
    {
        var sql = $"""
                update users
                set
                    name = @Name,
                    username = @Username,
                    email = @Email,
                    last_modified_at = CURRENT_TIMESTAMP
                where id = @Id
                returning {_selectColumns};
            """;

        var variables = new
        {
            request.UpdatedUser.Id,
            request.UpdatedUser.Name,
            request.UpdatedUser.Username,
            request.UpdatedUser.Email,
        };

        var connection = DbContext.CreateConnection();
        return await connection.QuerySingleAsync<UserResponse>(sql, variables);
    }
}
