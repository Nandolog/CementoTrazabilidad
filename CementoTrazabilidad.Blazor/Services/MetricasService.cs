using CementoTrazabilidad.Shared.DTOs;
using System.Net.Http.Json;

namespace CementoTrazabilidad.Blazor.Services;

public class MetricasService : IMetricasService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MetricasService> _logger;

    public MetricasService(HttpClient httpClient, ILogger<MetricasService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<MetricasTurnoDto?> GetMetricasTurnoAsync(int turnoId)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<MetricasTurnoDto>>(
                $"api/metricas/turno/{turnoId}");
            
            return response?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al obtener métricas del turno {turnoId}");
            return null;
        }
    }

    public async Task<List<ParadasDetalladasDto>> GetParadasDetalladasAsync(int turnoId)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<ParadasDetalladasDto>>>(
                $"api/metricas/paradas-detalladas/{turnoId}");
            
            return response?.Data ?? new List<ParadasDetalladasDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al obtener paradas detalladas del turno {turnoId}");
            return new List<ParadasDetalladasDto>();
        }
    }

    public async Task<List<DistribucionTiempoDto>> GetDistribucionTiempoAsync(int turnoId)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<DistribucionTiempoDto>>>(
                $"api/metricas/distribucion-tiempo/{turnoId}");
            
            return response?.Data ?? new List<DistribucionTiempoDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al obtener distribución de tiempo del turno {turnoId}");
            return new List<DistribucionTiempoDto>();
        }
    }

    private class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
    }
}