using Microsoft.Extensions.Logging;
using SecureLink.Core.Contracts;
using SecureLink.Infrastructure.Contracts;

namespace SecureLink.Infrastructure.Services;

public class UsersService(IUsersRepository usersRepository, ILogger<UsersService> logger)
    : IUsersService
{
    private IUsersRepository _usersRepository = usersRepository;
    private ILogger<UsersService> _logger = logger;

    public Task<ServiceResult<UserResponse, UserErrorDetails>> Create(CreateUserRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<ErrorDetails>> Delete(DeleteUserRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<UserResponse, ErrorDetails>> Get(GetUserRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<List<UserResponse>, ErrorDetails>> List(ListUsersRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<UserResponse, UserErrorDetails>> Update(UpdateUserRequest request)
    {
        throw new NotImplementedException();
    }
}
