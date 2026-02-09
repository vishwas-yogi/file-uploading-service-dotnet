using Dapper;
using Microsoft.Extensions.Logging;
using SecureLink.Infrastructure.Contracts;

namespace SecureLink.Infrastructure.Services;

public class MigrationRunner(
    IDapperContext dapperContext,
    ILogger<MigrationRunner> logger,
    string migrationsPath
)
{
    private readonly IDapperContext _dapperContext = dapperContext;
    private readonly ILogger<MigrationRunner> _logger = logger;
    private readonly string _migrationsPath = migrationsPath;

    public async Task RunMigrations()
    {
        try
        {
            // 1. Ensure Migration Tracking Table exists or create it
            await EnsureMigrationTableExists();

            // 2. Load all the migrations from the directory
            // 2.1. Create migration table object by extracting info name, version, script
            var migrationFiles = LoadAllMigrations();

            foreach (var file in migrationFiles)
            {
                // 3. Check if migration has already been applied
                if (await IsMigrationApplied(file.Version))
                {
                    _logger.LogInformation("Migration {Version} already applied", file.Version);
                    continue;
                }

                // 4. Apply the migration
                // 4.1 Update the migration table
                _logger.LogInformation(
                    "Applying migration {Version}: {Name}",
                    file.Version,
                    file.Name
                );
                await ApplyMigration(file);
                _logger.LogInformation(
                    "Migration {Version}_{Name}.sql applied",
                    file.Version,
                    file.Name
                );
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to apply migrations.");
            throw;
        }
    }

    private async Task EnsureMigrationTableExists()
    {
        var sql = """
            CREATE TABLE IF NOT EXISTS migrations_record (
                version INT PRIMARY KEY,
                name VARCHAR(500),
                applied_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
            );
            """;
        using var connection = _dapperContext.CreateConnection();
        var count = await connection.ExecuteAsync(sql);
    }

    private IEnumerable<Migration> LoadAllMigrations()
    {
        var migrations = new List<Migration>();
        var delimiters = new char[] { '_' };

        if (!Directory.Exists(_migrationsPath))
        {
            _logger.LogWarning("Migrations directory not found {Path}", _migrationsPath);
            return migrations;
        }

        var sqlFiles = Directory.GetFiles(_migrationsPath, "*.sql").OrderBy(f => f);

        // Parse the files.
        // Names should be Version_Name.sql. E.g. 001_CreateUsersTable.sql
        foreach (var file in sqlFiles)
        {
            var script = File.ReadAllText(file);
            var fullName = Path.GetFileNameWithoutExtension(file);

            var parts = fullName.Split(
                delimiters,
                2,
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
            );

            if (parts.Length < 2)
            {
                _logger.LogWarning("Invalid migration file name: {fullName}", fullName);
                continue;
            }

            bool success = int.TryParse(parts[0], out var version);
            if (!success)
            {
                _logger.LogWarning("Invalid migration file name: {fullName}", fullName);
                continue;
            }

            migrations.Add(
                new Migration
                {
                    Name = parts[1],
                    Version = version,
                    Script = script,
                }
            );
        }

        // Sorting because later versions of migrations might be dependent on earlier versions
        return migrations.OrderBy(m => m.Version);
    }

    private async Task<bool> IsMigrationApplied(int version)
    {
        var sql = "SELECT COUNT(*) FROM migrations_record WHERE version=@Version";
        using var connection = _dapperContext.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Version = version });
        return count > 0;
    }

    private async Task ApplyMigration(Migration migration)
    {
        using var connection = _dapperContext.CreateConnection();
        connection.Open();

        using var transaction = connection.BeginTransaction();

        try
        {
            await connection.ExecuteAsync(migration.Script, transaction: transaction);
            var sql = """
                INSERT INTO migrations_record (version, name)
                VALUES (@Version, @Name)
                """;

            await connection.ExecuteAsync(
                sql,
                new { migration.Version, migration.Name },
                transaction: transaction
            );

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private class Migration
    {
        public int Version { get; set; }
        public string Script { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
