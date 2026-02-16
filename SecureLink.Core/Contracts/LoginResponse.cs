namespace SecureLink.Core.Contracts;

public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    long ExpiresAt,
    Guid UserId,
    string Username
);
