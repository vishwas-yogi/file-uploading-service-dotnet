using SecureLink.Core.Contracts;
using SecureLink.Core.Entities;

namespace SecureLink.Infrastructure.Contracts;

public interface IUsersValidator
{
    Task<ValidationResult<UserErrorDetails>> Validate(CreateUserRequest request);
    Task<ValidationResult<UserErrorDetails>> Validate(UpdateUserRequest request, User existingUser);
}
