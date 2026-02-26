using Dapper;
using Microsoft.Extensions.Logging;
using SecureLink.Core.Contracts;
using SecureLink.Infrastructure.Contracts;

namespace SecureLink.Infrastructure.Repositories;

public class FileRepository(ILogger<FileRepository> logger, IDapperContext dapperContext)
    : RepositoryBase(dapperContext)
{
    private ILogger<FileRepository> _logger = logger;

    public async Task<Guid> Persist(FilePersistRepoRequest request)
    {
        _logger.LogInformation("File upload initiating for: {filename}", request.Filename);

        var sql = """
                    insert into files
                    (
                        id,
                        filename,
                        user_filename,
                        content_type,
                        owner,
                        status,
                        created_at,
                        last_modified_at
                    )
                    values
                    (
                        @Id,
                        @Filename,
                        @UserFileName,
                        @ContentType,
                        @Owner,
                        @Status::file_status,
                        CURRENT_TIMESTAMP,
                        CURRENT_TIMESTAMP
                    )
                    returning id;
            """;

        var variables = new
        {
            Id = Guid.NewGuid(),
            request.Filename,
            request.UserFilename,
            request.ContentType,
            request.Owner,
            Status = FileStatus.Pending.ToString(),
        };

        using var connection = DbContext.CreateConnection();
        return await connection.QuerySingleAsync<Guid>(sql, variables);
    }

    public async Task<bool> MarkFileAvailable(Guid fileId, string fileLocation)
    {
        var sql = """
                update files
                set
                    location = @Location,
                    status = @Status::file_status,
                    last_modified_at = CURRENT_TIMESTAMP
                where id = @Id;
            """;

        var variables = new
        {
            Location = fileLocation,
            Id = fileId,
            Status = FileStatus.Available.ToString(),
        };

        using var connection = DbContext.CreateConnection();
        var affected = await connection.ExecuteAsync(sql, variables);
        return affected > 0;
    }
}
