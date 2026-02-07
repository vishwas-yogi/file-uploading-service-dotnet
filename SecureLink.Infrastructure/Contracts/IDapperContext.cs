using System.Data;

namespace SecureLink.Infrastructure.Contracts;

public interface IDapperContext
{
    IDbConnection CreateConnection();
}
