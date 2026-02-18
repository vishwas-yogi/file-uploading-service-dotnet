using Microsoft.AspNetCore.Mvc;
using SecureLink.Core.Contracts;
using SecureLink.Infrastructure.Contracts;

namespace SecureLink.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(IUsersRepository usersRepository, ITokenService tokenService) : ControllerBase
{
    public readonly ITokenService _tokenService = tokenService;
    public readonly IUsersRepository _usersRepo = usersRepository;

    [HttpPost]
    [Route("login")]
    public async Task<ActionResult<string>> Login([FromBody] LoginApiRequest request)
    {
        var user = await _usersRepo.GetByUsername(request.Username);

        if (user is null)
        {
            return NotFound();
        }

        return _tokenService.GenerateAccessToken(user.Id);
    }
}
