using LeadBoard.Shared.Entities;
using LeadBoard.Shared.Wrappers;
using LeadBoardNet.API.Data;
using LeadBoardNet.API.Dtos.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LeadBoardNet.API.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly JwtOptions _jwt;

    public AuthService(ApplicationDbContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ITokenService tokenService, IOptions<JwtOptions> jwtOptions)
    {
        _context = context;
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _jwt = jwtOptions.Value;
    }

    public async Task RegisterAsync(RegisterRequest request, CancellationToken ct)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName
        };
        
        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            throw new InvalidOperationException("No se pudo crear el usuario: " + string.Join("; ", result.Errors.Select(e => e.Description)));
        
        // Opcional: asignar rol por defecto
        // await _users.AddToRoleAsync(user, "User");
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, string ipAddress, CancellationToken ct)
    {
        var user = await _userManager.Users.SingleOrDefaultAsync(u => u.Email == request.Email, ct);
        if (user is null)
            throw new UnauthorizedAccessException("Credenciales inválidas.");

        // Verifica password + aplica lockout según configuración
        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!signInResult.Succeeded)
            throw new UnauthorizedAccessException("Credenciales inválidas o usuario bloqueado.");

        var roles = await _userManager.GetRolesAsync(user);

        // Access token
        var (accessToken, accessExp) = _tokenService.CreateAccessToken(user, roles);

        // Refresh token (persistido)
        var now = DateTime.UtcNow;
        var (refreshPlain, refreshHash, refreshExp) = _tokenService.CreateRefreshToken(now, _jwt.RefreshTokenDays);

        var refreshEntity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refreshHash,
            CreatedAtUtc = now,
            ExpiresAtUtc = refreshExp,
            CreatedByIp = ipAddress
        };

        _context.RefreshTokens.Add(refreshEntity);
        await _context.SaveChangesAsync(ct);

        var authResponse = new AuthResponse
        {
            AccessToken = accessToken,
            AccessTokenExpiresAtUtc = accessExp,
            RefreshToken = refreshPlain,
            RefreshTokenExpiresAtUtc = refreshExp
        };

        return Result<AuthResponse>.Success(authResponse);
    }

    public async Task<Result<AuthResponse>> RefreshAsync(RefreshRequest request, string ipAddress, CancellationToken ct)
    {
        var refreshHash = _tokenService.HashToken(request.RefreshToken);

        var existing = await _context.RefreshTokens
            .Include(x => x.User)
            .SingleOrDefaultAsync(x => x.TokenHash == refreshHash, ct);

        if (existing is null)
            throw new UnauthorizedAccessException("Refresh token inválido.");

        if (!existing.IsActive)
            throw new UnauthorizedAccessException("Refresh token expirado o revocado.");

        var user = existing.User;

        // Rotación: revocar token actual y emitir uno nuevo
        var now = DateTime.UtcNow;

        existing.RevokedAtUtc = now;
        existing.RevokedByIp = ipAddress;

        var roles = await _userManager.GetRolesAsync(user);
        var (accessToken, accessExp) = _tokenService.CreateAccessToken(user, roles);

        var (newRefreshPlain, newRefreshHash, newRefreshExp) = _tokenService.CreateRefreshToken(now, _jwt.RefreshTokenDays);

        existing.ReplacedByTokenHash = newRefreshHash;

        _context.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = newRefreshHash,
            CreatedAtUtc = now,
            ExpiresAtUtc = newRefreshExp,
            CreatedByIp = ipAddress
        });

        await _context.SaveChangesAsync(ct);

        var authResponse = new AuthResponse
        {
            AccessToken = accessToken,
            AccessTokenExpiresAtUtc = accessExp,
            RefreshToken = newRefreshPlain,
            RefreshTokenExpiresAtUtc = newRefreshExp
        };

        return Result<AuthResponse>.Success(authResponse);
    }

    public async Task LogoutAsync(string refreshToken, string ipAddress, CancellationToken ct)
    {
        var refreshHash = _tokenService.HashToken(refreshToken);

        var existing = await _context.RefreshTokens.SingleOrDefaultAsync(x => x.TokenHash == refreshHash, ct);
        if (existing is null)
            return; // idempotente: no revelar existencia

        if (!existing.IsRevoked)
        {
            existing.RevokedAtUtc = DateTime.UtcNow;
            existing.RevokedByIp = ipAddress;
            await _context.SaveChangesAsync(ct);
        }
    }
}