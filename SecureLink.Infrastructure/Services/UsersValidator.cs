using System.Net.Mail;
using SecureLink.Core.Contracts;
using SecureLink.Core.Entities;
using SecureLink.Infrastructure.Contracts;

namespace SecureLink.Infrastructure.Services;

public class UsersValidator(IUsersRepository usersRepository) : IUsersValidator
{
    private readonly IUsersRepository _usersRepo = usersRepository;
    private User? _existingUser = null;
    private UserErrorDetails errors = new();

    public async Task<ValidationResult<UserErrorDetails>> Validate(CreateUserRequest request)
    {
        return await Validate(new ValidationRequest(request.Username, request.Name, request.Email));
    }

    public async Task<ValidationResult<UserErrorDetails>> Validate(
        UpdateUserRequest request,
        User existingUser
    )
    {
        _existingUser = existingUser;
        return await Validate(
            new ValidationRequest(
                request.Username ?? existingUser.Username,
                request.Name ?? existingUser.Name,
                request.Email ?? existingUser.Email
            )
        );
    }

    private async Task ValidateUsername(string? username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            errors.Username = "Username can't be empty";
            return;
        }

        var user = await _usersRepo.GetByUsername(username);
        if (user is not null && (_existingUser is null || user.Id != _existingUser.Id))
        {
            errors.Username = $"Username {username} is not unique";
            return;
        }
    }

    private async Task ValidateName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            errors.Name = "Name can't be empty";
            return;
        }

        if (!name.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)))
        {
            errors.Name = "Invalid Name";
            return;
        }
    }

    private void ValidateEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return; // As email is an optional field

        try
        {
            var _ = new MailAddress(email);
        }
        catch
        {
            errors.Email = "Invalid email address";
            return;
        }
    }

    private async Task<ValidationResult<UserErrorDetails>> Validate(ValidationRequest request)
    {
        errors = new();
        await ValidateUsername(request.Username);
        await ValidateName(request.Name);
        ValidateEmail(request.Email);

        if (HasErrors(errors))
        {
            return new ValidationResult<UserErrorDetails> { IsValid = false, Error = errors };
        }

        return new ValidationResult<UserErrorDetails> { IsValid = true };
    }

    private static bool HasErrors(UserErrorDetails errors)
    {
        return !string.IsNullOrEmpty(errors.Username)
            || !string.IsNullOrEmpty(errors.Name)
            || !string.IsNullOrEmpty(errors.Email);
    }

    private record ValidationRequest(string? Username, string? Name, string? Email);
}
