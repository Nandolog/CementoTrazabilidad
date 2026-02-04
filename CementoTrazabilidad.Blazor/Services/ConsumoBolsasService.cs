using CementoTrazabilidad.Shared.DTOs;
using System.Net.Http.Json;

namespace CementoTrazabilidad.Blazor.Services
{
    public interface IConsumoBolsasService
    {
        Task<List<ConsumoBolsasDTO>> GetConsumosAsync();
        Task<List<ConsumoBolsasDTO>> GetConsumosByTurnoAsync(int turnoId);
        Task<ConsumoBolsasDTO?> CreateConsumoAsync(ConsumoBolsasCreateDTO consumo);
        Task<bool> DeleteConsumoAsync(int id);
        Task<List<ProveedorBolsaDTO>> GetProveedoresAsync();
    }

    public class ConsumoBolsasService : IConsumoBolsasService
    {
        private readonly HttpClient _httpClient;

        public ConsumoBolsasService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<ConsumoBolsasDTO>> GetConsumosAsync()
        {
            try
            {
                var result = await _httpClient.GetFromJsonAsync<List<ConsumoBolsasDTO>>("api/ConsumoBolsas");
                return result ?? new List<ConsumoBolsasDTO>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al obtener consumos: {ex.Message}");
                return new List<ConsumoBolsasDTO>();
            }
        }

        public async Task<List<ConsumoBolsasDTO>> GetConsumosByTurnoAsync(int turnoId)
        {
            try
            {
                var result = await _httpClient.GetFromJsonAsync<List<ConsumoBolsasDTO>>($"api/ConsumoBolsas/turno/{turnoId}");
                return result ?? new List<ConsumoBolsasDTO>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al obtener consumos del turno: {ex.Message}");
                return new List<ConsumoBolsasDTO>();
            }
        }

        public async Task<ConsumoBolsasDTO?> CreateConsumoAsync(ConsumoBolsasCreateDTO consumo)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/ConsumoBolsas", consumo);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<ConsumoBolsasDTO>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al crear consumo: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> DeleteConsumoAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/ConsumoBolsas/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al eliminar consumo: {ex.Message}");
                return false;
            }
        }

        public async Task<List<ProveedorBolsaDTO>> GetProveedoresAsync()
        {
            try
            {
                var result = await _httpClient.GetFromJsonAsync<List<ProveedorBolsaDTO>>("api/ProveedoresBolsa");
                return result ?? new List<ProveedorBolsaDTO>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al obtener proveedores: {ex.Message}");
                return new List<ProveedorBolsaDTO>();
            }
        }
    }
}