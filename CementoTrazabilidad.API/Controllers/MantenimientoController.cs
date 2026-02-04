using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CementoTrazabilidad.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using System.Text.Json;

namespace CementoTrazabilidad.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador")]
public class MantenimientoController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<MantenimientoController> _logger;

    public MantenimientoController(ApplicationDbContext context, ILogger<MantenimientoController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("estadisticas")]
    public async Task<ActionResult> GetEstadisticas()
    {
        try
        {
            var estadisticas = new
            {
                TotalTurnos = await _context.TurnosProduccion.CountAsync(),
                TotalLotes = await _context.LotesProduccion.CountAsync(),
                TotalEventosCarga = await _context.EventosCarga.CountAsync(),
                TotalConsumos = await _context.ConsumoBolsas.CountAsync(),
                TotalParadas = await _context.Paradas.CountAsync(),
                TotalProduccionMaterial = await _context.ProduccionMaterial.CountAsync()
            };

            return Ok(new { success = true, data = estadisticas });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estadísticas");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpGet("exportar-backup")]
    public async Task<ActionResult> ExportarBackup()
    {
        try
        {
            _logger.LogInformation("Iniciando exportación de backup de datos");

            var backup = new
            {
                FechaExportacion = DateTime.Now,
                Turnos = await _context.TurnosProduccion
                    .Include(t => t.PersonalTurno)
                    .ToListAsync(),
                Lotes = await _context.LotesProduccion
                    .Include(l => l.Material)
                    .ToListAsync(),
                EventosCarga = await _context.EventosCarga.ToListAsync(),
                Consumos = await _context.ConsumoBolsas
                    .Include(c => c.ProveedorBolsa)
                    .ToListAsync(),
                Paradas = await _context.Paradas.ToListAsync(),
                ProduccionMaterial = await _context.ProduccionMaterial
                    .Include(p => p.Material)
                    .ToListAsync(),
                Despachos = await _context.Despachos.ToListAsync()
            };

            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
            };

            var jsonBackup = JsonSerializer.Serialize(backup, jsonOptions);
            var bytes = Encoding.UTF8.GetBytes(jsonBackup);

            var fileName = $"Backup_CementoTrazabilidad_{DateTime.Now:yyyyMMdd_HHmmss}.json";

            _logger.LogInformation($"Backup generado exitosamente: {fileName}");

            return File(bytes, "application/json", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al exportar backup");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpDelete("limpiar-turnos")]
    public async Task<ActionResult> LimpiarTurnos()
    {
        try
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _logger.LogWarning("Iniciando limpieza total de base de datos de producción");

                // 1. Eliminar en orden de dependencias
                var registrosDefectos = await _context.RegistrosDefectosBolsas.CountAsync();
                _context.RegistrosDefectosBolsas.RemoveRange(_context.RegistrosDefectosBolsas);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Eliminados {registrosDefectos} registros de defectos");

                var consumos = await _context.ConsumoBolsas.CountAsync();
                _context.ConsumoBolsas.RemoveRange(_context.ConsumoBolsas);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Eliminados {consumos} consumos de bolsas");

                var lotesProveedor = await _context.LotesProveedorBolsa.CountAsync();
                _context.LotesProveedorBolsa.RemoveRange(_context.LotesProveedorBolsa);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Eliminados {lotesProveedor} lotes de proveedor");

                var eventos = await _context.EventosCarga.CountAsync();
                _context.EventosCarga.RemoveRange(_context.EventosCarga);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Eliminados {eventos} eventos de carga");

                var lotes = await _context.LotesProduccion.CountAsync();
                _context.LotesProduccion.RemoveRange(_context.LotesProduccion);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Eliminados {lotes} lotes de producción");

                var produccion = await _context.ProduccionMaterial.CountAsync();
                _context.ProduccionMaterial.RemoveRange(_context.ProduccionMaterial);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Eliminados {produccion} registros de producción");

                var paradas = await _context.Paradas.CountAsync();
                _context.Paradas.RemoveRange(_context.Paradas);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Eliminadas {paradas} paradas");

                var personalTurno = await _context.PersonalTurno.CountAsync();
                _context.PersonalTurno.RemoveRange(_context.PersonalTurno);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Eliminados {personalTurno} asignaciones de personal");

                var despachos = await _context.Despachos.CountAsync();
                _context.Despachos.RemoveRange(_context.Despachos);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Eliminados {despachos} despachos");

                // 2. Finalmente eliminar turnos
                var turnos = await _context.TurnosProduccion.CountAsync();
                _context.TurnosProduccion.RemoveRange(_context.TurnosProduccion);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Eliminados {turnos} turnos");

                // 3. Reiniciar contadores de identidad
                await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('TurnosProduccion', RESEED, 0)");
                await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('LotesProduccion', RESEED, 0)");
                await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('ProduccionMaterial', RESEED, 0)");
                await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Paradas', RESEED, 0)");
                await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('EventosCarga', RESEED, 0)");
                await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('ConsumoBolsas', RESEED, 0)");
                await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Despachos', RESEED, 0)");

                await transaction.CommitAsync();

                _logger.LogWarning("✅ Limpieza de base de datos completada exitosamente");

                return Ok(new
                {
                    success = true,
                    message = "Base de datos limpiada exitosamente",
                    detalles = new
                    {
                        turnosEliminados = turnos,
                        lotesEliminados = lotes,
                        eventosEliminados = eventos,
                        consumosEliminados = consumos,
                        paradasEliminadas = paradas
                    }
                });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al limpiar base de datos");
            return StatusCode(500, new
            {
                success = false,
                message = $"Error al limpiar: {ex.Message}"
            });
        }
    }
}