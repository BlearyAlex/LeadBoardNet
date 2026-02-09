using System.Security.Claims;
using LeadBoard.Shared.Entities;

namespace LeadBoardNet.API.Services;

public interface ITokenService
{
    (string Token, DateTime ExpiresAtUtc) CreateAccessToken(ApplicationUser user, IList<string> roles, IEnumerable<Claim>? extraClaims = null);

    // Genera refresh token "en claro" (para devolver al cliente) y su hash (para persistir)
    (string RefreshTokenPlain, string RefreshTokenHash, DateTime ExpiresAtUtc) CreateRefreshToken(DateTime utcNow, int refreshTokenDays);

    string HashToken(string tokenPlain);
}