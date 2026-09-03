using CementoTrazabilidad.API.Services;
using CementoTrazabilidad.Infrastructure.Data;
using CementoTrazabilidad.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;

namespace CementoTrazabilidad.API.Controllers;

[ApiController]
[Route("api/[controller]")]
// ❌ QUITAR [Authorize] del controlador completo
public class ExportController : ControllerBase
{
    private readonly IExcelExportService _excelService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ExportController> _logger;
     

    public ExportController(
        IExcelExportService excelService,
        ApplicationDbContext context,
        ILogger<ExportController> logger)   
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

            // Obtener consumos de bolsas y mapear a DTO
            var consumosEntities = await _context.ConsumoBolsas
                .Include(c => c.ProveedorBolsa)
                .Include(c => c.ProduccionMaterial)
                .ThenInclude(pm => pm.Material)
                .Where(c => c.TurnoProduccionID == turnoId)
                .ToListAsync();

            var consumos = consumosEntities.Select(c => new ConsumoBolsasDTO
            {
                ConsumoBolsasID = c.ConsumoBolsasID,
                ProveedorBolsaID = c.ProveedorBolsaID,
                ProveedorNombre = c.ProveedorBolsa?.Nombre,
                TurnoProduccionID = c.TurnoProduccionID,
                ProduccionMaterialID = c.ProduccionMaterialID,
                MaterialNombre = c.ProduccionMaterial?.Material?.Nombre ?? "Sin material",
                CantidadBolsas = c.CantidadBolsas,
                BolsasDefectuosas = c.BolsasDefectuosas,
                FechaConsumo = c.FechaConsumo,
                LoteBolsa = c.LoteBolsa,
                TipoCemento = c.TipoCemento,
                Observaciones = c.Observaciones
            }).ToList();

            // Generar Excel
            // ✅ Obtener personal del turno
            var personalTurno = await _context.PersonalTurno
                .Include(pt => pt.Personal)
                .Where(pt => pt.TurnoProduccionID == turnoId)
                .Select(pt => new PersonalTurnoDto
                {
                    PersonalTurnoID = pt.PersonalTurnoID,
                    TurnoProduccionID = pt.TurnoProduccionID,
                    PersonalID = pt.PersonalID,
                    RolTurno = pt.RolTurno ?? "Operario",
                    PersonalNombre = pt.Personal != null ? pt.Personal.Nombre : "Sin nombre",
                    PersonalLegajo = pt.Personal != null ? pt.Personal.Legajo : "N/A",
                    RolPersonal = pt.Personal != null ? pt.Personal.Rol : "N/A",
                    Activo = pt.Personal != null ? pt.Personal.Activo : false
                })
                .ToListAsync();
            // ✅ Obtener stock de palets del turno
            // ✅ Obtener stock de palets del turno directamente desde la base de datos
            RegistroStockPaletsDto? stockPalets = null;
            try
            {
                var stockEntity = await _context.RegistrosStockPalets
                    .FirstOrDefaultAsync(s => s.TurnoProduccionID == turnoId);

                if (stockEntity != null)
                {
                    stockPalets = new RegistroStockPaletsDto
                    {
                        RegistroStockPaletsID = stockEntity.RegistroStockPaletsID,
                        TurnoProduccionID = stockEntity.TurnoProduccionID,
                        StockInicialC32 = stockEntity.StockInicialC32,
                        StockInicialF40 = stockEntity.StockInicialF40,
                        StockFinalC32 = stockEntity.StockFinalC32,
                        StockFinalF40 = stockEntity.StockFinalF40,
                        FechaHoraRegistroInicial = stockEntity.FechaHoraRegistroInicial,
                        FechaHoraRegistroFinal = stockEntity.FechaHoraRegistroFinal,
                        ObservacionesInicio = stockEntity.ObservacionesInicio,
                        ObservacionesFin = stockEntity.ObservacionesFin
                    };
                    _logger.LogInformation($"✅ Stock de palets encontrado para turno {turnoId}");
                }
                else
                {
                    _logger.LogInformation($"ℹ️ No hay stock registrado para turno {turnoId}");
                }
            }
            catch (Exception exStock)
            {
                _logger.LogWarning($"⚠️ Error al obtener stock de palets: {exStock.Message}");
                stockPalets = null;
            }

