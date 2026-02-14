namespace SecureLink.Core.Contracts;

public interface IAuthService
{
    Task<LoginResponse> Login(LoginRequest req);
    Task<bool> Logout(string refreshToken);
    Task<bool> Register(RegisterRequest req);
    Task<LoginResponse?> RefreshTokens(string refreshToken);
}
