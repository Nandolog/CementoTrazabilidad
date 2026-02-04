using Blazored.LocalStorage;
using CementoTrazabilidad.Shared.DTOs;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Text.Json;

namespace CementoTrazabilidad.Blazor.Services;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;

    public CustomAuthStateProvider(ILocalStorageService localStorage)  // ← SOLO localStorage
    {
        _localStorage = localStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");
            var userInfo = await _localStorage.GetItemAsync<UsuarioInfo>("userInfo");

            if (!string.IsNullOrEmpty(token) && userInfo != null)
            {
                // Crear claims basados en userInfo
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, userInfo.UsuarioID.ToString()),
                    new Claim(ClaimTypes.Name, userInfo.Nombre),
                    new Claim(ClaimTypes.Role, userInfo.Rol),
                    new Claim("Legajo", userInfo.Legajo)
                };

                var identity = new ClaimsIdentity(claims, "jwt");
                var user = new ClaimsPrincipal(identity);
                return new AuthenticationState(user);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en CustomAuthStateProvider: {ex.Message}");
        }

        // Usuario no autenticado
        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }

    public async Task NotifyUserAuthentication(string token, UsuarioInfo userInfo)
    {
        await _localStorage.SetItemAsync("authToken", token);
        await _localStorage.SetItemAsync("userInfo", userInfo);

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task NotifyUserLogout()
    {
        await _localStorage.RemoveItemAsync("authToken");
        await _localStorage.RemoveItemAsync("userInfo");

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}