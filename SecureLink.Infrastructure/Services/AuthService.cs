using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SecureLink.Core.Contracts;
using SecureLink.Core.Entities;
using SecureLink.Infrastructure.Contracts;

namespace SecureLink.Infrastructure.Services;

public class AuthService(
    ITokenService tokenService,
    IUsersService usersService,
    IPasswordHasher passwordHasher,
    IAuthValidator authValidator,
    IOptions<JwtSettings> jwtSettings,
    ILogger<AuthService> logger
) : IAuthService
{
    private readonly ITokenService _tokenService = tokenService;
    private readonly IUsersService _usersService = usersService;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;
    private readonly IAuthValidator _authValidator = authValidator;
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;
    private readonly ILogger<AuthService> _logger = logger;
    private readonly ServiceResult<LoginResponse, ErrorDetails> _loginError = ServiceResult<
        LoginResponse,
        ErrorDetails
    >.BadRequest(new ErrorDetails { Message = "Invalid username or password. Please try again." });

    public async Task<ServiceResult<LoginResponse, ErrorDetails>> Login(LoginRequest request)
    {
        var userResponse = await _usersService.Get(new GetUserByUsernameReq(request.Username));
        if (!userResponse.IsSuccess)
            return _loginError;

        var user = userResponse.Data!;
        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            return _loginError;

        var accessToken = _tokenService.GenerateAccessToken(user.Id);
        var refreshToken = await _tokenService.GenerateRefreshToken(user.Id);

        return ServiceResult<LoginResponse, ErrorDetails>.Success(
            new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                UserId = user.Id,
                Username = user.Username,
                ExpiresAt = _jwtSettings.AccessTokenExpirationInMinutes * 60, // Time in seconds
            }
        );
    }

    public async Task<ServiceResult<bool, RefreshTokenErrorDetails>> Logout(LogoutRequest request)
    {
        // 1. Refresh token is expired: Revoke refresh token => logout
        // 2. Refresh token is revoked: Security breach => looutFromAllDevices
        // 3. Refresh token is user mismatched: Potential theft => logout
        // 4. Refresh token is valid => logout

        var ValidationResult = await _authValidator.ValidateRefreshToken(
            request.RefreshToken,
            request.UserId
        );

        if (!ValidationResult.IsValid)
        {
            var error = ValidationResult.Error!;
            if (error.IsRevoked)
            {
                var errorMsg =
                    $"Might be a security breach. Revoked refresh token used for user: {request.UserId}";
                error.Message = errorMsg;
                _logger.LogCritical("{message}", errorMsg);

                await _tokenService.RevokeAllTokens(request.UserId);
                return ServiceResult<bool, RefreshTokenErrorDetails>.Unauthorized(error);
            }

            if (error.IsUserMismatch)
            {
                var errorMsg =
                    $"Might be a security breach. User: {request.UserId} used a refresh token of different user";
                error.Message = errorMsg;
                _logger.LogCritical("{message}", errorMsg);

                // Revoke this token so can't be used.
                await _tokenService.RevokeToken(request.RefreshToken);
                return ServiceResult<bool, RefreshTokenErrorDetails>.Unauthorized(error);
            }

            return ServiceResult<bool, RefreshTokenErrorDetails>.Unauthorized(
                new RefreshTokenErrorDetails { Message = "Invalid refresh token" }
            );
        }

        // In all other cases, the current token can be revoked
        return await _tokenService.RevokeToken(request.RefreshToken)
            ? ServiceResult<bool, RefreshTokenErrorDetails>.Success(true)
            : ServiceResult<bool, RefreshTokenErrorDetails>.UnexpectedError(
                new RefreshTokenErrorDetails { Message = "Something went wrong" }
            );
    }

    public async Task<ServiceResult<LoginResponse?, RefreshTokenErrorDetails>> RefreshTokens(
        RefreshTokensRequest request
    )
    {
        // 1. Refresh token is revoked: Security breach => looutFromAllDevices. Return error.
        // 2. Refresh token is user mismatched: Potential theft => logout. Return error.
        // Although in this case, as this endpoint doesn't need authorization so we rely on the request's payload for userId
        // 3. Refresh token is expired or valid: Revoke current refresh token => Generate new pair of tokens

        var ValidationResult = await _authValidator.ValidateRefreshToken(
            request.RefreshToken,
            request.UserId
        );

        if (!ValidationResult.IsValid)
        {
            var error = ValidationResult.Error!;
            string errorMsg;
            if (error.IsRevoked)
            {
                errorMsg =
                    $"Might be a security breach. Revoked refresh token used for user: {request.UserId}";
                error.Message = errorMsg;
                _logger.LogCritical("{message}", errorMsg);

                await _tokenService.RevokeAllTokens(request.UserId);
                return ServiceResult<LoginResponse?, RefreshTokenErrorDetails>.Unauthorized(error);
            }

            if (error.IsUserMismatch)
            {
                errorMsg =
                    $"Might be a security breach. User: {request.UserId} used a refresh token of different user";
                error.Message = errorMsg;
                _logger.LogCritical("{message}", errorMsg);

                // Revoke this token so it can't be used.
                await _tokenService.RevokeToken(request.RefreshToken);
                return ServiceResult<LoginResponse?, RefreshTokenErrorDetails>.Unauthorized(error);
            }

            if (error.IsExpired)
            {
                errorMsg = "Refresh token is expired. Please login again";
                error.Message = errorMsg;
                _logger.LogError("{message}", errorMsg);
                await _tokenService.RevokeToken(request.RefreshToken);
                return ServiceResult<LoginResponse?, RefreshTokenErrorDetails>.Unauthorized(error);
            }

            return ServiceResult<LoginResponse?, RefreshTokenErrorDetails>.Unauthorized(
                new RefreshTokenErrorDetails { Message = "Invalid refresh token" }
            );
        }

        var userResponse = await _usersService.Get(new GetUserRequest(request.UserId));
        if (!userResponse.IsSuccess)
            return ServiceResult<LoginResponse?, RefreshTokenErrorDetails>.Unauthorized(
                new RefreshTokenErrorDetails { Message = "Invalid user" }
            );

        await _tokenService.RevokeToken(request.RefreshToken);
        var newAccessToken = _tokenService.GenerateAccessToken(request.UserId);
        var newRefreshToken = await _tokenService.GenerateRefreshToken(request.UserId);

        return ServiceResult<LoginResponse?, RefreshTokenErrorDetails>.Success(
            new LoginResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                UserId = request.UserId,
                Username = userResponse.Data!.Username,
                ExpiresAt = _jwtSettings.AccessTokenExpirationInMinutes * 60, // Time in seconds
            }
        );
    }

    public async Task<ServiceResult<bool, UserErrorDetails>> Register(RegisterRequest request)
    {
        var passwordValidation = _authValidator.ValidatePassword(request.Password);

        if (!passwordValidation.IsValid)
        {
            return ServiceResult<bool, UserErrorDetails>.BadRequest(
                new UserErrorDetails { PasswordError = passwordValidation.Error!.Message }
            );
        }

        var response = await _usersService.Create(
            new CreateUserRequest
            {
                Username = request.Username,
                Name = request.Name,
                Email = request.Email,
                PasswordHash = _passwordHasher.HashPassword(request.Password),
            }
        );

        if (!response.IsSuccess)
        {
            return ServiceResult<bool, UserErrorDetails>.UnexpectedError(response.Error!);
        }

        return ServiceResult<bool, UserErrorDetails>.Success(true);
    }
}
