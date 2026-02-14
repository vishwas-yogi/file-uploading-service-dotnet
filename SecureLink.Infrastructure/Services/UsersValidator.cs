using SecureLink.Core.Contracts;
using SecureLink.Infrastructure.Contracts;

namespace SecureLink.Infrastructure.Services;

public class UsersValidator(IUsersRepository usersRepository) : IUsersValidator
{
    private IUsersRepository _usersRepo = usersRepository;

    public Task<ValidationResult<UserErrorDetails>> Validate(CreateUserRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<ValidationResult<UserErrorDetails>> Validate(
        UpdateUserRequest request,
        UserResponse existingUser
    )
    {
        throw new NotImplementedException();
    }

    private async Task ValidateUsername(string username, UserErrorDetails errors)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            errors.Username = "Username can't be empty";
            return;
        }
    }

    private static bool HasErrors(UserErrorDetails errors)
    {
        return !string.IsNullOrEmpty(errors.Username)
            || !string.IsNullOrEmpty(errors.Name)
            || !string.IsNullOrEmpty(errors.Email);
    }
}
