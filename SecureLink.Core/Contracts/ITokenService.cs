namespace SecureLink.Core.Contracts;

public interface ITokenService
{
    string GenerateAccessToken(Guid userId);
    string GenerateRefreshToken();
}
