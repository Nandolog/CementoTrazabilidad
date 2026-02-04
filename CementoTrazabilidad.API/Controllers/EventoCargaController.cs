using CementoTrazabilidad.Core.Entidades;
using CementoTrazabilidad.Infrastructure.Data;
using CementoTrazabilidad.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; // Agrega este using

namespace CementoTrazabilidad.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class EventoCargaController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<EventoCargaController> _logger; // Agrega este campo

    public EventoCargaController(ApplicationDbContext context, ILogger<EventoCargaController> logger) // Modifica el constructor
    {
        _context = context;
        _logger = logger; // Asigna el logger
    }

    // POST: api/eventocarga
    // POST: api/eventocarga
    [HttpPost]
    public async Task<IActionResult> RegistrarEvento([FromBody] RegistrarEventoCargaDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Datos inválidos"
                });

            // Validar que el turno existe y está activo
            var turno = await _context.TurnosProduccion
                .FirstOrDefaultAsync(t => t.TurnoProduccionID == dto.TurnoProduccionID);

            if (turno == null)
                return NotFound(new ApiResponse<object> { Success = false, Message = "Turno no encontrado" });

            if (turno.Estado != "En Proceso")
                return BadRequest(new ApiResponse<object> { Success = false, Message = "El turno no está en proceso" });

            // ✅ NUEVA VALIDACIÓN: Verificar si hay paradas activas
            if (dto.TipoEvento == "Inicio")
            {
                // Buscar paradas activas (sin FechaHoraFin)
                var paradaActiva = await _context.Paradas
                    .Where(p => p.TurnoProduccionID == dto.TurnoProduccionID &&
                               !p.FechaHoraFin.HasValue)
                    .FirstOrDefaultAsync();

                if (paradaActiva != null)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"No se puede iniciar la carga. Hay una parada activa de tipo '{paradaActiva.TipoParada}' desde {paradaActiva.FechaHoraInicio:HH:mm}. Debe finalizarla primero."
                    });
                }

                // También verificar paradas recientemente finalizadas (tiempo mínimo)
                var ahora = DateTime.Now;
                var tiempoMinimoMinutos = 0; // Ajusta según tu regla de negocio

                var paradaReciente = await _context.Paradas
                    .Where(p => p.TurnoProduccionID == dto.TurnoProduccionID &&
                               p.FechaHoraFin.HasValue)
                    .OrderByDescending(p => p.FechaHoraFin)
                    .FirstOrDefaultAsync();

                if (paradaReciente != null && paradaReciente.FechaHoraFin.HasValue)
                {
                    var tiempoTranscurrido = (ahora - paradaReciente.FechaHoraFin.Value).TotalMinutes;
                    
                    if (tiempoTranscurrido < tiempoMinimoMinutos)
                    {
                        var tiempoRestante = Math.Ceiling(tiempoMinimoMinutos - tiempoTranscurrido);
                        return BadRequest(new ApiResponse<object>
                        {
                            Success = false,
                            Message = $"Debe esperar {tiempoRestante} minuto(s) más después de finalizar la parada antes de iniciar una nueva carga. Parada finalizada: {paradaReciente.FechaHoraFin.Value:HH:mm:ss}"
                        });
                    }
                }
            }

            // Obtener TODOS los eventos del turno ordenados por fecha
            var eventosTurno = await _context.EventosCarga
                .Where(e => e.TurnoProduccionID == dto.TurnoProduccionID)
                .OrderBy(e => e.FechaHora)
                .ToListAsync();

            // Verificar el estado actual de cada zona
            var estadoZonas = new Dictionary<string, string>
            {
                { "Anden", "No iniciado" },
                { "Paletizado", "No iniciado" }
            };

            // Procesar eventos para determinar estado actual
            foreach (var evento in eventosTurno)
            {
                if (estadoZonas.ContainsKey(evento.ZonaCarga))
                {
                    estadoZonas[evento.ZonaCarga] = evento.TipoEvento == "Inicio" ? "En curso" : "Finalizado";
                }
            }

            // Para evento "Inicio": verificar que no haya otra zona en curso
            if (dto.TipoEvento == "Inicio")
            {
                // Verificar si ya está en curso
                if (estadoZonas[dto.ZonaCarga] == "En curso")
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"La zona '{dto.ZonaCarga}' ya está en curso. Finalízala primero."
                    });
                }

                // Verificar si hay otra zona en curso (máquina única)
                var otraZonaEnCurso = estadoZonas
                    .Where(kv => kv.Key != dto.ZonaCarga && kv.Value == "En curso")
                    .FirstOrDefault();

                if (otraZonaEnCurso.Key != null)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"La zona '{otraZonaEnCurso.Key}' está en curso. Finalízala antes de iniciar otra."
                    });
                }
            }
            else if (dto.TipoEvento == "Fin")
            {
                // Para evento "Fin": verificar que esté en curso
                if (estadoZonas[dto.ZonaCarga] != "En curso")
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"La zona '{dto.ZonaCarga}' no está en curso para finalizar."
                    });
                }
            }

            // Crear el nuevo evento
            var eventoNuevo = new EventoCarga
            {
                TurnoProduccionID = dto.TurnoProduccionID,
                ZonaCarga = dto.ZonaCarga,
                TipoEvento = dto.TipoEvento,
                FechaHora = DateTime.Now,
                Observaciones = dto.Observaciones
            };

            _context.EventosCarga.Add(eventoNuevo);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = $"Evento de {dto.TipoEvento} registrado exitosamente para {dto.ZonaCarga}",
                Data = new
                {
                    eventoNuevo.EventoCargaID,
                    eventoNuevo.TurnoProduccionID,
                    eventoNuevo.ZonaCarga,
                    eventoNuevo.TipoEvento,
                    eventoNuevo.FechaHora,
                    eventoNuevo.Observaciones
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = $"Error interno del servidor: {ex.Message}"
            });
        }
    }

    // GET: api/eventocarga/turno/5/resumen
    [HttpGet("turno/{turnoId}/resumen")]
    public async Task<IActionResult> GetResumen(int turnoId)
    {
        try
        {
            // Verificar que el turno existe
            var turnoExiste = await _context.TurnosProduccion
                .AnyAsync(t => t.TurnoProduccionID == turnoId);

            if (!turnoExiste)
            {
                return NotFound(new ApiResponse<List<ResumenCargaDto>>
                {
                    Success = false,
                    Message = $"Turno con ID {turnoId} no encontrado"
                });
            }

            var eventos = await _context.EventosCarga
                .Where(e => e.TurnoProduccionID == turnoId)
                .OrderBy(e => e.FechaHora)
                .ToListAsync();

            var resumen = new List<ResumenCargaDto>();

            // Definir zonas estándar
            var zonas = new[] { "Anden", "Paletizado" };

            foreach (var zona in zonas)
            {
                // Obtener eventos específicos de esta zona
                var eventosZona = eventos
                    .Where(e => e.ZonaCarga == zona)
                    .OrderBy(e => e.FechaHora)
                    .ToList();

                DateTime? inicio = null;
                DateTime? fin = null;
                string estado = "No iniciado";

                // Buscar último inicio
                var ultimoInicio = eventosZona
                    .LastOrDefault(e => e.TipoEvento == "Inicio");

                if (ultimoInicio != null)
                {
                    inicio = ultimoInicio.FechaHora;
                    estado = "En curso"; // Suponemos que está en curso

                    // Buscar un Fin que venga después de este Inicio
                    var finPosterior = eventosZona
                        .Where(e => e.TipoEvento == "Fin" && e.FechaHora > inicio.Value)
                        .OrderByDescending(e => e.FechaHora)
                        .FirstOrDefault();

                    if (finPosterior != null)
                    {
                        fin = finPosterior.FechaHora;
                        estado = "Finalizado";
                    }
                }

                TimeSpan? tiempoTotal = null;
                if (inicio.HasValue && fin.HasValue)
                {
                    tiempoTotal = fin.Value - inicio.Value;
                }

                resumen.Add(new ResumenCargaDto
                {
                    ZonaCarga = zona,
                    HoraInicio = inicio,
                    HoraFin = fin,
                    TiempoTotal = tiempoTotal,
                    Estado = estado
                });
            }

            // DEVUELVE EL ApiResponse CORRECTO
            return Ok(new ApiResponse<List<ResumenCargaDto>>
            {
                Success = true,
                Data = resumen,
                Count = resumen.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetResumen para turno {TurnoId}", turnoId);

            return StatusCode(500, new ApiResponse<List<ResumenCargaDto>>
            {
                Success = false,
                Message = $"Error interno: {ex.Message}"
            });
        }
    }

    // GET: api/eventocarga/turno/5
    [HttpGet("turno/{turnoId}")]
    public async Task<IActionResult> GetEventosPorTurno(int turnoId)
    {
        var eventos = await _context.EventosCarga
            .Where(e => e.TurnoProduccionID == turnoId)
            .OrderBy(e => e.FechaHora)
            .Select(e => new EventoCargaDto
            {
                EventoCargaID = e.EventoCargaID,
                TurnoProduccionID = e.TurnoProduccionID,
                ZonaCarga = e.ZonaCarga,
                TipoEvento = e.TipoEvento,
                FechaHora = e.FechaHora,
                Observaciones = e.Observaciones
            })
            .ToListAsync();

        return Ok(new { success = true, data = eventos });
    }

    // DELETE: api/eventocarga/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<IActionResult> Delete(int id)
    {
        var evento = await _context.EventosCarga.FindAsync(id);
        if (evento == null)
            return NotFound(new { success = false, message = "Evento no encontrado" });

        _context.EventosCarga.Remove(evento);
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Evento eliminado" });
    }

    // En tu controller de Paradas (backend)
    [HttpGet("turno/{turnoId}/activas")]
    public async Task<IActionResult> GetParadasActivas(int turnoId)
    {
        var paradas = await _context.Paradas
            .Where(p => p.TurnoProduccionID == turnoId &&
                       p.Estado == "En curso") // O el campo que uses para estado
            .ToListAsync();

        // Mapear entidades Parada a ParadaDto
        var paradasDto = paradas.Select(p => new ParadaDto
        {
            ParadaID = p.ParadaID,
            TurnoProduccionID = p.TurnoProduccionID,
            Tipo = p.TipoParada,
            Descripcion = p.Descripcion,
            FechaHoraInicio = p.FechaHoraInicio,
            FechaHoraFin = p.FechaHoraFin,
            DuracionMinutos = p.FechaHoraFin.HasValue ? (int?)(p.FechaHoraFin.Value - p.FechaHoraInicio).TotalMinutes : null,
            AccionesCorrectivas = null // Asignar si existe en tu modelo
        }).ToList();

        return Ok(new ApiResponse<List<ParadaDto>>
        {
            Success = true,
            Data = paradasDto,
            Message = null,
            Count = paradasDto.Count
        });
    }

    // GET: api/eventocarga/turno/5/historial
    [HttpGet("turno/{turnoId}/historial")]
    public async Task<IActionResult> GetHistorialEventos(int turnoId)
    {
        try
        {
            var eventos = await _context.EventosCarga
                .Where(e => e.TurnoProduccionID == turnoId)
                .OrderBy(e => e.FechaHora)
                .Select(e => new
                {
                    e.EventoCargaID,
                    e.ZonaCarga,
                    e.TipoEvento,
                    e.FechaHora,
                    e.Observaciones
                })
                .ToListAsync();

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = eventos,
                Count = eventos.Count
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = $"Error al obtener historial: {ex.Message}"
            });
        }
    }

    // GET: api/eventocarga/turno/{turnoId}/dashboard
    [HttpGet("turno/{turnoId}/dashboard")]
    public async Task<IActionResult> GetDashboard(int turnoId)
    {
        try
        {
            _logger.LogInformation($"📊 Obteniendo dashboard para turno {turnoId}");

            // 1. Obtener consumo de bolsas de EventosCarga
            var consumoBolsas = await _context.EventosCarga
                .Where(e => e.TurnoProduccionID == turnoId && e.CantidadBolsas.HasValue)
                .SumAsync(e => e.CantidadBolsas.Value);

            consumoBolsas = consumoBolsas > 0 ? consumoBolsas : 0;

            // 2. Obtener lotes del turno - AHORA SÍ FUNCIONARÁ
            var lotes = await _context.LotesProduccion
                .Where(l => l.TurnoID == turnoId)  // ← TurnoID
                .OrderByDescending(l => l.FechaHoraInicio)
                .Take(10)
                .Select(l => new
                {
                    loteID = l.LoteID,
                    numeroLote = l.NumeroLote,
                    fechaHoraInicio = l.FechaHoraInicio,
                    cantidadBolsas = l.CantidadBolsas,
                    materialID = l.MaterialID,
                    tipoRegistro = l.TipoRegistro
                })
                .ToListAsync();

            _logger.LogInformation($"📦 Consumo bolsas: {consumoBolsas}, 📋 Lotes: {lotes.Count}");

            return Ok(new
            {
                success = true,
                data = new
                {
                    consumoBolsas,
                    lotes
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error en GetDashboard");

            return StatusCode(500, new
            {
                success = false,
                message = $"Error: {ex.Message}"
            });
        }
    }

    // GET: api/eventocarga/test-dashboard
    [HttpGet("test-dashboard")]
    public IActionResult GetTestDashboard()
    {
        try
        {
            var lotes = new List<object>
        {
            new
            {
                loteID = 1,
                numeroLote = "T1-" + DateTime.Now.ToString("yyyyMMdd") + "-001",
                fechaHoraInicio = DateTime.Now.AddHours(-2),
                cantidadBolsas = 1500
            },
            new
            {
                loteID = 2,
                numeroLote = "T1-" + DateTime.Now.ToString("yyyyMMdd") + "-002",
                fechaHoraInicio = DateTime.Now.AddHours(-1),
                cantidadBolsas = 1200
            },
            new
            {
                loteID = 3,
                numeroLote = "T1-" + DateTime.Now.ToString("yyyyMMdd") + "-003",
                fechaHoraInicio = DateTime.Now.AddMinutes(-30),
                cantidadBolsas = 800
            }
        };

            return Ok(new
            {
                success = true,
                data = new
                {
                    consumoBolsas = 3500,
                    lotes = lotes
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = $"Error: {ex.Message}"
            });
        }
    }
}