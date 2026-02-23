namespace SecureLink.Core.Contracts;

public record RefreshTokensApiRequest
{
    public required string RefreshToken { get; init; }
    public required Guid UserId { get; init; }
}
