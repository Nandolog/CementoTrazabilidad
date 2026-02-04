using System.Net.Http.Json;
using Blazored.LocalStorage;
using CementoTrazabilidad.Shared.DTOs;

namespace CementoTrazabilidad.Blazor.Services;

public class MaterialService : IMaterialService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;

    public MaterialService(HttpClient httpClient, ILocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
    }

    private async Task ConfigureHttpClient()
    {
        var token = await _localStorage.GetItemAsync<string>("authToken");
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
    }

    public async Task<List<MaterialDto>> GetMaterialesAsync(bool? soloActivos = true)
    {
        await ConfigureHttpClient();
        try
        {
            var url = "api/materiales";
            if (soloActivos.HasValue)
            {
                url += $"?activos={soloActivos.Value.ToString().ToLower()}";
            }
            
            return await _httpClient.GetFromJsonAsync<List<MaterialDto>>(url)
                   ?? new List<MaterialDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en GetMaterialesAsync: {ex.Message}");
            return new List<MaterialDto>();
        }
    }

    public async Task<MaterialDto?> GetMaterialAsync(int id)
    {
        await ConfigureHttpClient();
        try
        {
            return await _httpClient.GetFromJsonAsync<MaterialDto>($"api/materiales/{id}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en GetMaterialAsync: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> CreateMaterialAsync(CreateMaterialDto request)
    {
        await ConfigureHttpClient();
        try
        {
            Console.WriteLine($"📤 Enviando material: Codigo={request.Codigo}, Descripcion={request.Descripcion}, PesoPorBolsa={request.PesoPorBolsa}, DensidadKGm3={request.DensidadKGm3}");
            
            var response = await _httpClient.PostAsJsonAsync("api/materiales", request);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ Error {response.StatusCode}: {errorContent}");
            }
            
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Excepción en CreateMaterialAsync: {ex.Message}");
            Console.WriteLine($"Stack: {ex.StackTrace}");
            return false;
        }
    }

    public async Task<bool> UpdateMaterialAsync(int id, UpdateMaterialDto request)
    {
        await ConfigureHttpClient();
        try
        {
            Console.WriteLine($"📤 Actualizando material {id}: Descripcion={request.Descripcion}, PesoPorBolsa={request.PesoPorBolsa}, DensidadKGm3={request.DensidadKGm3}, Activo={request.Activo}");
            
            var response = await _httpClient.PutAsJsonAsync($"api/materiales/{id}", request);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ Error {response.StatusCode}: {errorContent}");
            }
            
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Excepción en UpdateMaterialAsync: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteMaterialAsync(int id)
    {
        await ConfigureHttpClient();
        try
        {
            var response = await _httpClient.DeleteAsync($"api/materiales/{id}");
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ Error al eliminar material {id}: {errorContent}");
            }
            
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Excepción en DeleteMaterialAsync: {ex.Message}");
            return false;
        }
    }
}