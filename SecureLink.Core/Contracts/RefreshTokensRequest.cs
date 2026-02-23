namespace SecureLink.Core.Contracts;

public record RefreshTokensRequest
{
    public required Guid UserId { get; init; }
    public required string RefreshToken { get; init; }
}
