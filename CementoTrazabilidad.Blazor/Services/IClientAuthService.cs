using CementoTrazabilidad.Shared.DTOs;

namespace CementoTrazabilidad.Blazor.Services;

public interface IClientAuthService  // ¡Nombre cambiado!
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task LogoutAsync();
    Task<bool> IsAuthenticatedAsync();
    Task<UsuarioInfo?> GetCurrentUserAsync();
    Task<string?> GetTokenAsync();
}
