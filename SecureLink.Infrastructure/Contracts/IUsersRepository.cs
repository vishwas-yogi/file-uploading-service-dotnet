using SecureLink.Core.Contracts;
using SecureLink.Core.Entities;

namespace SecureLink.Infrastructure.Contracts;

public interface IUsersRepository
{
    Task<List<UserResponse>> List();
    Task<User?> GetById(Guid id);
    Task<User?> GetByUsername(string username);
    Task<UserResponse> Create(CreateUserRepoRequest request);
    Task<UserResponse> Update(UpdateUserRepoRequest request);
    Task<bool> Delete(DeleteUserRepoRequest request);
}
