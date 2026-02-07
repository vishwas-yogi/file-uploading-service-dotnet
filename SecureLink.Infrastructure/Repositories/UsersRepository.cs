using SecureLink.Infrastructure.Contracts;

namespace SecureLink.Infrastructure.Repositories;

public class UsersRepository(IDapperContext dapperContext)
    : RepositoryBase(dapperContext),
        IUsersRepository { }
