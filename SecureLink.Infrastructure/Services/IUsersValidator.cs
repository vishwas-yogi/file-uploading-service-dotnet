using SecureLink.Core.Contracts;

namespace SecureLink.Infrastructure.Services;

public interface IUsersValidator
{
    Task<ValidationResult<UserErrorDetails>> Validate(CreateUserRequest request);
    Task<ValidationResult<UserErrorDetails>> Validate(UpdateUserRequest request);
}
