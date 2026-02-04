using CementoTrazabilidad.Shared.DTOs;
using System.Net.Http.Json;

namespace CementoTrazabilidad.Blazor.Services;

public class ParadaService : IParadaService
{
    private readonly HttpClient _http;
    private readonly ILogger<ParadaService> _logger;

    public ParadaService(HttpClient http, ILogger<ParadaService> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<List<ParadaDto>> GetParadasTurnoAsync(int turnoId)
    {
        try
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<List<ParadaDto>>>($"api/parada/turno/{turnoId}");
            return response?.Success == true ? response.Data ?? new() : new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener paradas del turno {TurnoId}", turnoId);
            return new();
        }
    }

    public async Task<bool> CrearParadaAsync(ParadaDto parada)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/parada", parada);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear parada");
            return false;
        }
    }

    public async Task<bool> ActualizarParadaAsync(int id, ParadaDto parada)
    {
        try
        {
            var response = await _http.PutAsJsonAsync($"api/parada/{id}", parada);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar parada {ParadaId}", id);
            return false;
        }
    }

    public async Task<bool> EliminarParadaAsync(int id)
    {
        try
        {
            var response = await _http.DeleteAsync($"api/parada/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar parada {ParadaId}", id);
            return false;
        }
    }

    public async Task<(bool HayCargasActivas, List<string> ZonasActivas)> GetCargasActivasAsync(int turnoId)
    {
        try
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<dynamic>>($"api/parada/turno/{turnoId}/activas");
            if (response?.Success == true && response.Data != null)
            {
                bool hayCargasActivas = response.Data.HayCargasActivas;
                List<string> zonasActivas = response.Data.ZonasActivas != null
                    ? System.Text.Json.JsonSerializer.Deserialize<List<string>>(response.Data.ZonasActivas.ToString()) ?? new List<string>()
                    : new List<string>();
                return (hayCargasActivas, zonasActivas);
            }
            return (false, new List<string>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cargas activas del turno {TurnoId}", turnoId);
            return (false, new List<string>());
        }
    }

    public async Task<bool> FinalizarParadaAsync(int id, DateTime? fechaHoraFin = null)
    {
        try
        {
            var url = fechaHoraFin.HasValue
                ? $"api/paradas/{id}/finalizar?fechaHoraFin={fechaHoraFin.Value.ToString("o")}"
                : $"api/paradas/{id}/finalizar";
            var response = await _http.PutAsync(url, null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al finalizar parada {ParadaId}", id);
            return false;
        }
    }
}