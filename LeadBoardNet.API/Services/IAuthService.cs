using LeadBoard.Shared.Wrappers;
using LeadBoardNet.API.Dtos.Auth;

namespace LeadBoardNet.API.Services;

public interface IAuthService
{
    Task RegisterAsync(RegisterRequest request, CancellationToken ct);
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request, string ipAddress, CancellationToken ct);
    Task<Result<AuthResponse>> RefreshAsync(RefreshRequest request, string ipAddress, CancellationToken ct);
    Task LogoutAsync(string refreshToken, string ipAddress, CancellationToken ct);
}