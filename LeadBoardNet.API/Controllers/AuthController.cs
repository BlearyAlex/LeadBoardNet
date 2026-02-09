using LeadBoard.Shared.Wrappers;
using LeadBoardNet.API.Dtos.Auth;
using LeadBoardNet.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeadBoardNet.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ApiControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }
    
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        await _authService.RegisterAsync(request, ct);
        return Ok(new { message = "Usuario registrado." });
    }
    
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var result = await _authService.LoginAsync(request, ip, ct);
        return Ok(result);
    }
    
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshRequest request, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var result = await _authService.RefreshAsync(request, ip, ct);
        return Ok(result);
    }
    
    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout([FromBody] RefreshRequest request, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        await _authService.LogoutAsync(request.RefreshToken, ip, ct);
        return Ok(new { message = "Sesión cerrada." });
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        // Ejemplo: devuelve info del usuario desde claims del JWT
        var userId = User.FindFirst("sub")?.Value;
        var email = User.FindFirst("email")?.Value;

        return Ok(new
        {
            userId,
            email,
            roles = User.Claims.Where(c => c.Type == System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToArray()
        });
    }

    [HttpGet("admin-area")]
    [Authorize(Policy = "RequireAdmin")]
    public IActionResult AdminArea()
    {
        return Ok(new { message = "Solo Admin puede ver esto." });
    }
}