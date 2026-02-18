namespace SecureLink.Infrastructure.Contracts;

public class JwtSettings
{
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public required int AccessTokenExpirationInMinutes { get; set; }
    public required int RefreshTokenExpirationInHours { get; set; }
    public required string SecretKey { get; set; }
}
