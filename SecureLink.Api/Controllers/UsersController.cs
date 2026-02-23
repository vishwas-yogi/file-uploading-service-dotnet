using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureLink.Core.Contracts;

namespace SecureLink.Api.Controllers;

[Authorize]
[ApiController]
[Route("users")]
public class UsersController(IUsersService usersService, ILogger<UsersController> logger)
    : ControllerBase
{
    private readonly IUsersService _usersService = usersService;
    private readonly ILogger<UsersController> _logger = logger;

    [HttpGet]
    [Route("")]
    public async Task<ActionResult<List<UserResponse>>> ListUsers()
    {
        _logger.LogInformation("List users request initiated");

        var response = await _usersService.List(new ListUsersRequest());
        _logger.LogInformation("List users request completed: {response}", response);

        return Result(response);
    }

    [HttpGet]
    [Route("{id}")]
    public async Task<ActionResult<UserResponse>> GetUser([FromRoute] Guid id)
    {
        _logger.LogInformation("Get user request initiated for id: {id}", id);

        var response = await _usersService.Get(new GetUserRequest(id));
        _logger.LogInformation("Get user request completed for id: {id}", id);

        return Result(response);
    }

    [HttpPost]
    [Route("")]
    public async Task<ActionResult<UserResponse>> CreateUser(CreateUserApiRequest request)
    {
        _logger.LogInformation(
            "Create user request initiated for username : {username}",
            request.Username
        );

        var response = await _usersService.Create(
            new CreateUserRequest
            {
                Name = request.Name,
                Username = request.Username,
                Email = request.Email,
                // Password hash is just encoding for now
                // TODO: change this to use PasswordHasher service, once the service is ready
                PasswordHash = Encoding.UTF8.GetBytes(request.Password).ToString()!,
            }
        );
        _logger.LogInformation("Create user request completed");

        return Result(response);
    }

    [HttpPut]
    [Route("{id}")]
    public async Task<ActionResult<UserResponse>> UpdateUser(
        [FromRoute] Guid id,
        [FromBody] UpdateUserApiRequest request
    )
    {
        _logger.LogInformation(
            "Update user request initiated for {id} with payload: {request}",
            id,
            request
        );

        var response = await _usersService.Update(
            new UpdateUserRequest
            {
                Id = id,
                Name = request.Name,
                Username = request.Username,
                Email = request.Email,
            }
        );
        _logger.LogInformation("Update request completed for user {id}", id);

        return Result(response);
    }

    [HttpDelete]
    [Route("{id}")]
    public async Task<ActionResult> DeleteUser([FromRoute] Guid id)
    {
        _logger.LogInformation("Delete user request initiated for user id: {id}", id);

        var response = await _usersService.Delete(new DeleteUserRequest(id));
        _logger.LogInformation("Delete user completed for user id: {id}", id);

        return Result(response);
    }

    private ObjectResult Result<TData, TError>(ServiceResult<TData, TError> serviceResult)
    {
        if (!serviceResult.IsSuccess)
        {
            return serviceResult.Status switch
            {
                ResponseStatus.ValidationError => StatusCode(400, serviceResult.Error),
                ResponseStatus.NotFound => NotFound(serviceResult.Error),
                _ => StatusCode(500, "An unexpected error occurred"),
            };
        }

        return serviceResult.Status switch
        {
            ResponseStatus.Created => CreatedAtAction(
                nameof(GetUser),
                new { id = (serviceResult.Data as UserResponse)!.Id },
                serviceResult.Data
            ),
            ResponseStatus.Deleted => StatusCode(204, null),
            _ => Ok(serviceResult.Data),
        };
    }
}
