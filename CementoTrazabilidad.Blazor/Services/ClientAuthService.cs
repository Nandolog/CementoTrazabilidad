using Blazored.LocalStorage;
using CementoTrazabilidad.Shared.DTOs;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;

namespace CementoTrazabilidad.Blazor.Services;

public class ClientAuthService : IClientAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;
    private readonly AuthenticationStateProvider _authStateProvider;

    public ClientAuthService(
        HttpClient httpClient,
        ILocalStorageService localStorage,
        AuthenticationStateProvider authStateProvider)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
        _authStateProvider = authStateProvider;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

                if (result?.Success == true && !string.IsNullOrEmpty(result.Token))
                {
                    // Guardar token y usuario
                    await _localStorage.SetItemAsync("authToken", result.Token);
                    if (result.Usuario != null)
                    {
                        await _localStorage.SetItemAsync("userInfo", result.Usuario);
                    }

                    // Notificar cambio de estado - ¡CORREGIDO!
                    if (_authStateProvider is CustomAuthStateProvider customProvider)
                    {
                        await customProvider.NotifyUserAuthentication(result.Token, result.Usuario!);
                    }

                    return result;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en login: {ex.Message}");
        }

        return new LoginResponse { Success = false, Message = "Error en la autenticación" };
    }

    public async Task LogoutAsync()
    {
        await _localStorage.RemoveItemAsync("authToken");
        await _localStorage.RemoveItemAsync("userInfo");
        _httpClient.DefaultRequestHeaders.Authorization = null;

        // Notificar cambio de estado - ¡CORREGIDO!
        if (_authStateProvider is CustomAuthStateProvider customProvider)
        {
            await customProvider.NotifyUserLogout();
        }
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        return await _localStorage.ContainKeyAsync("authToken");
    }

    public async Task<UsuarioInfo?> GetCurrentUserAsync()
    {
        return await _localStorage.GetItemAsync<UsuarioInfo>("userInfo");
    }

    public async Task<string?> GetTokenAsync()
    {
        return await _localStorage.GetItemAsync<string>("authToken");
    }
    /// <summary>
    /// Permite al usuario autenticado cambiar su propia contraseña.
    /// </summary>
    public async Task<(bool Success, string Message)> CambiarPasswordAsync(CambiarPasswordDto dto)
    {
        try
        {
            // Configurar el token de autenticación
            var token = await _localStorage.GetItemAsync<string>("authToken");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await _httpClient.PostAsJsonAsync("api/auth/cambiar-password", dto);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<CambiarPasswordResponseDto>();
                    return (result?.Success ?? true, result?.Mensaje ?? "Contraseña cambiada exitosamente");
            }

            // Intentar leer el mensaje de error
            try
            {
                var errorResult = await response.Content.ReadFromJsonAsync<CambiarPasswordResponseDto>();
                if (errorResult != null && !string.IsNullOrEmpty(errorResult.Mensaje))
                {
                    return (false, errorResult.Mensaje);
                }
            }
            catch
            {
                // Si no se puede deserializar, intentar leer como string
                var errorText = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(errorText))
                {
                    return (false, errorText);
                }
            }

            return (false, "Error al cambiar la contraseña. Verifique los datos e intente nuevamente.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error en CambiarPasswordAsync: {ex.Message}");
            return (false, $"Error: {ex.Message}");
        }
    }
}