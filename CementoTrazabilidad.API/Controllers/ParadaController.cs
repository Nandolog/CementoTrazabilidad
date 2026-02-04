using CementoTrazabilidad.Core.Entidades;
using CementoTrazabilidad.Infrastructure.Data;
using CementoTrazabilidad.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CementoTrazabilidad.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ParadaController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ParadaController> _logger;

    public ParadaController(ApplicationDbContext context, ILogger<ParadaController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/parada/turno/5
    [HttpGet("turno/{turnoId}")]
    public async Task<IActionResult> GetParadasPorTurno(int turnoId)
    {
        try
        {
            var paradas = await _context.Paradas
                .Where(p => p.TurnoProduccionID == turnoId)
                .OrderByDescending(p => p.FechaHoraInicio)
                .Select(p => new ParadaDto
                {
                    ParadaID = p.ParadaID,
                    TurnoProduccionID = p.TurnoProduccionID,
                    Tipo = p.TipoParada,
                    Motivo = p.TipoParada, // ✅ Usar TipoParada como Motivo (ya que no existe Motivo en BD)
                    Descripcion = p.Descripcion,
                    FechaHoraInicio = p.FechaHoraInicio,
                    FechaHoraFin = p.FechaHoraFin,
                    DuracionMinutos = p.FechaHoraFin.HasValue
                        ? (int)(p.FechaHoraFin.Value - p.FechaHoraInicio).TotalMinutes
                        : null,
                    AccionesCorrectivas = null // ✅ No existe en BD
                })
                .ToListAsync();

            return Ok(new ApiResponse<List<ParadaDto>>
            {
                Success = true,
                Data = paradas,
                Count = paradas.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener paradas del turno {TurnoId}", turnoId);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Error al obtener paradas"
            });
        }
    }

    // POST: api/parada
    [HttpPost]
    public async Task<IActionResult> CrearParada([FromBody] ParadaDto paradaDto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Datos inválidos"
                });

            var turno = await _context.TurnosProduccion
                .FirstOrDefaultAsync(t => t.TurnoProduccionID == paradaDto.TurnoProduccionID);

            if (turno == null)
                return NotFound(new ApiResponse<object> { Success = false, Message = "Turno no encontrado" });

            if (turno.Estado != "En Proceso")
                return BadRequest(new ApiResponse<object> { Success = false, Message = "El turno no está en proceso" });

            // Auto-finalizar cargas activas
            var eventosTurno = await _context.EventosCarga
                .Where(e => e.TurnoProduccionID == paradaDto.TurnoProduccionID)
                .OrderBy(e => e.FechaHora)
                .ToListAsync();

            var cargasActivas = new List<(string Zona, DateTime Inicio)>();
            var estadoZonas = new Dictionary<string, (bool EnCurso, DateTime? UltimoInicio)>
            {
                { "Anden", (false, null) },
                { "Paletizado", (false, null) }
            };

            foreach (var evento in eventosTurno)
            {
                if (estadoZonas.ContainsKey(evento.ZonaCarga))
                {
                    if (evento.TipoEvento == "Inicio")
                        estadoZonas[evento.ZonaCarga] = (true, evento.FechaHora);
                    else if (evento.TipoEvento == "Fin")
                        estadoZonas[evento.ZonaCarga] = (false, null);
                }
            }

            foreach (var zona in estadoZonas)
            {
                if (zona.Value.EnCurso && zona.Value.UltimoInicio.HasValue)
                    cargasActivas.Add((zona.Key, zona.Value.UltimoInicio.Value));
            }

            var cargasFinalizadas = new List<string>();
            foreach (var (zona, inicioFecha) in cargasActivas)
            {
                var eventoFin = new EventoCarga
                {
                    TurnoProduccionID = paradaDto.TurnoProduccionID,
                    ZonaCarga = zona,
                    TipoEvento = "Fin",
                    FechaHora = paradaDto.FechaHoraInicio,
                    Observaciones = $"🛑 Finalizado automáticamente por parada: {paradaDto.Tipo}"
                };
                _context.EventosCarga.Add(eventoFin);
                cargasFinalizadas.Add(zona);
            }

            // ✅ CREAR PARADA - SOLO con columnas que EXISTEN en BD
            var parada = new Parada
            {
                TurnoProduccionID = paradaDto.TurnoProduccionID,
                TipoParada = paradaDto.Tipo ?? "No especificado",
                Descripcion = paradaDto.Descripcion ?? "Sin descripción",
                FechaHoraInicio = paradaDto.FechaHoraInicio,
                FechaHoraFin = paradaDto.FechaHoraFin
                // ✅ NO asignar: Motivo, Estado, AccionesCorrectivas, PersonalResponsableID (no existen en BD)
            };

            _context.Paradas.Add(parada);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation(
                "✅ Parada {ParadaId} creada. Tipo: {Tipo}, Duración: {Duracion} min",
                parada.ParadaID, parada.TipoParada, parada.DuracionMinutos ?? (parada.FechaHoraFin.HasValue ? (int?)(parada.FechaHoraFin.Value - parada.FechaHoraInicio).TotalMinutes : null));

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = cargasFinalizadas.Any()
                    ? $"Parada registrada. Se finalizaron automáticamente {cargasFinalizadas.Count} carga(s): {string.Join(", ", cargasFinalizadas)}"
                    : "Parada registrada exitosamente",
                Data = new
                {
                    parada.ParadaID,
                    parada.TurnoProduccionID,
                    Tipo = parada.TipoParada,
                    parada.Descripcion,
                    parada.FechaHoraInicio,
                    parada.FechaHoraFin,
                    Estado = parada.Estado,
                    // Usar DuracionMinutos público o calcularlo aquí si es null
                    DuracionMinutos = parada.DuracionMinutos ?? (parada.FechaHoraFin.HasValue ? (int?)(parada.FechaHoraFin.Value - parada.FechaHoraInicio).TotalMinutes : null),
                    ImpactoProductivo = parada.ImpactoProductivo,
                    CargasFinalizadas = cargasFinalizadas
                }
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error al crear parada");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = $"Error al registrar parada: {ex.Message}"
            });
        }
    }

    // PUT: api/parada/5
    [HttpPut("{id}")]
    public async Task<IActionResult> ActualizarParada(int id, [FromBody] ParadaDto paradaDto)
    {
        try
        {
            var parada = await _context.Paradas.FindAsync(id);

            if (parada == null)
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Parada no encontrada"
                });

            // ✅ Actualizar SOLO columnas que existen
            parada.TipoParada = paradaDto.Tipo;
            parada.Descripcion = paradaDto.Descripcion;
            parada.FechaHoraInicio = paradaDto.FechaHoraInicio;
            parada.FechaHoraFin = paradaDto.FechaHoraFin;

            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Parada actualizada exitosamente"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar parada {ParadaId}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Error al actualizar parada"
            });
        }
    }

    // DELETE: api/parada/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<IActionResult> EliminarParada(int id)
    {
        try
        {
            var parada = await _context.Paradas.FindAsync(id);

            if (parada == null)
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Parada no encontrada"
                });

            _context.Paradas.Remove(parada);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Parada eliminada exitosamente"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar parada {ParadaId}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Error al eliminar parada"
            });
        }
    }

    // GET: api/parada/turno/5/activas
    [HttpGet("turno/{turnoId}/activas")]
    public async Task<IActionResult> GetCargasActivas(int turnoId)
    {
        try
        {
            var eventos = await _context.EventosCarga
                .Where(e => e.TurnoProduccionID == turnoId)
                .OrderBy(e => e.FechaHora)
                .ToListAsync();

            var zonasActivas = new List<string>();
            var estadoZonas = new Dictionary<string, bool>
            {
                { "Anden", false },
                { "Paletizado", false }
            };

            foreach (var evento in eventos)
            {
                if (estadoZonas.ContainsKey(evento.ZonaCarga))
                {
                    estadoZonas[evento.ZonaCarga] = evento.TipoEvento == "Inicio";
                }
            }

            zonasActivas = estadoZonas
                .Where(kv => kv.Value)
                .Select(kv => kv.Key)
                .ToList();

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = new
                {
                    HayCargasActivas = zonasActivas.Any(),
                    ZonasActivas = zonasActivas,
                    Cantidad = zonasActivas.Count
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cargas activas del turno {TurnoId}", turnoId);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Error al obtener cargas activas"
            });
        }
    }

    // PUT: api/parada/5/finalizar
    [HttpPut("{id}/finalizar")]
    [Authorize(Roles = "Administrador,Supervisor,JefeTurno,Operario")]
    public async Task<IActionResult> FinalizarParada(int id, [FromBody] FinalizarParadaDto dto)
    {
        try
        {
            var parada = await _context.Paradas.FindAsync(id);
            if (parada == null)
                return NotFound(new { success = false, message = "Parada no encontrada" });
            
            if (parada.FechaHoraFin.HasValue)
                return BadRequest(new { success = false, message = "La parada ya está finalizada" });
            
            parada.FechaHoraFin = dto.FechaHoraFin ?? DateTime.Now;
            
            await _context.SaveChangesAsync();
            
            return Ok(new { success = true, message = "Parada finalizada" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al finalizar parada");
            return StatusCode(500, new { success = false, message = "Error interno" });
        }
    }

    public class FinalizarParadaDto
    {
        public DateTime? FechaHoraFin { get; set; }
    }
}