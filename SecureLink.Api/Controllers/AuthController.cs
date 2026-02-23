using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureLink.Core.Contracts;

namespace SecureLink.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(IAuthService authService, ITokenService tokenService) : ControllerBase
{
    public readonly ITokenService _tokenService = tokenService;
    public readonly IAuthService _authService = authService;

    [HttpPost]
    [Route("login")]
    public async Task<ActionResult<string>> Login([FromBody] LoginApiRequest request)
    {
        var response = await _authService.Login(
            new LoginRequest { Username = request.Username, Password = request.Password }
        );

        if (!response.IsSuccess)
        {
            return Unauthorized(response.Error);
        }

        return Ok(response.Data);
    }

    [HttpPost]
    [Route("register")]
    public async Task<ActionResult<string>> Register([FromBody] RegisterApiRequest request)
    {
        var response = await _authService.Register(
            new RegisterRequest
            {
                Username = request.Username,
                Password = request.Password,
                Name = request.Name,
                Email = request.Email,
            }
        );

        if (!response.IsSuccess)
        {
            if (response.Status == ResponseStatus.BadRequest)
            {
                return BadRequest(response.Error);
            }

            return StatusCode(500, response.Error);
        }

        return Ok("User registered successfully");
    }

    [HttpPost]
    [Route("logout")]
    [Authorize]
    public async Task<ActionResult<string>> Logout([FromBody] LogoutApiRequest request)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized("Invalid user token");
        }

        var response = await _authService.Logout(
            new LogoutRequest { RefreshToken = request.RefreshToken, UserId = userId.Value }
        );

        if (!response.IsSuccess)
        {
            if (response.Status == ResponseStatus.Unauthorized)
            {
                return Unauthorized("Unauthorized User access");
            }

            return StatusCode(500, "Something went wrong");
        }

        return Ok("User logged out successfully");
    }

    [HttpPost]
    [Route("refresh")]
    public async Task<ActionResult<LoginResponse>> RefreshTokens(
        [FromBody] RefreshTokensApiRequest request
    )
    {
        var response = await _authService.RefreshTokens(
            new RefreshTokensRequest
            {
                RefreshToken = request.RefreshToken,
                UserId = request.UserId,
            }
        );

        if (!response.IsSuccess)
        {
            if (response.Status == ResponseStatus.Unauthorized)
            {
                return Unauthorized("Unauthorized user");
            }

            return StatusCode(500, "Something went wrong");
        }

        return Ok(response.Data);
    }
}
