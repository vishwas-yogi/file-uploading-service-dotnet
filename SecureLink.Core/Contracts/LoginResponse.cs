namespace SecureLink.Core.Contracts;

public record LoginResponse(
    string AcessToken,
    string RefreshToken,
    long ExpiresAt,
    Guid UserId,
    string Username
);
