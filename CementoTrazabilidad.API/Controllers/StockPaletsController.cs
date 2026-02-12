using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using CementoTrazabilidad.Infrastructure.Data;
using CementoTrazabilidad.Core.Entidades;
using CementoTrazabilidad.Shared.DTOs;

namespace CementoTrazabilidad.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StockPaletsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<StockPaletsController> _logger;

        public StockPaletsController(ApplicationDbContext context, ILogger<StockPaletsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/stockpalets/turno/{turnoId}
        [HttpGet("turno/{turnoId}")]
        public async Task<ActionResult<RegistroStockPaletsDto>> GetByTurno(int turnoId)
        {
            var registro = await _context.RegistrosStockPalets
                .Include(r => r.PersonalRegistro)
                .Where(r => r.TurnoProduccionID == turnoId && r.Activo)
                .OrderByDescending(r => r.FechaHoraRegistroInicial)
                .FirstOrDefaultAsync();

            if (registro == null)
            {
                return NotFound(new { success = false, message = "No hay registro de stock para este turno" });
            }

            var dto = MapToDto(registro);
            return Ok(new { success = true, data = dto });
        }

        // POST: api/stockpalets/inicial
        [HttpPost("inicial")]
        public async Task<ActionResult<RegistroStockPaletsDto>> RegistrarStockInicial(CreateStockInicialDto request)
        {
            try
            {
                _logger.LogInformation($"📦 Registrando stock inicial para turno {request.TurnoProduccionID}");

                // Verificar que el turno existe
                var turno = await _context.TurnosProduccion.FindAsync(request.TurnoProduccionID);
                if (turno == null)
                {
                    return NotFound(new { success = false, message = "Turno no encontrado" });
                }

                // Verificar que el personal existe
                var personal = await _context.Personal.FindAsync(request.PersonalID);
                if (personal == null)
                {
                    return NotFound(new { success = false, message = "Personal no encontrado" });
                }

                // Verificar si ya existe un registro activo para este turno
                var existente = await _context.RegistrosStockPalets
                    .FirstOrDefaultAsync(r => r.TurnoProduccionID == request.TurnoProduccionID && r.Activo);

                if (existente != null)
                {
                    return BadRequest(new { success = false, message = "Ya existe un registro de stock inicial para este turno" });
                }

                var registro = new RegistroStockPalets
                {
                    TurnoProduccionID = request.TurnoProduccionID,
                    PersonalID = request.PersonalID,
                    StockInicialC32 = request.StockInicialC32,
                    StockInicialF40 = request.StockInicialF40,
                    FechaHoraRegistroInicial = DateTime.Now,
                    ObservacionesInicio = request.ObservacionesInicio,
                    Activo = true
                };

                _context.RegistrosStockPalets.Add(registro);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Stock inicial registrado: ID {registro.RegistroStockPaletsID}");

                var dto = MapToDto(registro, personal);
                return CreatedAtAction(nameof(GetByTurno), new { turnoId = registro.TurnoProduccionID }, new { success = true, data = dto });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al registrar stock inicial");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // PUT: api/stockpalets/{id}/final
        [HttpPut("{id}/final")]
        public async Task<IActionResult> RegistrarStockFinal(int id, UpdateStockFinalDto request)
        {
            try
            {
                var registro = await _context.RegistrosStockPalets
                    .Include(r => r.PersonalRegistro)
                    .FirstOrDefaultAsync(r => r.RegistroStockPaletsID == id);

                if (registro == null)
                {
                    return NotFound(new { success = false, message = "Registro no encontrado" });
                }

                if (registro.StockFinalC32.HasValue && registro.StockFinalF40.HasValue)
                {
                    return BadRequest(new { success = false, message = "El stock final ya fue registrado" });
                }

                registro.StockFinalC32 = request.StockFinalC32;
                registro.StockFinalF40 = request.StockFinalF40;
                registro.FechaHoraRegistroFinal = DateTime.Now;
                registro.ObservacionesFin = request.ObservacionesFin;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Stock final registrado: ID {id}");

                var dto = MapToDto(registro);
                return Ok(new { success = true, data = dto, message = "Stock final registrado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error al registrar stock final para ID {id}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // DELETE: api/stockpalets/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador,Supervisor")]
        public async Task<IActionResult> Delete(int id)
        {
            var registro = await _context.RegistrosStockPalets.FindAsync(id);
            if (registro == null)
            {
                return NotFound(new { success = false, message = "Registro no encontrado" });
            }

            registro.Activo = false;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"✅ Registro de stock eliminado (soft delete): ID {id}");

            return Ok(new { success = true, message = "Registro eliminado exitosamente" });
        }

        // Helper method
        private RegistroStockPaletsDto MapToDto(RegistroStockPalets registro, Personal? personal = null)
        {
            return new RegistroStockPaletsDto
            {
                RegistroStockPaletsID = registro.RegistroStockPaletsID,
                TurnoProduccionID = registro.TurnoProduccionID,
                PersonalID = registro.PersonalID,
                PersonalNombre = personal?.Nombre ?? registro.PersonalRegistro?.Nombre,
                StockInicialC32 = registro.StockInicialC32,
                StockInicialF40 = registro.StockInicialF40,
                StockFinalC32 = registro.StockFinalC32,
                StockFinalF40 = registro.StockFinalF40,
                ProduccionNetaC32 = registro.ProduccionNetaC32,
                ProduccionNetaF40 = registro.ProduccionNetaF40,
                ProduccionTotalNeta = registro.ProduccionTotalNeta,
                FechaHoraRegistroInicial = registro.FechaHoraRegistroInicial,
                FechaHoraRegistroFinal = registro.FechaHoraRegistroFinal,
                ObservacionesInicio = registro.ObservacionesInicio,
                ObservacionesFin = registro.ObservacionesFin
            };
        }
    }
}