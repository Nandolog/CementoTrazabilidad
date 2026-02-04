using System.Net.Http.Json;
using CementoTrazabilidad.Shared.DTOs;

namespace CementoTrazabilidad.Blazor.Services
{
    public class DespachoService : IDespachoService
    {
        private readonly HttpClient _http;

        public DespachoService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<DespachoDTO>> GetDespachosAsync()
        {
            try
            {
                var result = await _http.GetFromJsonAsync<List<DespachoDTO>>("api/despachos");
                return result ?? new List<DespachoDTO>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener despachos: {ex.Message}");
                return new List<DespachoDTO>();
            }
        }

        public async Task<DespachoDTO?> GetDespachoByIdAsync(int id)
        {
            try
            {
                return await _http.GetFromJsonAsync<DespachoDTO>($"api/despachos/{id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener despacho {id}: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> CreateDespachoAsync(CreateDespachoDTO despacho)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/despachos", despacho);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al crear despacho: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateDespachoAsync(int id, UpdateDespachoDTO despacho)
        {
            try
            {
                var response = await _http.PutAsJsonAsync($"api/despachos/{id}", despacho);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar despacho {id}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteDespachoAsync(int id)
        {
            try
            {
                var response = await _http.DeleteAsync($"api/despachos/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar despacho {id}: {ex.Message}");
                return false;
            }
        }
    }
}