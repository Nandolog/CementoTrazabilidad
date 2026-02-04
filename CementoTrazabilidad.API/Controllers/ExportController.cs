using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CementoTrazabilidad.API.Services;
using CementoTrazabilidad.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using CementoTrazabilidad.Shared.DTOs;

namespace CementoTrazabilidad.API.Controllers;

[ApiController]
[Route("api/[controller]")]
// ❌ QUITAR [Authorize] del controlador completo
public class ExportController : ControllerBase
{
    private readonly IExcelExportService _excelService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ExportController> _logger;

    public ExportController(IExcelExportService excelService, ApplicationDbContext context, ILogger<ExportController> logger)
    {
        _excelService = excelService;
        _context = context;
        _logger = logger;
    }

    [HttpGet("dashboard/turno/{turnoId}")]
    [AllowAnonymous]  // ✅ Permitir acceso sin autenticación
    public async Task<IActionResult> ExportarDashboardTurno(int turnoId)
    {
        try
        {
            _logger.LogInformation($"📥 Exportando turno {turnoId} a Excel");
            
            // Obtener datos del turno
            var turno = await _context.TurnosProduccion.FindAsync(turnoId);
            if (turno == null)
            {
                _logger.LogWarning($"⚠️ Turno {turnoId} no encontrado");
                return NotFound(new { message = "Turno no encontrado" });
            }

            // Calcular métricas
            var metricas = await CalcularMetricasTurno(turnoId);
            
            // Obtener paradas detalladas
            var paradas = await ObtenerParadasDetalladas(turnoId);

            // Generar Excel
            var excel = _excelService.GenerarReporteTurno(metricas, MapearTurnoDto(turno), paradas);

            var fileName = $"Dashboard_Turno{metricas.TurnoNumero}_{metricas.Fecha:yyyyMMdd}.xlsx";
            
            _logger.LogInformation($"✅ Excel generado: {fileName}");
            
            return File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"❌ Error al exportar turno {turnoId}");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpGet("dashboard/diario/{fecha}")]
    [AllowAnonymous]  // ✅ Permitir acceso sin autenticación
    public async Task<IActionResult> ExportarDashboardDiario(string fecha)
    {
        try
        {
            _logger.LogInformation($"📥 Exportando dashboard diario para {fecha}");
            
            var fechaDate = DateOnly.Parse(fecha);
            
            // Obtener los 3 turnos del día
            var turnos = await _context.TurnosProduccion
                .Where(t => t.Fecha == fechaDate)
                .OrderBy(t => t.TurnoNumero)
                .ToListAsync();

            if (!turnos.Any())
            {
                _logger.LogWarning($"⚠️ No hay turnos para la fecha {fecha}");
                return NotFound(new { message = "No hay turnos para la fecha especificada" });
            }

            // Calcular métricas de cada turno
            var metricasTurnos = new List<MetricasTurnoDto>();
            foreach (var turno in turnos)
            {
                var metricas = await CalcularMetricasTurno(turno.TurnoProduccionID);
                metricasTurnos.Add(metricas);
            }

            // Calcular métricas diarias consolidadas
            var metricasDiarias = CalcularMetricasDiarias(metricasTurnos, fechaDate);

            // Generar Excel
            var turnosDto = turnos.Select(MapearTurnoDto).ToList();
            var excel = _excelService.GenerarReporteDiario(metricasTurnos, turnosDto, metricasDiarias);

            var fileName = $"Dashboard_Diario_{fechaDate:yyyyMMdd}.xlsx";
            
            _logger.LogInformation($"✅ Excel diario generado: {fileName}");
            
            return File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"❌ Error al exportar dashboard diario para fecha {fecha}");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // ============ MÉTODOS AUXILIARES ============
    
    private MetricasDiariasDto CalcularMetricasDiarias(List<MetricasTurnoDto> metricasTurnos, DateOnly fecha)
    {
        var horasMarchaTotales = TimeSpan.FromHours(metricasTurnos.Sum(m => m.HorasMarcha.TotalHours));
        var horasProductivasTotales = TimeSpan.FromHours(metricasTurnos.Sum(m => m.HorasProductivas.TotalHours));
        var toneladasDiarias = metricasTurnos.Sum(m => m.ToneladasProducidas);
        
        var tnPorHoraDiarias = horasProductivasTotales.TotalHours > 0 
            ? toneladasDiarias / (decimal)horasProductivasTotales.TotalHours 
            : 0m;

        var factorCorreccionDiario = horasMarchaTotales.TotalHours > 0
            ? (decimal)(horasProductivasTotales.TotalHours / horasMarchaTotales.TotalHours * 100)
            : 0m;

        var factorProduccionDiario = tnPorHoraDiarias / 80m * 100m;

        return new MetricasDiariasDto
        {
            Fecha = fecha,
            HorasMarchaTotales = horasMarchaTotales,
            HorasProductivasTotales = horasProductivasTotales,
            TotalParadasDiarias = TimeSpan.FromHours(metricasTurnos.Sum(m => m.TotalParadas.TotalHours)),
            ToneladasProducidasDiarias = toneladasDiarias,
            BolsasTotalesDiarias = metricasTurnos.Sum(m => m.BolsasRealizadas),
            PaletsTotalesDiarios = metricasTurnos.Sum(m => m.PaletsRealizados),
            FactorCorreccionDiario = Math.Round(factorCorreccionDiario, 2),
            FactorProduccionDiario = Math.Round(factorProduccionDiario, 2),
            ToneladasPorHoraDiarias = Math.Round(tnPorHoraDiarias, 2)
        };
    }

    private async Task<MetricasTurnoDto> CalcularMetricasTurno(int turnoId)
    {
        var turno = await _context.TurnosProduccion.FindAsync(turnoId);
        if (turno == null) throw new Exception("Turno no encontrado");

        var inicio = turno.FechaHoraInicio;
        var fin = turno.FechaHoraFin ?? DateTime.Now;
        var horasMarcha = fin - inicio;

        var horasTeoricasTurno = turno.TurnoNumero switch
        {
            1 => new TimeSpan(8, 10, 0),
            2 => new TimeSpan(7, 40, 0),
            3 => new TimeSpan(7, 10, 0),
            _ => new TimeSpan(8, 0, 0)
        };

        var paradas = await _context.Paradas
            .Where(p => p.TurnoProduccionID == turnoId)
            .ToListAsync();

        var totalParadas = TimeSpan.FromMinutes(paradas.Sum(p => 
            ((p.FechaHoraFin ?? DateTime.Now) - p.FechaHoraInicio).TotalMinutes));

        var horasProductivas = horasMarcha - totalParadas;

        var paradasMecanicas = paradas
            .Where(p => p.TipoParada != null && p.TipoParada.Contains("Mecanica", StringComparison.OrdinalIgnoreCase))
            .Sum(p => ((p.FechaHoraFin ?? DateTime.Now) - p.FechaHoraInicio).TotalMinutes);

        var paradasElectricas = paradas
            .Where(p => p.TipoParada != null && p.TipoParada.Contains("Electrica", StringComparison.OrdinalIgnoreCase))
            .Sum(p => ((p.FechaHoraFin ?? DateTime.Now) - p.FechaHoraInicio).TotalMinutes);

        var paradasOperativas = paradas
            .Where(p => p.TipoParada != null && p.TipoParada.Contains("Operativa", StringComparison.OrdinalIgnoreCase))
            .Sum(p => ((p.FechaHoraFin ?? DateTime.Now) - p.FechaHoraInicio).TotalMinutes);

        var paradasCircunstanciales = paradas
            .Where(p => p.TipoParada != null && p.TipoParada.Contains("Circunstancial", StringComparison.OrdinalIgnoreCase))
            .Sum(p => ((p.FechaHoraFin ?? DateTime.Now) - p.FechaHoraInicio).TotalMinutes);

        // Obtener producción
        var producciones = await _context.ProduccionMaterial
            .Where(p => p.TurnoProduccionID == turnoId)
            .ToListAsync();

        var bolsasRealizadas = producciones.Sum(p => p.BolsasElaboradas);
        var bolsasRotas = producciones.Sum(p => p.BolsasRotas);
        var bolsasNetas = bolsasRealizadas - bolsasRotas;
        var toneladasProducidas = bolsasNetas * 0.05m; // 50kg = 0.05 toneladas

        var tnPorHora = horasProductivas.TotalHours > 0 
            ? toneladasProducidas / (decimal)horasProductivas.TotalHours 
            : 0m;

        var factorCorreccion = horasMarcha.TotalHours > 0 
            ? (decimal)(horasProductivas.TotalHours / horasMarcha.TotalHours * 100) 
            : 0m;

        var factorProduccion = tnPorHora / 80m * 100m; // Objetivo 80 Tn/h

        // Obtener andenes y palets
        var eventosAndenes = await _context.EventosCarga
            .Where(e => e.TurnoProduccionID == turnoId && e.TipoEvento == "ANDEN")
            .Select(e => e.ZonaCarga)
            .Distinct()
            .CountAsync();

        var eventosPalets = await _context.EventosCarga
            .Where(e => e.TurnoProduccionID == turnoId && e.TipoEvento == "PALET")
            .CountAsync();

        // ✅ CORRECCIÓN: Calcular palets basado en 40 bolsas por palet
        var paletsCalculados = bolsasNetas / 40;  // Cambiar de 50 a 40
        
        // Si hay eventos registrados, usar ese valor; sino usar el calculado
        var paletsFinales = eventosPalets > 0 ? eventosPalets : paletsCalculados;
        
        // ✅ CORRECCIÓN: Si no hay eventos de andenes pero sí producción, asignar valor estimado
        if (eventosAndenes == 0 && bolsasNetas > 0)
        {
            eventosAndenes = Math.Max(1, (int)Math.Ceiling((decimal)bolsasNetas / 1000)); // Estimar 1 anden cada ~1000 bolsas
        }

        return new MetricasTurnoDto
        {
            TurnoProduccionID = turnoId,
            TurnoNumero = turno.TurnoNumero,
            Fecha = turno.Fecha,
            HorasMarcha = horasMarcha,
            HorasProductivas = horasProductivas,
            HorasProductivasObjetivo = horasTeoricasTurno,
            TotalParadas = totalParadas,
            ParadasMecanicas = paradasMecanicas,
            ParadasElectricas = paradasElectricas,
            ParadasOperativas = paradasOperativas,
            ParadasCircunstanciales = paradasCircunstanciales,
            BolsasRealizadas = bolsasRealizadas,
            BolsasRotas = bolsasRotas,
            BolsasNetas = bolsasNetas,
            ToneladasProducidas = toneladasProducidas,
            ToneladasPorHora = tnPorHora,
            ToneladasPorHoraObjetivo = 80m,
            FactorCorreccion = Math.Round(factorCorreccion, 2),
            FactorProduccion = Math.Round(factorProduccion, 2),
            CumplimientoHoras = Math.Round((decimal)(horasProductivas.TotalHours / horasTeoricasTurno.TotalHours * 100), 2),
            CumplimientoProduccion = Math.Round(factorProduccion, 2),
            CantidadAndenes = eventosAndenes,
            PaletsRealizados = paletsFinales,  // Usar el valor corregido
            PaletsObjetivoTurno = 213, // 640/3 ≈ 213
            PaletsObjetivoDiario = 640,
            CumplimientoPalets = paletsFinales > 0 ? Math.Round((decimal)paletsFinales / 213m * 100m, 2) : 0m
        };
    }

    private async Task<List<ParadasDetalladasDto>> ObtenerParadasDetalladas(int turnoId)
    {
        var paradas = await _context.Paradas
            .Where(p => p.TurnoProduccionID == turnoId)
            .ToListAsync();

        return paradas
            .GroupBy(p => p.TipoParada ?? "Sin clasificar")
            .Select(g => new ParadasDetalladasDto
            {
                TipoParada = g.Key,
                CantidadParadas = g.Count(),
                TotalMinutos = g.Sum(p => ((p.FechaHoraFin ?? DateTime.Now) - p.FechaHoraInicio).TotalMinutes),
                TotalHoras = g.Sum(p => ((p.FechaHoraFin ?? DateTime.Now) - p.FechaHoraInicio).TotalHours)
            })
            .OrderByDescending(p => p.TotalMinutos)
            .ToList();
    }

    private TurnoDto MapearTurnoDto(Core.Entidades.TurnoProduccion turno)
    {
        return new TurnoDto
        {
            TurnoProduccionID = turno.TurnoProduccionID,
            Fecha = turno.Fecha,
            TurnoNumero = turno.TurnoNumero,
            Estado = turno.Estado,
            FechaHoraInicio = turno.FechaHoraInicio,
            FechaHoraFin = turno.FechaHoraFin,
            Observaciones = turno.Observaciones
        };
    }
}