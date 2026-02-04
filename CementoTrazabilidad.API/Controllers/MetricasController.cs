using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CementoTrazabilidad.Infrastructure.Data;
using CementoTrazabilidad.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace CementoTrazabilidad.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MetricasController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<MetricasController> _logger;

    // Objetivos hardcoded (FASE 1)
    private const decimal OBJETIVO_TN_POR_HORA = 80m;
    private const int OBJETIVO_PALETS_DIARIO = 640;
    private const double OBJETIVO_HORAS_PRODUCTIVAS = 7.7;

    public MetricasController(ApplicationDbContext context, ILogger<MetricasController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("turno/{turnoId}")]
    public async Task<ActionResult<MetricasTurnoDto>> GetMetricasTurno(int turnoId)
    {
        try
        {
            _logger.LogInformation($"📊 Calculando métricas para turno {turnoId}");

            // 1. Obtener turno
            var turno = await _context.TurnosProduccion.FindAsync(turnoId);
            if (turno == null)
                return NotFound(new { message = "Turno no encontrado" });

            // 2. Calcular horas de marcha
            var inicio = turno.FechaHoraInicio;
            var fin = turno.FechaHoraFin ?? DateTime.Now;
            var horasMarcha = fin - inicio;

            // 3. Obtener horas teóricas según el turno
            var horasTeoricasTurno = turno.TurnoNumero switch
            {
                1 => new TimeSpan(8, 10, 0),  // TM: 8h 10m
                2 => new TimeSpan(7, 40, 0),  // TT: 7h 40m
                3 => new TimeSpan(7, 10, 0),  // TN: 7h 10m
                _ => new TimeSpan(8, 0, 0)
            };

            // 4. Obtener paradas del turno
            var paradas = await _context.Paradas
                .Where(p => p.TurnoProduccionID == turnoId)
                .ToListAsync();

            // 5. Calcular tiempos de paradas por tipo
            var paradasMecanicas = paradas
                .Where(p => p.TipoParada.Contains("Mecanica", StringComparison.OrdinalIgnoreCase))
                .Sum(p => ((p.FechaHoraFin ?? DateTime.Now) - p.FechaHoraInicio).TotalMinutes);

            var paradasElectricas = paradas
                .Where(p => p.TipoParada.Contains("Electrica", StringComparison.OrdinalIgnoreCase))
                .Sum(p => ((p.FechaHoraFin ?? DateTime.Now) - p.FechaHoraInicio).TotalMinutes);

            var paradasOperativas = paradas
                .Where(p => p.TipoParada.Contains("Operativa", StringComparison.OrdinalIgnoreCase))
                .Sum(p => ((p.FechaHoraFin ?? DateTime.Now) - p.FechaHoraInicio).TotalMinutes);

            var paradasCircunstanciales = paradas
                .Where(p => p.TipoParada.Contains("Circunstancial", StringComparison.OrdinalIgnoreCase))
                .Sum(p => ((p.FechaHoraFin ?? DateTime.Now) - p.FechaHoraInicio).TotalMinutes);

            var totalParadas = TimeSpan.FromMinutes(
                paradasMecanicas + paradasElectricas + paradasOperativas + paradasCircunstanciales
            );

            // 6. Obtener eventos de carga para calcular tiempos de actividades
            var eventosCarga = await _context.EventosCarga
                .Where(e => e.TurnoProduccionID == turnoId)
                .OrderBy(e => e.FechaHora)
                .ToListAsync();

            // Calcular tiempo en andenes
            var tiempoAndenes = CalcularTiempoPorZona(eventosCarga, "Anden");
            var tiempoPaletizado = CalcularTiempoPorZona(eventosCarga, "Palet");

            // 7. Obtener producción de lotes
            var lotes = await _context.LotesProduccion
                .Include(l => l.Material)
                .Where(l => l.TurnoID == turnoId)
                .ToListAsync();

            var bolsasRealizadas = lotes.Sum(l => l.CantidadBolsas);

            // 8. Obtener datos de ProduccionMaterial
            var produccionMaterial = await _context.ProduccionMaterial
                .Include(p => p.Material)
                .Where(p => p.TurnoProduccionID == turnoId)
                .ToListAsync();

            var bolsasRotas = produccionMaterial.Sum(p => p.BolsasRotas);
            var bolsasNetas = bolsasRealizadas - bolsasRotas;

            // 9. Calcular toneladas (asumiendo peso promedio de 50kg, ajustar según material real)
            var pesoPromedioBolsa = lotes.Any() 
                ? lotes.Average(l => l.Material?.PesoBolsa ?? 50m) 
                : 50m;
            
            var toneladasProducidas = (bolsasNetas * pesoPromedioBolsa) / 1000m;

            // 10. Calcular horas productivas
            var horasProductivas = horasMarcha - totalParadas;
            var horasProductivasDecimal = (decimal)horasProductivas.TotalHours;

            // 11. Calcular Tn/h
            var tnPorHora = horasProductivasDecimal > 0 
                ? toneladasProducidas / horasProductivasDecimal 
                : 0m;

            // 12. Calcular factores
            var factorCorreccion = horasMarcha.TotalHours > 0 
                ? (decimal)(horasProductivas.TotalHours / horasMarcha.TotalHours * 100) 
                : 0m;

            var factorProduccion = OBJETIVO_TN_POR_HORA > 0 
                ? (tnPorHora / OBJETIVO_TN_POR_HORA * 100) 
                : 0m;

            // 13. Contar andenes utilizados
            var andenesUtilizados = eventosCarga
                .Where(e => e.ZonaCarga.Contains("Anden", StringComparison.OrdinalIgnoreCase))
                .Select(e => e.ZonaCarga)
                .Distinct()
                .Count();
            
            // ✅ MEJORADO: Estimación más realista de andenes si no hay eventos
            if (andenesUtilizados == 0 && bolsasNetas > 0)
            {
                // Estimación conservadora: 1 anden puede cargar entre 800-1500 bolsas
                // Usamos un promedio de 1000 bolsas por anden
                andenesUtilizados = Math.Max(1, (int)Math.Ceiling((decimal)bolsasNetas / 1000m));
                
                // Limitar a un máximo razonable (por ejemplo, 10 andenes)
                andenesUtilizados = Math.Min(andenesUtilizados, 10);
                
                _logger.LogInformation($"📦 Andenes estimados: {andenesUtilizados} (basado en {bolsasNetas} bolsas)");
            }
            
            // ✅ 14. Calcular palets (CORRECCIÓN: 40 bolsas por palet)
            var paletsRealizados = bolsasNetas / 40;  // Cambiar de 500 a 40
            var paletsObjetivoTurno = OBJETIVO_PALETS_DIARIO / 3;
            
            // ✅ Si no hay eventos de palets registrados, usar el cálculo estimado
            var eventosPaletsRegistrados = await _context.EventosCarga
                .Where(e => e.TurnoProduccionID == turnoId && e.TipoEvento == "PALET")
                .CountAsync();
            
            // Si hay eventos registrados, usar ese valor; sino usar el calculado
            if (eventosPaletsRegistrados > 0)
            {
                paletsRealizados = eventosPaletsRegistrados;
            }
            
            // Si no hay andenes de eventos pero sí producción, asignar valor estimado
            if (andenesUtilizados == 0 && bolsasNetas > 0)
            {
                andenesUtilizados = Math.Max(1, (int)Math.Ceiling((decimal)bolsasNetas / 1000)); // Estimar 1 anden cada ~1000 bolsas
            }

            // 15. Construir DTO de respuesta
            var metricas = new MetricasTurnoDto
            {
                TurnoProduccionID = turnoId,
                TurnoNumero = turno.TurnoNumero,
                Fecha = turno.Fecha,
                
                // Tiempos
                HorasMarcha = horasMarcha,
                HorasProductivas = horasProductivas,
                TotalParadas = totalParadas,
                HorasTeoricasTurno = horasTeoricasTurno,
                
                // Paradas clasificadas
                ParadasMecanicas = Math.Round(paradasMecanicas, 2),
                ParadasElectricas = Math.Round(paradasElectricas, 2),
                ParadasOperativas = Math.Round(paradasOperativas, 2),
                ParadasCircunstanciales = Math.Round(paradasCircunstanciales, 2),
                
                // Actividades
                TiempoAndenes = Math.Round(tiempoAndenes, 2),
                TiempoPaletizado = Math.Round(tiempoPaletizado, 2),
                TiempoCambioCamara = 0, // Por ahora 0, se puede agregar
                TiempoStockLleno = 0,
                
                // Producción
                BolsasRealizadas = bolsasRealizadas,
                BolsasRotas = bolsasRotas,
                BolsasNetas = bolsasNetas,
                ToneladasProducidas = Math.Round(toneladasProducidas, 2),
                ToneladasPorHora = Math.Round(tnPorHora, 2),
                
                // Andenes y Palets
                CantidadAndenes = andenesUtilizados > 0 ? andenesUtilizados : 4, // Default 4
                PaletsRealizados = paletsRealizados,
                
                // KPIs
                FactorCorreccion = Math.Round(factorCorreccion, 2),
                FactorProduccion = Math.Round(factorProduccion, 2),
                EficienciaGlobal = Math.Round(factorCorreccion, 2),
                
                // Objetivos
                ToneladasPorHoraObjetivo = OBJETIVO_TN_POR_HORA,
                HorasProductivasObjetivo = TimeSpan.FromHours(OBJETIVO_HORAS_PRODUCTIVAS),
                PaletsObjetivoDiario = OBJETIVO_PALETS_DIARIO,
                PaletsObjetivoTurno = paletsObjetivoTurno,
                
                // Cumplimientos
                CumplimientoProduccion = Math.Round(factorProduccion, 2),
                CumplimientoHoras = Math.Round((decimal)(horasProductivas.TotalHours / OBJETIVO_HORAS_PRODUCTIVAS * 100), 2),
                CumplimientoPalets = paletsObjetivoTurno > 0 
                    ? Math.Round((decimal)paletsRealizados / paletsObjetivoTurno * 100, 2) 
                    : 0m
            };

            _logger.LogInformation($"✅ Métricas calculadas - FC: {metricas.FactorCorreccion}%, FP: {metricas.FactorProduccion}%");

            return Ok(new { success = true, data = metricas });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al calcular métricas del turno {turnoId}");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpGet("paradas-detalladas/{turnoId}")]
    public async Task<ActionResult<List<ParadasDetalladasDto>>> GetParadasDetalladas(int turnoId)
    {
        try
        {
            var paradas = await _context.Paradas
                .Where(p => p.TurnoProduccionID == turnoId)
                .ToListAsync();

            var paradasAgrupadas = paradas
                .GroupBy(p => p.TipoParada)
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
                        Motivo = p.Descripcion ?? ""
                    }).ToList()
                })
                .OrderByDescending(p => p.TotalMinutos)
                .ToList();

            return Ok(new { success = true, data = paradasAgrupadas });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al obtener paradas detalladas del turno {turnoId}");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpGet("distribucion-tiempo/{turnoId}")]
    public async Task<ActionResult<List<DistribucionTiempoDto>>> GetDistribucionTiempo(int turnoId)
    {
        try
        {
            var turno = await _context.TurnosProduccion.FindAsync(turnoId);
            if (turno == null)
                return NotFound();

            var horasMarcha = ((turno.FechaHoraFin ?? DateTime.Now) - turno.FechaHoraInicio).TotalMinutes;

            // Calcular paradas
            var totalParadas = await _context.Paradas
                .Where(p => p.TurnoProduccionID == turnoId)
                .SumAsync(p => EF.Functions.DateDiffMinute(p.FechaHoraInicio, p.FechaHoraFin ?? DateTime.Now));

            // Calcular tiempos de carga
            var eventosCarga = await _context.EventosCarga
                .Where(e => e.TurnoProduccionID == turnoId)
                .OrderBy(e => e.FechaHora)
                .ToListAsync();

            var tiempoAndenes = CalcularTiempoPorZona(eventosCarga, "Anden");
            var tiempoPaletizado = CalcularTiempoPorZona(eventosCarga, "Palet");

            // Tiempo productivo
            var tiempoProductivo = horasMarcha - totalParadas;

            var distribucion = new List<DistribucionTiempoDto>
            {
                new() { 
                    Actividad = "Producción Efectiva", 
                    Minutos = tiempoProductivo,
                    Horas = tiempoProductivo / 60,
                    Porcentaje = (decimal)(tiempoProductivo / horasMarcha * 100),
                    Color = "#28a745"
                },
                new() { 
                    Actividad = "Paradas", 
                    Minutos = totalParadas,
                    Horas = totalParadas / 60,
                    Porcentaje = (decimal)(totalParadas / horasMarcha * 100),
                    Color = "#dc3545"
                },
                new() { 
                    Actividad = "Carga Andenes", 
                    Minutos = tiempoAndenes,
                    Horas = tiempoAndenes / 60,
                    Porcentaje = (decimal)(tiempoAndenes / horasMarcha * 100),
                    Color = "#17a2b8"
                },
                new() { 
                    Actividad = "Paletizado", 
                    Minutos = tiempoPaletizado,
                    Horas = tiempoPaletizado / 60,
                    Porcentaje = (decimal)(tiempoPaletizado / horasMarcha * 100),
                    Color = "#ffc107"
                }
            };

            return Ok(new { success = true, data = distribucion });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al calcular distribución de tiempo del turno {turnoId}");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    // Método helper para calcular tiempo por zona de carga
    private double CalcularTiempoPorZona(List<Core.Entidades.EventoCarga> eventos, string zona)
    {
        var eventosFiltrados = eventos.Where(e => e.ZonaCarga == zona).ToList();
        
        double tiempoTotal = 0;
        DateTime? ultimoInicio = null;

        foreach (var evento in eventosFiltrados.OrderBy(e => e.FechaHora))
        {
            if (evento.TipoEvento == "Inicio")
            {
                ultimoInicio = evento.FechaHora;
            }
            else if (evento.TipoEvento == "Fin" && ultimoInicio.HasValue)
            {
                var duracion = (evento.FechaHora - ultimoInicio.Value).TotalMinutes;
                tiempoTotal += duracion;
                ultimoInicio = null;
            }
        }

        return tiempoTotal;
    }
}