            // Generar Excel con el personal
            var excel = _excelService.GenerarReporteTurno(metricas, MapearTurnoDto(turno), paradas, consumos, personalTurno, stockPalets);

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

    // ✅ AGREGAR este nuevo endpoint al final del archivo ExportController.cs

    [HttpGet("dashboard/mensual/{año}/{mes}")]
    [AllowAnonymous]
    public async Task<IActionResult> ExportarDashboardMensual(int año, int mes)
    {
        try
        {
            _logger.LogInformation($"📥 Exportando dashboard mensual para {año}-{mes:D2}");
            
            // Obtener primer y último día del mes
            var primerDia = new DateOnly(año, mes, 1);
            var ultimoDia = primerDia.AddMonths(1).AddDays(-1);
            
            // Obtener TODOS los turnos del mes
            var turnosMes = await _context.TurnosProduccion
                .Where(t => t.Fecha >= primerDia && t.Fecha <= ultimoDia)
                .OrderBy(t => t.Fecha)
                .ThenBy(t => t.TurnoNumero)
                .ToListAsync();

            if (!turnosMes.Any())
            {
                _logger.LogWarning($"⚠️ No hay turnos para {año}-{mes:D2}");
                return NotFound(new { message = $"No hay turnos para {año}-{mes:D2}" });
            }

            // Calcular métricas de cada turno
            var metricasTurnos = new List<MetricasTurnoDto>();
            foreach (var turno in turnosMes)
            {
                var metricas = await CalcularMetricasTurno(turno.TurnoProduccionID);
                metricasTurnos.Add(metricas);
            }

            // Generar Excel mensual
            var excel = _excelService.GenerarReporteMensual(metricasTurnos, turnosMes.Select(MapearTurnoDto).ToList(), año, mes);

            var fileName = $"Reporte_Mensual_{año}_{mes:D2}.xlsx";
            
            _logger.LogInformation($"✅ Excel mensual generado: {fileName}");
            
            return File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"❌ Error al exportar dashboard mensual para {año}-{mes:D2}");
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

        // Se define factorConfiabilidadDiario igual que factorCorreccionDiario
        var factorConfiabilidadDiario = factorCorreccionDiario;

        return new MetricasDiariasDto
        {
            Fecha = fecha,
            HorasMarchaTotales = horasMarchaTotales,
            HorasProductivasTotales = horasProductivasTotales,
            TotalParadasDiarias = TimeSpan.FromHours(metricasTurnos.Sum(m => m.TotalParadas.TotalHours)),
            ToneladasProducidasDiarias = toneladasDiarias,
            BolsasTotalesDiarias = metricasTurnos.Sum(m => m.BolsasRealizadas),
            PaletsTotalesDiarios = metricasTurnos.Sum(m => m.PaletsRealizados),
            FactorConfiabilidadDiario = Math.Round(factorConfiabilidadDiario, 2),
            FactorProduccionDiario = Math.Round(factorProduccionDiario, 2),
            ToneladasPorHoraDiarias = Math.Round(tnPorHoraDiarias, 2)
        };
    }

