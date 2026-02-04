using CementoTrazabilidad.Shared.DTOs;
using System.Net.Http.Json;
using System.Text.Json;

namespace CementoTrazabilidad.Blazor.Services;

public class LoteService : ILoteService
{
    private readonly HttpClient _http;

    public LoteService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<LoteProduccionDto>> GetLotesPorTurnoAsync(int turnoId)
    {
        try
        {
            Console.WriteLine($"🔄 Frontend - Obteniendo lotes del turno {turnoId}");
            Console.WriteLine($"   Endpoint: GET api/lotes/turno/{turnoId}");

            // ✅ CORREGIDO: Deserializar el objeto completo que incluye success y data
            var response = await _http.GetFromJsonAsync<JsonElement>($"api/lotes/turno/{turnoId}");
            
            if (response.TryGetProperty("data", out var dataElement))
            {
                var lotes = JsonSerializer.Deserialize<List<LoteProduccionDto>>(
                    dataElement.GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                ) ?? new List<LoteProduccionDto>();
                
                Console.WriteLine($"✅ Lotes obtenidos: {lotes.Count}");
                
                if (lotes.Any())
                {
                    Console.WriteLine($"📋 Primeros lotes:");
                    foreach (var l in lotes.Take(3))
                    {
                        Console.WriteLine($"   - {l.NumeroLote}: {l.CantidadBolsas} bolsas ({l.FechaHoraInicio:HH:mm} - {l.FechaHoraFin:HH:mm})");
                    }
                }
                
                return lotes;
            }
            
            Console.WriteLine($"⚠️ No se encontró la propiedad 'data' en la respuesta");
            return new List<LoteProduccionDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error en GetLotesPorTurnoAsync: {ex.Message}");
            Console.WriteLine($"   StackTrace: {ex.StackTrace}");
            return new List<LoteProduccionDto>();
        }
    }

    public async Task<(bool success, string message)> CrearLoteAsync(CreateLoteProduccionDto dto)
    {
        try
        {
            Console.WriteLine($"🔄 Frontend - Creando lote");
            Console.WriteLine($"   TurnoID: {dto.TurnoID}");
            Console.WriteLine($"   Cantidad Bolsas: {dto.CantidadBolsas}");
            Console.WriteLine($"   Endpoint: POST api/lotes");

            var response = await _http.PostAsJsonAsync("api/lotes", dto);
            
            Console.WriteLine($"🔵 Status Code: {(int)response.StatusCode} {response.StatusCode}");
            
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"✅ Lote creado exitosamente");
                return (true, "Lote registrado exitosamente");
            }
            
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"❌ Error del backend: {error}");
            return (false, error);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Excepción en CrearLoteAsync: {ex.Message}");
            return (false, ex.Message);
        }
    }

    public async Task<ResultadoTrazabilidadDto> ConsultarTrazabilidadAsync(ConsultaTrazabilidadDto consulta)
    {
        try
        {
            Console.WriteLine($"🔄 Frontend - Consultando trazabilidad");
            Console.WriteLine($"   Fecha/Hora: {consulta.FechaHoraImpresa}");
            Console.WriteLine($"   Endpoint: POST api/lotes/trazabilidad");

            var response = await _http.PostAsJsonAsync("api/lotes/trazabilidad", consulta);
            
            Console.WriteLine($"🔵 Status Code: {(int)response.StatusCode} {response.StatusCode}");
            
            if (response.IsSuccessStatusCode)
            {
                var resultado = await response.Content.ReadFromJsonAsync<ResultadoTrazabilidadDto>();
                var res = resultado ?? new ResultadoTrazabilidadDto { Encontrado = false };
                
                Console.WriteLine($"✅ Resultado: {(res.Encontrado ? "Encontrado" : "No encontrado")}");
                return res;
            }
            
            Console.WriteLine($"❌ Error en consulta de trazabilidad");
            return new ResultadoTrazabilidadDto { Encontrado = false };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Excepción en ConsultarTrazabilidadAsync: {ex.Message}");
            return new ResultadoTrazabilidadDto { Encontrado = false };
        }
    }

    public async Task<ResultadoTrazabilidadDto?> BuscarTrazabilidadAsync(ConsultaTrazabilidadDto consulta)
    {
        return await ConsultarTrazabilidadAsync(consulta);
    }

    public async Task<LoteProduccionDto?> ObtenerLotePorId(int loteId)
    {
        try
        {
            // ✅ CORREGIDO: Deserializar el objeto completo que incluye success y data
            var response = await _http.GetFromJsonAsync<JsonElement>($"api/lotes/{loteId}");
            
            if (response.TryGetProperty("data", out var dataElement))
            {
                return JsonSerializer.Deserialize<LoteProduccionDto>(
                    dataElement.GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error en ObtenerLotePorId: {ex.Message}");
            return null;
        }
    }

    // Implementación requerida por la interfaz ILoteService
    public async Task<LoteProduccionDto?> GetLoteByIdAsync(int loteId)
    {
        return await ObtenerLotePorId(loteId);
    }
}