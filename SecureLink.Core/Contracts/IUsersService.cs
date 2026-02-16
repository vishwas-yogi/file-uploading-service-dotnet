namespace SecureLink.Core.Contracts;

public interface IUsersService
{
    Task<ServiceResult<List<UserResponse>, ErrorDetails>> List(ListUsersRequest request);
    Task<ServiceResult<UserResponse, ErrorDetails>> Get(GetUserRequest request);
    Task<ServiceResult<UserResponse, UserErrorDetails>> Create(CreateUserRequest request);
    Task<ServiceResult<UserResponse, UserErrorDetails>> Update(UpdateUserRequest request);
    Task<ServiceResult<string, ErrorDetails>> Delete(DeleteUserRequest request);
}
