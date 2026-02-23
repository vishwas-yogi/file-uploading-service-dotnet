using System.Text.RegularExpressions;
using SecureLink.Core.Contracts;
using SecureLink.Infrastructure.Contracts;

namespace SecureLink.Infrastructure.Services;

public partial class AuthValidator(IRefreshTokensRepository refreshTokensRepository)
    : IAuthValidator
{
    private readonly IRefreshTokensRepository _refreshTokensRepository = refreshTokensRepository;

    [GeneratedRegex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).+$")]
    private static partial Regex StrongPasswordRegex();

    private static readonly int _minLength = 12;

    public ValidationResult<ErrorDetails> ValidatePassword(string password)
    {
        var isValid =
            !string.IsNullOrWhiteSpace(password)
            && password.Length >= _minLength
            && StrongPasswordRegex().IsMatch(password);

        if (!isValid)
        {
            return new ValidationResult<ErrorDetails>
            {
                IsValid = false,
                Error = new ErrorDetails { Message = "Weak Password" },
            };
        }

        return new ValidationResult<ErrorDetails> { IsValid = true };
    }

    public async Task<ValidationResult<RefreshTokenErrorDetails>> ValidateRefreshToken(
        string token,
        Guid userId
    )
    {
        var refreshToken = await _refreshTokensRepository.GetToken(token);

        if (refreshToken is null)
        {
            return new ValidationResult<RefreshTokenErrorDetails>
            {
                IsValid = false,
                Error = new RefreshTokenErrorDetails { Message = "Refresh token is required" },
            };
        }

        if (refreshToken.UserId != userId)
        {
            return new ValidationResult<RefreshTokenErrorDetails>
            {
                IsValid = false,
                Error = new RefreshTokenErrorDetails { IsUserMismatch = true },
            };
        }

        var isRevoked = refreshToken.RevokedAt is not null;
        var isExpired = refreshToken.ExpiresAt < DateTimeOffset.UtcNow;

        // TODO: maybe add a grace period for revokedAt and expiredAt. If inconsistencies are observed during concurrent requests
        if (isRevoked || isExpired)
        {
            return new ValidationResult<RefreshTokenErrorDetails>
            {
                IsValid = false,
                Error = new RefreshTokenErrorDetails
                {
                    IsRevoked = isRevoked,
                    IsExpired = isExpired,
                },
            };
        }

        return new ValidationResult<RefreshTokenErrorDetails> { IsValid = true };
    }
}
