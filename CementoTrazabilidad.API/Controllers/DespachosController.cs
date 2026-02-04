using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CementoTrazabilidad.Infrastructure.Data;
using CementoTrazabilidad.Core.Entidades;
using CementoTrazabilidad.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace CementoTrazabilidad.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DespachosController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DespachosController> _logger;

    public DespachosController(ApplicationDbContext context, ILogger<DespachosController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DespachoDTO>>> GetDespachos()
    {
        try
        {
            var despachos = await _context.Despachos
                .Include(d => d.Turno)
                .OrderByDescending(d => d.FechaHoraDespacho)
                .Select(d => new DespachoDTO
                {
                    DespachoID = d.DespachoID,
                    TurnoProduccionID = d.TurnoProduccionID,
                    MaterialID = d.MaterialID,
                    Modalidad = d.Modalidad,
                    Destino = d.Destino,
                    Bolsas = d.Bolsas,
                    Toneladas = d.Toneladas,
                    Camiones = d.Camiones,
                    FechaHoraDespacho = d.FechaHoraDespacho,
                    TurnoNumero = d.Turno != null ? d.Turno.TurnoNumero : null,
                    TurnoFecha = d.Turno != null ? (DateTime?)d.Turno.Fecha.ToDateTime(TimeOnly.MinValue) : null,
                    MaterialDescripcion = d.MaterialID != null 
                        ? _context.Materiales.Where(m => m.MaterialID == d.MaterialID).Select(m => m.Descripcion).FirstOrDefault()
                        : null
                })
                .ToListAsync();

            return Ok(despachos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener despachos");
            return StatusCode(500, "Error al obtener despachos");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DespachoDTO>> GetDespacho(int id)
    {
        try
        {
            var despacho = await _context.Despachos
                .Include(d => d.Turno)
                .Where(d => d.DespachoID == id)
                .Select(d => new DespachoDTO
                {
                    DespachoID = d.DespachoID,
                    TurnoProduccionID = d.TurnoProduccionID,
                    MaterialID = d.MaterialID,
                    Modalidad = d.Modalidad,
                    Destino = d.Destino,
                    Bolsas = d.Bolsas,
                    Toneladas = d.Toneladas,
                    Camiones = d.Camiones,
                    FechaHoraDespacho = d.FechaHoraDespacho,
                    TurnoNumero = d.Turno != null ? d.Turno.TurnoNumero : null,
                    TurnoFecha = d.Turno != null ? (DateTime?)d.Turno.Fecha.ToDateTime(TimeOnly.MinValue) : null,
                    MaterialDescripcion = d.MaterialID != null 
                        ? _context.Materiales.Where(m => m.MaterialID == d.MaterialID).Select(m => m.Descripcion).FirstOrDefault()
                        : null
                })
                .FirstOrDefaultAsync();

            if (despacho == null)
                return NotFound();

            return Ok(despacho);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al obtener despacho {id}");
            return StatusCode(500, "Error al obtener despacho");
        }
    }

    [HttpPost]
    public async Task<ActionResult<DespachoDTO>> CreateDespacho(CreateDespachoDTO dto)
    {
        try
        {
            var despacho = new Despacho
            {
                TurnoProduccionID = dto.TurnoProduccionID,
                MaterialID = dto.MaterialID,
                Modalidad = dto.Modalidad,
                Destino = dto.Destino,
                Bolsas = dto.Bolsas,
                Toneladas = dto.Toneladas,
                Camiones = dto.Camiones,
                FechaHoraDespacho = dto.FechaHoraDespacho
            };

            _context.Despachos.Add(despacho);
            await _context.SaveChangesAsync();

            var resultado = await _context.Despachos
                .Include(d => d.Turno)
                .Where(d => d.DespachoID == despacho.DespachoID)
                .Select(d => new DespachoDTO
                {
                    DespachoID = d.DespachoID,
                    TurnoProduccionID = d.TurnoProduccionID,
                    MaterialID = d.MaterialID,
                    Modalidad = d.Modalidad,
                    Destino = d.Destino,
                    Bolsas = d.Bolsas,
                    Toneladas = d.Toneladas,
                    Camiones = d.Camiones,
                    FechaHoraDespacho = d.FechaHoraDespacho,
                    TurnoNumero = d.Turno != null ? d.Turno.TurnoNumero : null,
                    TurnoFecha = d.Turno != null ? (DateTime?)d.Turno.Fecha.ToDateTime(TimeOnly.MinValue) : null,
                    MaterialDescripcion = d.MaterialID != null 
                        ? _context.Materiales.Where(m => m.MaterialID == d.MaterialID).Select(m => m.Descripcion).FirstOrDefault()
                        : null
                })
                .FirstOrDefaultAsync();

            return CreatedAtAction(nameof(GetDespacho), new { id = despacho.DespachoID }, resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear despacho");
            return StatusCode(500, "Error al crear despacho");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDespacho(int id, UpdateDespachoDTO dto)
    {
        try
        {
            var despacho = await _context.Despachos.FindAsync(id);
            if (despacho == null)
                return NotFound();

            despacho.TurnoProduccionID = dto.TurnoProduccionID;
            despacho.MaterialID = dto.MaterialID;
            despacho.Modalidad = dto.Modalidad;
            despacho.Destino = dto.Destino;
            despacho.Bolsas = dto.Bolsas;
            despacho.Toneladas = dto.Toneladas;
            despacho.Camiones = dto.Camiones;
            despacho.FechaHoraDespacho = dto.FechaHoraDespacho;

            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al actualizar despacho {id}");
            return StatusCode(500, "Error al actualizar despacho");
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> DeleteDespacho(int id)
    {
        try
        {
            var despacho = await _context.Despachos.FindAsync(id);
            if (despacho == null)
                return NotFound();

            _context.Despachos.Remove(despacho);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al eliminar despacho {id}");
            return StatusCode(500, "Error al eliminar despacho");
        }
    }
}