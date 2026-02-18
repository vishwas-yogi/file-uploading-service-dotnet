using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SecureLink.Core.Contracts;
using SecureLink.Infrastructure.Contracts;

namespace SecureLink.Infrastructure.Services;

public class TokenService(IOptions<JwtSettings> jwtSettings, ILogger<TokenService> logger)
    : ITokenService
{
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;
    private readonly ILogger<TokenService> _logger = logger;

    public string GenerateAccessToken(Guid userId)
    {
        var claims = new List<Claim>
        {
            new(
                Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Sub,
                userId.ToString()
            ),
            new(
                Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString()
            ),
            // new (ClaimTypes.Role, "user") // TODO: add roles to user and in the token
            // But first, I'll have to figure out what authorization pattern would best suit our needs
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationInMinutes),
            SigningCredentials = credentials,
        };

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(tokenDescriptor);
        return handler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        throw new NotImplementedException();
    }
}