    private async Task<MetricasTurnoDto> CalcularMetricasTurno(int turnoId)
    {
        var turno = await _context.TurnosProduccion.FindAsync(turnoId);
        if (turno == null) throw new Exception("Turno no encontrado");

        // ✅ CORRECCIÓN 1: Validación de fechas
        var inicio = turno.FechaHoraInicio;
        var fin = turno.FechaHoraFin ?? DateTime.Now;

        // Si la fecha de fin es menor que la de inicio, usar hora actual
        if (fin < inicio)
        {
            fin = DateTime.Now;
            _logger.LogWarning($"⚠️ Turno {turnoId}: FechaHoraFin corregida");
        }

        var horasMarcha = fin - inicio;

        // Si las horas de marcha son negativas, forzar a cero
        if (horasMarcha.TotalHours < 0)
        {
            horasMarcha = TimeSpan.Zero;
            _logger.LogWarning($"⚠️ Turno {turnoId}: Horas de marcha negativas, forzando a 0");
        }

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

        // ============================================
        // ✅ 1. CALCULAR PARADAS EXCLUYENDO STOCK LLENO (PARA FACTORES)
        // ============================================
        var paradasSinStockLleno = paradas
            .Where(p => p.TipoParada == null ||
                        !p.TipoParada.Contains("Stock Lleno", StringComparison.OrdinalIgnoreCase))
            .ToList();

        // ============================================
        // ✅ 2. TOTAL DE PARADAS (INCLUYE STOCK LLENO - SOLO PARA MOSTRAR)
        // ============================================
        var totalParadas = TimeSpan.FromMinutes(paradas.Sum(p =>
            ((p.FechaHoraFin ?? DateTime.Now) - p.FechaHoraInicio).TotalMinutes));

        // ============================================
        // ✅ 3. HORAS PRODUCTIVAS EXCLUYENDO STOCK LLENO (PARA FACTORES)
        // ============================================
        var minutosParadasFactores = paradasSinStockLleno.Sum(p =>
            ((p.FechaHoraFin ?? DateTime.Now) - p.FechaHoraInicio).TotalMinutes);

        var horasProductivas = horasMarcha - TimeSpan.FromMinutes(minutosParadasFactores);

        // Si las horas productivas son negativas, forzar a cero
        if (horasProductivas.TotalHours < 0)
        {
            horasProductivas = TimeSpan.Zero;
            _logger.LogWarning($"⚠️ Turno {turnoId}: Horas productivas negativas, forzando a 0");
        }

        // ============================================
        // ✅ 4. PARADAS CLASIFICADAS (TODAS, INCLUYENDO STOCK LLENO PARA MOSTRAR)
        // ============================================
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

        var paradasStockLleno = paradas
            .Where(p => p.TipoParada != null && p.TipoParada.Contains("Stock Lleno", StringComparison.OrdinalIgnoreCase))
            .Sum(p => ((p.FechaHoraFin ?? DateTime.Now) - p.FechaHoraInicio).TotalMinutes);

        // ============================================
        // ✅ 5. OBTENER PRODUCCIÓN
        // ============================================
        var bolsasRealizadas = await _context.LotesProduccion
            .Where(l => l.TurnoID == turnoId)
            .SumAsync(l => (int?)l.CantidadBolsas) ?? 0;

        var bolsasRotas = await _context.LotesProduccion
            .Where(l => l.TurnoID == turnoId)
            .SumAsync(l => (int?)l.BolsasRotas) ?? 0;

        var bolsasNetas = bolsasRealizadas - bolsasRotas;
        var toneladasProducidas = bolsasNetas * 0.05m; // 50kg = 0.05 toneladas

        // ============================================
        // ✅ 6. CALCULAR Tn/h Y FACTORES EXCLUYENDO STOCK LLENO
        // ============================================
        var tnPorHora = horasProductivas.TotalHours > 0
            ? toneladasProducidas / (decimal)horasProductivas.TotalHours
            : 0m;

        var factorConfiabilidad = horasMarcha.TotalHours > 0
            ? (decimal)(horasProductivas.TotalHours / horasMarcha.TotalHours * 100)
            : 0m;

        var factorProduccion = tnPorHora / 80m * 100m; // Objetivo 80 Tn/h

        // ============================================
        // ✅ 7. ANDENES - Solo desde EventosCarga
        // ============================================
        int cantidadAndenes = 0;

        // Buscar eventos de la zona "Anden" con TipoEvento "Inicio"
        var eventosAndenInicio = await _context.EventosCarga
            .Where(e => e.TurnoProduccionID == turnoId
                        && e.ZonaCarga == "Anden"
                        && e.TipoEvento == "Inicio")
            .ToListAsync();

        if (eventosAndenInicio.Any())
        {
            cantidadAndenes = eventosAndenInicio.Count;
            _logger.LogInformation($"✅ Turno {turnoId}: Andenes desde EventosCarga (inicios) = {cantidadAndenes}");
        }
        else
        {
            // Alternativa: Buscar eventos de Fin si no hay Inicios
            var eventosAndenFin = await _context.EventosCarga
                .Where(e => e.TurnoProduccionID == turnoId
                            && e.ZonaCarga == "Anden"
                            && e.TipoEvento == "Fin")
                .CountAsync();

            if (eventosAndenFin > 0)
            {
                cantidadAndenes = eventosAndenFin;
                _logger.LogInformation($"✅ Turno {turnoId}: Andenes desde EventosCarga (fines) = {cantidadAndenes}");
            }
            else
            {
                // Última alternativa: Contar eventos únicos de la zona Anden
                var eventosAndenUnicos = await _context.EventosCarga
                    .Where(e => e.TurnoProduccionID == turnoId
                                && e.ZonaCarga == "Anden")
                    .Select(e => e.ZonaCarga)
                    .Distinct()
                    .CountAsync();

                if (eventosAndenUnicos > 0)
                {
                    cantidadAndenes = eventosAndenUnicos;
                    _logger.LogInformation($"✅ Turno {turnoId}: Andenes únicos en EventosCarga = {cantidadAndenes}");
                }
            }
        }

        if (cantidadAndenes == 0)
        {
            _logger.LogWarning($"⚠️ Turno {turnoId}: No se encontraron andenes en EventosCarga");
        }

        // ============================================
        // ✅ 8. PALETS
        // ============================================
        var eventosPalets = await _context.EventosCarga
            .Where(e => e.TurnoProduccionID == turnoId && e.TipoEvento == "PALET")
            .CountAsync();

        _logger.LogInformation($"✅ Turno {turnoId}: Palets registrados = {eventosPalets}");

        var paletsCalculados = bolsasNetas / 40;
        var paletsFinales = eventosPalets > 0 ? eventosPalets : paletsCalculados;

        _logger.LogInformation($"📊 Turno {turnoId}: Palets finales = {paletsFinales} (eventos: {eventosPalets}, calculados: {paletsCalculados})");

        // ============================================
        // ✅ 9. RETORNAR MÉTRICAS COMPLETAS
        // ============================================
        return new MetricasTurnoDto
        {
            TurnoProduccionID = turnoId,
            TurnoNumero = turno.TurnoNumero,
            Fecha = turno.Fecha,
            HorasMarcha = horasMarcha,
            HorasProductivas = horasProductivas,          // ✅ Sin Stock Lleno
            HorasProductivasObjetivo = horasTeoricasTurno,
            TotalParadas = totalParadas,                  // ✅ Con Stock Lleno (para mostrar)
            ParadasMecanicas = paradasMecanicas,
            ParadasElectricas = paradasElectricas,
            ParadasOperativas = paradasOperativas,
            ParadasCircunstanciales = paradasCircunstanciales,
            TiempoStockLleno = paradasStockLleno,         // ✅ Solo para mostrar
            BolsasRealizadas = bolsasRealizadas,
            BolsasRotas = bolsasRotas,
            BolsasNetas = bolsasNetas,
            ToneladasProducidas = toneladasProducidas,
            ToneladasPorHora = tnPorHora,                 // ✅ Sin Stock Lleno
            ToneladasPorHoraObjetivo = 80m,
            FactorConfiabilidad = Math.Round(factorConfiabilidad, 2),  // ✅ Sin Stock Lleno
            FactorProduccion = Math.Round(factorProduccion, 2),        // ✅ Sin Stock Lleno
            CumplimientoHoras = Math.Round((decimal)(horasProductivas.TotalHours / horasTeoricasTurno.TotalHours * 100), 2), // ✅ Sin Stock Lleno
            CumplimientoProduccion = Math.Round(factorProduccion, 2),  // ✅ Sin Stock Lleno
            CantidadAndenes = cantidadAndenes,
            PaletsRealizados = paletsFinales,
            PaletsObjetivoTurno = 213,
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
                TotalHoras = g.Sum(p => ((p.FechaHoraFin ?? DateTime.Now) - p.FechaHoraInicio).TotalHours),
                Paradas = g.Select(p => new ParadaIndividualDto
                {
                    ParadaID = p.ParadaID,
                    Inicio = p.FechaHoraInicio,
                    Fin = p.FechaHoraFin,
                    Minutos = ((p.FechaHoraFin ?? DateTime.Now) - p.FechaHoraInicio).TotalMinutes,
                    Descripcion = p.Descripcion ?? "Sin descripción",
                    MotivoFalla = p.MotivoFalla ?? "No especificado",      // ✅ NUEVO
                    AccionCorrectiva = p.AccionCorrectiva ?? "No especificada", // ✅ NUEVO
                    Responsable = p.Responsable ?? "No asignado"           // ✅ NUEVO
                }).ToList()
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