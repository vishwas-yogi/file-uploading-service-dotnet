namespace SecureLink.Core.Contracts;

public interface IAuthService
{
    Task<ServiceResult<LoginResponse, ErrorDetails>> Login(LoginRequest request);
    Task<ServiceResult<bool, RefreshTokenErrorDetails>> Logout(LogoutRequest request);
    Task<ServiceResult<bool, UserErrorDetails>> Register(RegisterRequest req);
    Task<ServiceResult<LoginResponse?, RefreshTokenErrorDetails>> RefreshTokens(
        RefreshTokensRequest request
    );
}
