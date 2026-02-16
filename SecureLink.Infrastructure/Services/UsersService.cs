using Microsoft.Extensions.Logging;
using SecureLink.Core.Contracts;
using SecureLink.Infrastructure.Contracts;
using SecureLink.Infrastructure.Helpers;

namespace SecureLink.Infrastructure.Services;

public class UsersService(
    IUsersRepository usersRepository,
    IUsersValidator validator,
    ILogger<UsersService> logger
) : IUsersService
{
    private readonly IUsersRepository _usersRepository = usersRepository;
    private readonly IUsersValidator _validator = validator;
    private readonly ILogger<UsersService> _logger = logger;

    public async Task<ServiceResult<UserResponse, UserErrorDetails>> Create(
        CreateUserRequest request
    )
    {
        _logger.LogInformation(
            "Create user request started with request for username: {username}",
            request.Username
        );

        var validationResult = await _validator.Validate(request);
        if (!validationResult.IsValid)
        {
            return ServiceResult<UserResponse, UserErrorDetails>.ValidationError(
                validationResult.Error!
            );
        }

        var createdUser = await _usersRepository.Create(
            new CreateUserRepoRequest
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Username = request.Username,
                Email = request.Email,
                PasswordHash = request.PasswordHash,
            }
        );

        return ServiceResult<UserResponse, UserErrorDetails>.Created(createdUser);
    }

    public async Task<ServiceResult<string, ErrorDetails>> Delete(DeleteUserRequest request)
    {
        _logger.LogInformation("Delete user request started for request: {request}", request);
        var isDeleted = await _usersRepository.Delete(new DeleteUserRepoRequest(request.Id));
        return isDeleted
            ? ServiceResult<string, ErrorDetails>.Deleted(request.Id.ToString(), null)
            : ServiceResult<string, ErrorDetails>.NotFound(
                new ErrorDetails { Message = "User not found" }
            );
    }

    public async Task<ServiceResult<UserResponse, ErrorDetails>> Get(GetUserRequest request)
    {
        _logger.LogInformation("Get user request started for request: {request}", request);
        var user = await _usersRepository.GetById(request.Id);

        if (user is null)
        {
            return ServiceResult<UserResponse, ErrorDetails>.NotFound(
                new ErrorDetails { Message = "User not found" }
            );
        }

        _logger.LogInformation(
            "Get user request completed with response: {response}",
            user.ToDto()
        );

        return ServiceResult<UserResponse, ErrorDetails>.Success(user.ToDto());
    }

    public async Task<ServiceResult<List<UserResponse>, ErrorDetails>> List(
        ListUsersRequest request
    )
    {
        _logger.LogInformation("List users request started");
        var users = await _usersRepository.List();
        _logger.LogInformation("List user request completed with response: {response}", users);
        return ServiceResult<List<UserResponse>, ErrorDetails>.Success(users);
    }

    public async Task<ServiceResult<UserResponse, UserErrorDetails>> Update(
        UpdateUserRequest request
    )
    {
        _logger.LogInformation("Update uesr request started for request: {request}", request);

        var existingUser = await _usersRepository.GetById(request.Id);
        if (existingUser is null)
        {
            return ServiceResult<UserResponse, UserErrorDetails>.NotFound(
                new UserErrorDetails { Message = "User not found" }
            );
        }

        var validationResult = await _validator.Validate(request, existingUser);
        if (!validationResult.IsValid)
        {
            return ServiceResult<UserResponse, UserErrorDetails>.ValidationError(
                validationResult.Error!
            );
        }

        existingUser!.Name = request.Name ?? existingUser.Name;
        existingUser.Username = request.Username ?? existingUser.Username;
        existingUser.Email = request.Email ?? existingUser.Email;

        var response = await _usersRepository.Update(
            new UpdateUserRepoRequest { UpdatedUser = existingUser }
        );

        return ServiceResult<UserResponse, UserErrorDetails>.Success(response);
    }
}
