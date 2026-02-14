using SecureLink.Core.Contracts;

namespace SecureLink.Infrastructure.Contracts;

public interface IUsersRepository
{
    Task<List<UserResponse>> List();
    Task<UserResponse?> Get(Guid id);
    Task<UserResponse> Create(CreateUserRepoRequest request);
    Task<UserResponse> Update(UpdateUserRepoRequest request);
    Task<bool> Delete(DeleteUserRepoRequest request);
}
