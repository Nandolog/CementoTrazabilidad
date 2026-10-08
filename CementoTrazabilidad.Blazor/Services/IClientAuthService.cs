
using CementoTrazabilidad.Shared.DTOs;

namespace CementoTrazabilidad.Blazor.Services;

public interface IClientAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task LogoutAsync();
    Task<bool> IsAuthenticatedAsync();
    Task<UsuarioInfo?> GetCurrentUserAsync();
    Task<string?> GetTokenAsync();

    Task<(bool Success, string Message)> CambiarPasswordAsync(CambiarPasswordDto dto);
}