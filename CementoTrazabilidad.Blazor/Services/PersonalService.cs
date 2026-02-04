using System.Net.Http.Json;
using Blazored.LocalStorage;
using CementoTrazabilidad.Shared.DTOs;

namespace CementoTrazabilidad.Blazor.Services;

public class PersonalService : IPersonalService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;

    public PersonalService(HttpClient httpClient, ILocalStorageService localStorage)
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

    public async Task<List<PersonalDto>> GetPersonalAsync()
    {
        await ConfigureHttpClient();
        try
        {
            Console.WriteLine("🔄 PersonalService.GetPersonalAsync() - Llamando a api/personal");
            
            var response = await _httpClient.GetAsync("api/personal");
            
            Console.WriteLine($"📡 Status: {(int)response.StatusCode} {response.StatusCode}");
            
            response.EnsureSuccessStatusCode();

            // ✅ CORRECCIÓN: Deserializar como ApiResponse<List<PersonalDto>>
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<PersonalDto>>>();
            
            if (apiResponse?.Success == true && apiResponse.Data != null)
            {
                Console.WriteLine($"✅ Personal obtenido: {apiResponse.Data.Count} registros");
                return apiResponse.Data;
            }
            
            Console.WriteLine("⚠️ La respuesta del API no contiene datos");
            return new List<PersonalDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error al obtener personal: {ex.Message}");
            Console.WriteLine($"   StackTrace: {ex.StackTrace}");
            return new List<PersonalDto>();
        }
    }

    public async Task<List<PersonalDto>> GetPersonalActivoAsync()
    {
        await ConfigureHttpClient();
        try
        {
            Console.WriteLine("🔄 PersonalService.GetPersonalActivoAsync() - Llamando a api/personal/activos");
            
            var response = await _httpClient.GetAsync("api/personal/activos");
            
            Console.WriteLine($"📡 Status: {(int)response.StatusCode} {response.StatusCode}");
            
            if (response.IsSuccessStatusCode)
            {
                // ✅ CORRECCIÓN: Deserializar como ApiResponse
                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<PersonalDto>>>();
                
                if (apiResponse?.Success == true && apiResponse.Data != null)
                {
                    Console.WriteLine($"✅ Personal activo obtenido: {apiResponse.Data.Count} registros");
                    return apiResponse.Data;
                }
            }

            // Fallback: Si el endpoint específico falla, obtener todo y filtrar
            Console.WriteLine("⚠️ Usando fallback: obteniendo todo el personal y filtrando");
            var todoElPersonal = await GetPersonalAsync();
            return todoElPersonal.Where(p => p.Activo).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error al obtener personal activo: {ex.Message}");
            return new List<PersonalDto>();
        }
    }

    public async Task<PersonalDto?> GetPersonalByIdAsync(int id)
    {
        await ConfigureHttpClient();
        try
        {
            var response = await _httpClient.GetAsync($"api/personal/{id}");
            response.EnsureSuccessStatusCode();
            
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<PersonalDto>>();
            return apiResponse?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener personal por ID {id}: {ex.Message}");
            return null;
        }
    }

    public async Task<PersonalDto?> GetPersonalByLegajoAsync(string legajo)
    {
        await ConfigureHttpClient();
        try
        {
            var response = await _httpClient.GetAsync($"api/personal/legajo/{legajo}");
            response.EnsureSuccessStatusCode();
            
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<PersonalDto>>();
            return apiResponse?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener personal por legajo {legajo}: {ex.Message}");
            return null;
        }
    }

    public async Task<List<PersonalDto>> SearchPersonalAsync(string term)
    {
        await ConfigureHttpClient();
        try
        {
            var response = await _httpClient.GetAsync($"api/personal/search?term={Uri.EscapeDataString(term)}");
            response.EnsureSuccessStatusCode();
            
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<PersonalDto>>>();
            return apiResponse?.Data ?? new List<PersonalDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al buscar personal con término '{term}': {ex.Message}");
            return new List<PersonalDto>();
        }
    }

    public async Task<bool> CreatePersonalAsync(CreatePersonalDto request)
    {
        await ConfigureHttpClient();
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/personal", request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al crear personal: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> UpdatePersonalAsync(int id, UpdatePersonalDto request)
    {
        await ConfigureHttpClient();
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/personal/{id}", request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al actualizar personal {id}: {ex.Message}");
            return false;
        }
    }

    public async Task<(bool success, string message)> DeletePersonalAsync(int id)
    {
        await ConfigureHttpClient();
        try
        {
            Console.WriteLine($"🗑️ Intentando eliminar personal ID: {id}");
            var response = await _httpClient.DeleteAsync($"api/personal/{id}");

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"✅ Personal {id} eliminado exitosamente");
                return (true, "Personal eliminado exitosamente");
            }

            // Leer y parsear el mensaje de error del servidor
            try
            {
                var errorResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
                if (errorResponse != null && !string.IsNullOrEmpty(errorResponse.Message))
                {
                    Console.WriteLine($"❌ Error del servidor: {errorResponse.Message}");
                    return (false, errorResponse.Message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ No se pudo parsear respuesta de error: {ex.Message}");
            }

            return (false, $"Error al eliminar personal (Código: {(int)response.StatusCode})");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Excepción al eliminar personal {id}: {ex.Message}");
            return (false, $"Error: {ex.Message}");
        }
    }

    public async Task<(bool success, string message)> ToggleActivoAsync(int id)
    {
        await ConfigureHttpClient();
        try
        {
            Console.WriteLine($"🔄 Cambiando estado del personal ID: {id}");
            var response = await _httpClient.PatchAsync($"api/personal/{id}/toggle-activo", null);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
                Console.WriteLine($"✅ Estado cambiado exitosamente");
                return (true, result?.Message ?? "Estado actualizado exitosamente");
            }

            // Leer mensaje de error
            try
            {
                var errorResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
                if (errorResponse != null && !string.IsNullOrEmpty(errorResponse.Message))
                {
                    Console.WriteLine($"❌ Error del servidor: {errorResponse.Message}");
                    return (false, errorResponse.Message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ No se pudo parsear respuesta de error: {ex.Message}");
            }

            return (false, $"Error al cambiar estado (Código: {(int)response.StatusCode})");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Excepción al cambiar estado del personal {id}: {ex.Message}");
            return (false, $"Error: {ex.Message}");
        }
    }

    public async Task<bool> ActualizarEstadoPersonalAsync(int personalId, bool nuevoEstado)
    {
        await ConfigureHttpClient();
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/personal/{personalId}/estado", nuevoEstado);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al actualizar estado del personal {personalId}: {ex.Message}");
            return false;
        }
    }

    public async Task<(bool success, string message)> ResetPasswordAsync(int personalId, ResetPasswordDto request)
    {
        await ConfigureHttpClient();
        try
        {
            Console.WriteLine($"🔐 Restableciendo contraseña para PersonalID: {personalId}");
            var response = await _httpClient.PostAsJsonAsync($"api/personal/{personalId}/reset-password", request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
                Console.WriteLine($"✅ Contraseña restablecida exitosamente");
                return (true, result?.Message ?? "Contraseña restablecida exitosamente");
            }

            // Leer mensaje de error
            try
            {
                var errorResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
                if (errorResponse != null && !string.IsNullOrEmpty(errorResponse.Message))
                {
                    Console.WriteLine($"❌ Error del servidor: {errorResponse.Message}");
                    return (false, errorResponse.Message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ No se pudo parsear respuesta de error: {ex.Message}");
            }

            return (false, $"Error al restablecer contraseña (Código: {(int)response.StatusCode})");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Excepción al restablecer contraseña: {ex.Message}");
            return (false, $"Error: {ex.Message}");
        }
    }
}

// Agregar esta clase al final del archivo, fuera de la clase PersonalService
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
}

