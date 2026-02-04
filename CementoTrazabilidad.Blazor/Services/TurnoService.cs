using System.Net.Http.Json;
using Blazored.LocalStorage;
using CementoTrazabilidad.Shared.DTOs;

namespace CementoTrazabilidad.Blazor.Services;

public class TurnoService : ITurnoService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;

    public TurnoService(HttpClient httpClient, ILocalStorageService localStorage)
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

    // ========== MÉTODO CRÍTICO CORREGIDO - CREAR TURNO ==========
    public async Task<bool> CreateTurnoAsync(CreateTurnoDto turno)
    {
        await ConfigureHttpClient();
        try
        {
            Console.WriteLine($"🔄 Frontend - Creando turno: {turno.Fecha} - Turno {turno.TurnoNumero}");
            Console.WriteLine($"   Endpoint: POST api/turnos");

            var response = await _httpClient.PostAsJsonAsync("api/turnos", turno);

            Console.WriteLine($"🔵 Status Code: {(int)response.StatusCode} {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"✅ Turno creado exitosamente");
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ Error del backend: {errorContent}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Excepción en CreateTurnoAsync: {ex.Message}");
            return false;
        }
    }

    // ========== MÉTODO ASIGNAR PERSONAL CORREGIDO ==========
    public async Task<bool> AsignarPersonalAsync(int turnoId, AsignarPersonalDto asignacion)
    {
        await ConfigureHttpClient();
        try
        {
            Console.WriteLine($"🔄 Frontend - Asignando personal {asignacion.PersonalId} al turno {turnoId}");
            Console.WriteLine($"   Endpoint: POST api/turnos/{turnoId}/asignar-personal");

            var response = await _httpClient.PostAsJsonAsync(
                $"api/turnos/{turnoId}/asignar-personal",
                asignacion
            );

            Console.WriteLine($"🔵 Status Code: {(int)response.StatusCode} {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"✅ Personal asignado exitosamente");
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ Error del backend: {errorContent}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Excepción en AsignarPersonalAsync: {ex.Message}");
            return false;
        }
    }

    // ========== MÉTODO GET PERSONAL TURNO CORREGIDO ==========
    public async Task<List<PersonalTurnoDto>> GetPersonalTurnoAsync(int turnoId)
    {
        await ConfigureHttpClient();
        try
        {
            Console.WriteLine($"🔄 Frontend - Obteniendo personal del turno {turnoId}");
            Console.WriteLine($"   Endpoint: GET api/turnos/{turnoId}/personal");

            var response = await _httpClient.GetAsync($"api/turnos/{turnoId}/personal");

            Console.WriteLine($"🔵 Status Code: {(int)response.StatusCode} {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                // ✅ CORRECCIÓN: Manejar respuesta envuelta
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<PersonalTurnoDto>>>();
                var personal = result?.Data ?? new List<PersonalTurnoDto>();
                
                Console.WriteLine($"✅ Personal obtenido: {personal.Count} personas");
                return personal;
            }
            else
            {
                Console.WriteLine($"❌ Error al obtener personal del turno");
                return new List<PersonalTurnoDto>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Excepción en GetPersonalTurnoAsync: {ex.Message}");
            return new List<PersonalTurnoDto>();
        }
    }

    // ========== MÉTODO INICIAR TURNO MEJORADO ==========
    public async Task<bool> IniciarTurnoAsync(int turnoId)
    {
        await ConfigureHttpClient();
        try
        {
            Console.WriteLine($"🔄 Frontend - Iniciando turno {turnoId}");
            Console.WriteLine($"   Endpoint: PUT api/turnos/{turnoId}/iniciar");

            var response = await _httpClient.PutAsync($"api/turnos/{turnoId}/iniciar", null);

            Console.WriteLine($"🔵 Status Code: {(int)response.StatusCode} {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"✅ Turno iniciado exitosamente");
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ Error del backend: {errorContent}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Excepción en IniciarTurnoAsync: {ex.Message}");
            return false;
        }
    }

    // ========== MÉTODO FINALIZAR TURNO MEJORADO ==========
    public async Task<bool> FinalizarTurnoAsync(int turnoId)
    {
        await ConfigureHttpClient();
        try
        {
            Console.WriteLine($"🔄 Frontend - Finalizando turno {turnoId}");
            Console.WriteLine($"   Endpoint: PUT api/turnos/{turnoId}/finalizar");

            var response = await _httpClient.PutAsync($"api/turnos/{turnoId}/finalizar", null);

            Console.WriteLine($"🔵 Status Code: {(int)response.StatusCode} {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"✅ Turno finalizado exitosamente");
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ Error del backend: {errorContent}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Excepción en FinalizarTurnoAsync: {ex.Message}");
            return false;
        }
    }

    // ========== MÉTODO GET TURNOS ASYNC CORREGIDO ==========
    public async Task<List<TurnoDto>> GetTurnosAsync()
    {
        await ConfigureHttpClient();
        try
        {
            Console.WriteLine($"🔄 Frontend - Obteniendo todos los turnos");
            Console.WriteLine($"   Endpoint: GET api/turnos");

            var response = await _httpClient.GetAsync("api/turnos");

            Console.WriteLine($"🔵 Status Code: {(int)response.StatusCode} {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                // ✅ CORRECCIÓN: El API devuelve { success, data, count }
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<TurnoDto>>>();
                var turnos = result?.Data ?? new List<TurnoDto>();
                
                Console.WriteLine($"✅ Turnos obtenidos: {turnos.Count}");
                
                if (turnos.Any())
                {
                    Console.WriteLine($"📋 Primeros turnos:");
                    foreach (var t in turnos.Take(3))
                    {
                        Console.WriteLine($"   - ID:{t.TurnoProduccionID} {t.Fecha} Turno#{t.TurnoNumero} Estado:{t.Estado}");
                    }
                }
                
                return turnos;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ Error al obtener turnos: {errorContent}");
                return new List<TurnoDto>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Excepción en GetTurnosAsync: {ex.Message}");
            Console.WriteLine($"   StackTrace: {ex.StackTrace}");
            return new List<TurnoDto>();
        }
    }

    // ✅ AGREGAR CLASE HELPER PARA DESERIALIZACIÓN
    private class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public int? Count { get; set; }
        public string? Message { get; set; }
    }

    // ========== MÉTODO GET TURNO BY ID CORREGIDO ==========
    public async Task<TurnoDto?> GetTurnoByIdAsync(int id)
    {
        await ConfigureHttpClient();
        try
        {
            Console.WriteLine($"🔄 Frontend - Obteniendo turno {id}");
            Console.WriteLine($"   Endpoint: GET api/turnos/{id}");

            var response = await _httpClient.GetAsync($"api/turnos/{id}");

            Console.WriteLine($"🔵 Status Code: {(int)response.StatusCode} {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                // ✅ CORRECCIÓN: Manejar respuesta envuelta
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<TurnoDto>>();
                var turno = result?.Data;
                
                if (turno != null)
                {
                    Console.WriteLine($"✅ Turno {id} obtenido (Estado: {turno.Estado})");
                }
                
                return turno;
            }
            else
            {
                Console.WriteLine($"❌ Error al obtener turno {id}");
                return null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Excepción en GetTurnoByIdAsync: {ex.Message}");
            return null;
        }
    }

    // ========== MÉTODO GET TURNO RESUMEN CORREGIDO ==========
    public async Task<TurnoResumenDto?> GetTurnoResumenAsync(int id)
    {
        await ConfigureHttpClient();
        try
        {
            Console.WriteLine($"🔄 Frontend - Obteniendo resumen del turno {id}");
            Console.WriteLine($"   Endpoint: GET api/turnos/{id}/resumen");

            var response = await _httpClient.GetAsync($"api/turnos/{id}/resumen");

            Console.WriteLine($"🔵 Status Code: {(int)response.StatusCode} {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                // ✅ CORRECCIÓN: Manejar respuesta envuelta
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<TurnoResumenDto>>();
                var resumen = result?.Data;
                
                Console.WriteLine($"✅ Resumen del turno {id} obtenido");
                return resumen;
            }
            else
            {
                Console.WriteLine($"❌ Error al obtener resumen del turno {id}");
                return null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Excepción en GetTurnoResumenAsync: {ex.Message}");
            return null;
        }
    }

    // ========== MÉTODOS EXISTENTES CON MEJORAS DE LOGGING ==========

    public async Task<List<TurnoDto>> GetTurnosDelDiaAsync(DateOnly fecha)
    {
        await ConfigureHttpClient();
        try
        {
            Console.WriteLine($"🔄 Frontend - Obteniendo turnos del día: {fecha:yyyy-MM-dd}");
            Console.WriteLine($"   Endpoint: GET api/turnos?fecha={fecha:yyyy-MM-dd}");

            var response = await _httpClient.GetAsync($"api/turnos?fecha={fecha:yyyy-MM-dd}");

            Console.WriteLine($"🔵 Status Code: {(int)response.StatusCode} {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                // ✅ CORRECCIÓN: Manejar respuesta envuelta
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<TurnoDto>>>();
                var turnos = result?.Data ?? new List<TurnoDto>();
                
                Console.WriteLine($"✅ Turnos del día obtenidos: {turnos.Count}");
                return turnos;
            }
            else
            {
                Console.WriteLine($"❌ Error al obtener turnos del día");
                return new List<TurnoDto>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Excepción en GetTurnosDelDiaAsync: {ex.Message}");
            return new List<TurnoDto>();
        }
    }

    public async Task<List<TurnoDto>> GetTurnosAbiertosAsync()
    {
        await ConfigureHttpClient();
        try
        {
            Console.WriteLine($"🔄 Frontend - Obteniendo turnos abiertos");

            // Obtener todos y filtrar localmente (temporal)
            var todos = await GetTurnosAsync();
            var abiertos = todos.Where(t => t.Estado == "Programado" || t.Estado == "En Proceso").ToList();

            Console.WriteLine($"✅ Turnos abiertos obtenidos: {abiertos.Count}");
            return abiertos;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Excepción en GetTurnosAbiertosAsync: {ex.Message}");
            return new List<TurnoDto>();
        }
    }

    public async Task<List<TurnoDto>> GetTurnosPorRangoAsync(DateOnly inicio, DateOnly fin)
    {
        await ConfigureHttpClient();
        try
        {
            Console.WriteLine($"🔄 Frontend - Obteniendo turnos por rango: {inicio:yyyy-MM-dd} a {fin:yyyy-MM-dd}");

            // Obtener todos y filtrar localmente (temporal)
            var todos = await GetTurnosAsync();
            var enRango = todos.Where(t => t.Fecha >= inicio && t.Fecha <= fin).ToList();

            Console.WriteLine($"✅ Turnos en rango obtenidos: {enRango.Count}");
            return enRango;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Excepción en GetTurnosPorRangoAsync: {ex.Message}");
            return new List<TurnoDto>();
        }
    }

    public async Task<TurnoDto?> GetTurnoActualAsync()
    {
        await ConfigureHttpClient();
        try
        {
            Console.WriteLine($"🔄 Frontend - Obteniendo turno actual");

            // Buscar turno en proceso del día actual
            var hoy = DateOnly.FromDateTime(DateTime.Today);
            var turnosHoy = await GetTurnosDelDiaAsync(hoy);
            var turnoActual = turnosHoy.FirstOrDefault(t => t.Estado == "En Proceso");

            if (turnoActual != null)
            {
                Console.WriteLine($"✅ Turno actual encontrado: ID {turnoActual.TurnoProduccionID}");
            }
            else
            {
                Console.WriteLine($"ℹ️ No hay turno actual en proceso");
            }

            return turnoActual;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Excepción en GetTurnoActualAsync: {ex.Message}");
            return null;
        }
    }

    public async Task<TurnoDetalleDto?> GetTurnoDetalleAsync(int turnoId)
    {
        await ConfigureHttpClient();
        try
        {
            Console.WriteLine($"🔄 Frontend - Obteniendo detalle del turno {turnoId}");

            // Implementación temporal - combinar datos de varios endpoints
            var turno = await GetTurnoByIdAsync(turnoId);
            if (turno == null)
            {
                Console.WriteLine($"❌ Turno {turnoId} no encontrado");
                return null;
            }

            var personal = await GetPersonalTurnoAsync(turnoId);
            var resumen = await GetTurnoResumenAsync(turnoId);

            // Crear DTO combinado
            var detalle = new TurnoDetalleDto
            {
                TurnoProduccionID = turno.TurnoProduccionID,
                Fecha = turno.Fecha,
                TurnoNumero = turno.TurnoNumero,
                Estado = turno.Estado,
                FechaHoraInicio = turno.FechaHoraInicio,
                FechaHoraFin = turno.FechaHoraFin,
                TotalBolsas = resumen?.TotalBolsas ?? 0,
                TotalToneladas = resumen?.TotalToneladas ?? 0,
                TotalBolsasRotas = resumen?.TotalBolsasRotas ?? 0,
                PorcentajeRotura = resumen?.PorcentajeRotura ?? 0,
                Personal = personal.Select(p => new PersonalDto
                {
                    PersonalID = p.PersonalID,
                    Nombre = p.PersonalNombre,
                    Legajo = p.PersonalLegajo,
                    Rol = p.RolTurno
                }).ToList()
            };

            Console.WriteLine($"✅ Detalle del turno {turnoId} obtenido");
            return detalle;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Excepción en GetTurnoDetalleAsync: {ex.Message}");
            return null;
        }
    }

    // ========== MÉTODOS TEMPORALES O NO IMPLEMENTADOS ==========

    public async Task<bool> RemoverPersonalAsync(int turnoId, int personalId)
    {
        await ConfigureHttpClient();
        try
        {
            Console.WriteLine($"⚠️ RemoverPersonalAsync - Endpoint no implementado aún");
            Console.WriteLine($"   Turno: {turnoId}, Personal: {personalId}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Excepción en RemoverPersonalAsync: {ex.Message}");
            return false;
        }
    }

    // Sobrecarga 2: Crear e iniciar turno (usa CreateTurnoAsync)
    public async Task<bool> IniciarTurnoAsync(CreateTurnoDto request)
    {
        Console.WriteLine($"🔄 Frontend - Creando e iniciando turno (sobrecarga)");
        return await CreateTurnoAsync(request);
    }

    public async Task<bool> RegistrarProduccionAsync(CreateProduccionDto request)
    {
        await ConfigureHttpClient();
        try
        {
            Console.WriteLine($"🔄 Frontend - Registrando producción para turno {request.TurnoProduccionID}");
            Console.WriteLine($"   Material: {request.MaterialID}, Bolsas: {request.BolsasElaboradas}");
            Console.WriteLine($"   Endpoint: POST api/turnos/{request.TurnoProduccionID}/produccion");

            var response = await _httpClient.PostAsJsonAsync(
                $"api/turnos/{request.TurnoProduccionID}/produccion",
                request
            );

            Console.WriteLine($"🔵 Status Code: {(int)response.StatusCode} {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"✅ Producción registrada exitosamente");
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ Error del backend: {errorContent}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Excepción en RegistrarProduccionAsync: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> RegistrarParadaAsync(CreateParadaDto request)
    {
        await ConfigureHttpClient();
        try
        {
            Console.WriteLine($"🔄 Frontend - Registrando parada para turno {request.TurnoProduccionID}");
            Console.WriteLine($"   Tipo: {request.Tipo}, Duración: {(request.FechaHoraFin - request.FechaHoraInicio)?.TotalMinutes ?? 0} min");
            Console.WriteLine($"   Endpoint: POST api/turnos/{request.TurnoProduccionID}/paradas");  // ✅ Cambiar a "paradas" (PLURAL)

            var response = await _httpClient.PostAsJsonAsync(
                $"api/turnos/{request.TurnoProduccionID}/paradas",  // ✅ Cambiar a "paradas" (PLURAL)
                request
            );

            Console.WriteLine($"🔵 Status Code: {(int)response.StatusCode} {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"✅ Parada registrada exitosamente");
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ Error del backend: {errorContent}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Excepción en RegistrarParadaAsync: {ex.Message}");
            return false;
        }
    }

    public async Task<List<ParadaDto>> GetParadasTurnoAsync(int turnoId)
    {
        await ConfigureHttpClient();
        try
        {
            Console.WriteLine($"🔄 Frontend - Obteniendo paradas del turno {turnoId}");
            Console.WriteLine($"   Endpoint: GET api/turnos/{turnoId}/paradas");

            var response = await _httpClient.GetAsync($"api/turnos/{turnoId}/paradas");

            Console.WriteLine($"🔵 Status Code: {(int)response.StatusCode} {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<ParadaDto>>>();
                var paradas = result?.Data ?? new List<ParadaDto>();
                
                Console.WriteLine($"✅ Paradas obtenidas: {paradas.Count}");
                return paradas;
            }
            else
            {
                Console.WriteLine($"❌ Error al obtener paradas del turno");
                return new List<ParadaDto>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Excepción en GetParadasTurnoAsync: {ex.Message}");
            return new List<ParadaDto>();
        }
    }

    // ========== MÉTODO GET TURNO ACTIVO ==========
    public async Task<TurnoProduccionDto?> GetTurnoActivoAsync()
    {
        await ConfigureHttpClient();
        try
        {
            Console.WriteLine($"🔄 Frontend - Obteniendo turno activo");
            Console.WriteLine($"   Endpoint: GET api/turnos/activo");

            var response = await _httpClient.GetAsync("api/turnos/activo");

            Console.WriteLine($"🔵 Status Code: {(int)response.StatusCode} {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<TurnoProduccionDto>>();
                var turnoActivo = result?.Data;
                
                if (turnoActivo != null)
                {
                    Console.WriteLine($"✅ Turno activo encontrado: ID {turnoActivo.TurnoProduccionID}");
                }
                else
                {
                    Console.WriteLine($"ℹ️ No hay turno activo");
                }
                
                return turnoActivo;
            }
            else
            {
                Console.WriteLine($"❌ Error al obtener turno activo");
                return null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Excepción en GetTurnoActivoAsync: {ex.Message}");
            return null;
        }
    }
}