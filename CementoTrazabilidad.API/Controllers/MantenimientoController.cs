using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CementoTrazabilidad.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

namespace CementoTrazabilidad.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador")]
public class MantenimientoController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<MantenimientoController> _logger;
    private readonly IHostEnvironment _env;
    private readonly IConfiguration _config;

    public MantenimientoController(ApplicationDbContext context, ILogger<MantenimientoController> logger, IHostEnvironment env, IConfiguration config)
    {
        _context = context;
        _logger = logger;
        _env = env;
        _config = config;
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
            // Protección: solo permitir en Development a menos que explícitamente se autorice
            if (!_env.IsDevelopment())
            {
                _logger.LogWarning("Intento de limpieza de BD denegado en entorno no-Development");
                return Forbid("Operación no permitida en este entorno");
            }

            _logger.LogWarning("Iniciando limpieza completa de la base de datos (EnsureDeleted)...");

            // 1) Eliminar la base de datos completamente
            await _context.Database.EnsureDeletedAsync();
            _logger.LogInformation("Base de datos eliminada con EnsureDeleted.");

            // 2) Recrear esquema aplicando migraciones
            await _context.Database.MigrateAsync();
            _logger.LogInformation("Migraciones aplicadas correctamente.");

            // 3) Re-sembrar datos iniciales (seed)
            await DbInitializer.SeedAsync(_context, _config);
            _logger.LogInformation("Seed inicial ejecutado.");

            _logger.LogWarning("✅ Limpieza y recreación de la base de datos completadas exitosamente");

            return Ok(new
            {
                success = true,
                message = "Base de datos recreada y seed aplicada"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al limpiar/recrear la base de datos");
            return StatusCode(500, new
            {
                success = false,
                message = "Error al limpiar/recrear la base de datos",
                detalle = ex.Message,
                inner = ex.InnerException?.Message
            });
        }
    }
}