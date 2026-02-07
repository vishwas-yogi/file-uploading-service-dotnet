using System;
using System.Data;
using Microsoft.Extensions.Options;
using Npgsql;

namespace SecureLink.Infrastructure.Contracts;

public class DapperContext : IDapperContext
{
    private readonly string _connectionString;

    public DapperContext(IOptions<DapperOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (string.IsNullOrWhiteSpace(options.Value.ConnectionString))
        {
            throw new ArgumentException(
                "ConnectionString must be provided in configuration",
                nameof(options)
            );
        }
        _connectionString = options.Value.ConnectionString;
    }

    public IDbConnection CreateConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }
}
