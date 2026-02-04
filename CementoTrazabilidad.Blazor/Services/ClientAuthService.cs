using CementoTrazabilidad.Shared.DTOs;
using System.Net.Http.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

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
}