using System.Data;

namespace SecureLink.Infrastructure.Contracts;

public class RepositoryBase
{
    protected IDapperContext DbContext { get; }

    protected RepositoryBase(IDapperContext dapperContext)
    {
        DbContext = dapperContext;
    }
}
