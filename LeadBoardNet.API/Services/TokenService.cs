using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using LeadBoard.Shared.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace LeadBoardNet.API.Services;

public class TokenService : ITokenService
{
    private readonly JwtOptions _jwt;

    public TokenService(IOptions<JwtOptions> jwtOptions)
    {
        _jwt = jwtOptions.Value;
    }

    public (string Token, DateTime ExpiresAtUtc) CreateAccessToken(ApplicationUser user, IList<string> roles,
        IEnumerable<Claim>? extraClaims = null)
    {
        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(_jwt.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            // Subject principal
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new(JwtRegisteredClaimNames.Iat, EpochTime.GetIntDate(now).ToString(), ClaimValueTypes.Integer64),
        };

        // Roles como claims
        foreach (var role in roles)
            claims.Add(new(ClaimTypes.Role, role));

        if (extraClaims is not null)
            claims.AddRange(extraClaims);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: creds
        );
        
        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return (tokenString, expires);
    }

    public (string RefreshTokenPlain, string RefreshTokenHash, DateTime ExpiresAtUtc) CreateRefreshToken(
        DateTime utcNow,
        int refreshTokenDays)
    {
        // Refresh token: bytes aleatorios criptográficamente seguros.
        var bytes = RandomNumberGenerator.GetBytes(64);
        var plain = Base64UrlEncoder.Encode(bytes);

        var hash = HashToken(plain);
        var expires = utcNow.AddDays(refreshTokenDays);

        return (plain, hash, expires);
    }

    public string HashToken(string tokenPlain)
    {
        // SHA-256 sobre el token en claro (Base64Url). Persistimos el hash.
        // Alternativa aún más fuerte: HMAC con clave del servidor (no compartida).
        var bytes = Encoding.UTF8.GetBytes(tokenPlain);
        var hashBytes = SHA256.HashData(bytes);
        return Convert.ToHexString(hashBytes); // 64 hex chars
    }
}