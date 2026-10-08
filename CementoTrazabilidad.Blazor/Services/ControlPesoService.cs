using CementoTrazabilidad.Shared.DTOs;
using System.Net.Http.Json;

namespace CementoTrazabilidad.Blazor.Services
{
    public interface IControlPesoService
    {
        Task<ControlPesoDto?> RegistrarControlPeso(CrearControlPesoDto dto);
        Task<List<ControlPesoDto>> GetControlesPorTurno(int turnoId);
        Task<List<EstadisticaBoquillaDto>> GetEstadisticasBoquillas(int turnoId);
        Task<DashboardControlPesoDto?> GetDashboardControlPeso(int turnoId);
        Task<byte[]?> ExportarExcel(int turnoId);
    }

    public class ControlPesoService : IControlPesoService
    {
        private readonly HttpClient _http;
        private readonly ILogger<ControlPesoService> _logger;

        public ControlPesoService(HttpClient http, ILogger<ControlPesoService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<ControlPesoDto?> RegistrarControlPeso(CrearControlPesoDto dto)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/controlpeso", dto);

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<ControlPesoDto>>();
                    return apiResponse?.Data;
                }

                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Error al registrar control de peso: {error}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción al registrar control de peso");
                return null;
            }
        }

        public async Task<List<ControlPesoDto>> GetControlesPorTurno(int turnoId)
        {
            try
            {
                var response = await _http.GetFromJsonAsync<ApiResponse<List<ControlPesoDto>>>(
                    $"api/controlpeso/turno/{turnoId}");

                return response?.Data ?? new List<ControlPesoDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener controles del turno {turnoId}");
                return new List<ControlPesoDto>();
            }
        }

        public async Task<List<EstadisticaBoquillaDto>> GetEstadisticasBoquillas(int turnoId)
        {
            try
            {
                var response = await _http.GetFromJsonAsync<ApiResponse<List<EstadisticaBoquillaDto>>>(
                    $"api/controlpeso/estadisticas-boquillas/turno/{turnoId}");

                return response?.Data ?? new List<EstadisticaBoquillaDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener estadísticas de boquillas");
                return new List<EstadisticaBoquillaDto>();
            }
        }

        public async Task<DashboardControlPesoDto?> GetDashboardControlPeso(int turnoId)
        {
            try
            {
                var response = await _http.GetFromJsonAsync<ApiResponse<DashboardControlPesoDto>>(
                    $"api/controlpeso/dashboard/turno/{turnoId}");

                return response?.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener dashboard de control de peso");
                return null;
            }
        }

        public async Task<byte[]?> ExportarExcel(int turnoId)
        {
            try
            {
                var response = await _http.GetAsync($"api/controlpeso/exportar-excel/turno/{turnoId}");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsByteArrayAsync();
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al exportar Excel del turno {turnoId}");
                return null;
            }
        }
    }
}