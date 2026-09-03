using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CementoTrazabilidad.Infrastructure.Data;
using CementoTrazabilidad.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using CementoTrazabilidad.Core.Entidades;

namespace CementoTrazabilidad.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MetricasController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<MetricasController> _logger;
    private const double TOLERANCIA_MINUTOS = 1.0;

    // ============================================
    // ✅ OBJETIVOS ACTUALIZADOS
    // ============================================
    private const decimal OBJETIVO_TN_POR_HORA = 80m;
    private const int BOLSAS_POR_PALET = 40;
    private const int BOLSAS_POR_HORA = 1600;

    public MetricasController(ApplicationDbContext context, ILogger<MetricasController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // ============================================
    // 📋 HELPER PARA OBTENER DURACIÓN TEÓRICA
    // ============================================

    private TimeSpan ObtenerDuracionTeorica(int turnoNumero)
    {
        return turnoNumero switch
        {
            1 => TimeSpan.FromHours(8.17),  // 8h 10m
            2 => TimeSpan.FromHours(7.67),  // 7h 40m
            3 => TimeSpan.FromHours(7.50),  // 7h 30m
            _ => TimeSpan.FromHours(8)
        };
    }

    private int ObtenerObjetivoPaletsTurno(int turnoNumero)
    {
        var horas = ObtenerDuracionTeorica(turnoNumero);
        return (int)(horas.TotalHours * BOLSAS_POR_HORA / BOLSAS_POR_PALET);
    }

    private int ObtenerObjetivoBolsasTurno(int turnoNumero)
    {
        var horas = ObtenerDuracionTeorica(turnoNumero);
        return (int)(horas.TotalHours * BOLSAS_POR_HORA);
    }

    // ============================================
    // 📋 MÉTODO PRINCIPAL
    // ============================================

    [HttpGet("turno/{turnoId}")]
    public async Task<ActionResult<MetricasTurnoDto>> GetMetricasTurno(int turnoId)
    {
        try
        {
            _logger.LogInformation("📊 Calculando métricas para turno {turnoId}", turnoId);

            // 1. Obtener turno
            var turno = await _context.TurnosProduccion.FindAsync(turnoId);
            if (turno == null)
                return NotFound(new { message = "Turno no encontrado" });

            // 2. Validar que el turno no esté Programado
            if (turno.Estado == "Programado")
            {
                _logger.LogWarning($"⚠️ Turno {turnoId} está en estado Programado, no se puede calcular métricas");
                return BadRequest(new { success = false, message = "El turno aún no ha sido iniciado" });
            }

            // 3. Calcular duración teórica y objetivos
            var duracionTeorica = ObtenerDuracionTeorica(turno.TurnoNumero);
            var objetivoPaletsTurno = ObtenerObjetivoPaletsTurno(turno.TurnoNumero);
            var objetivoBolsasTurno = ObtenerObjetivoBolsasTurno(turno.TurnoNumero);

            // 4. Calcular horas de marcha
            var inicio = turno.FechaHoraInicio;
            DateTime fin;

            if (turno.Estado == "En Proceso")
            {
                // Turno activo - usar hora actual, limitada al fin programado
                var ahora = DateTime.Now;
                var finProgramado = inicio.Add(duracionTeorica);
                fin = ahora < finProgramado ? ahora : finProgramado;

                _logger.LogInformation($"🔄 Turno {turnoId} activo - Hora actual: {ahora:HH:mm}, " +
                                       $"Fin programado: {finProgramado:HH:mm}, Usando: {fin:HH:mm}");
            }
            else if (turno.FechaHoraFin.HasValue)
            {
                // Turno cerrado - usar fecha de fin real
                fin = turno.FechaHoraFin.Value;
                _logger.LogInformation($"✅ Turno {turnoId} cerrado - Fin real: {fin:HH:mm}");
            }
            else
            {
                // Fallback - usar hora actual
                fin = DateTime.Now;
                _logger.LogWarning($"⚠️ Turno {turnoId} sin estado definido - Usando hora actual");
            }

            var horasMarcha = fin - inicio;

            // Validar que no sea negativo
            if (horasMarcha < TimeSpan.Zero)
            {
                _logger.LogWarning($"⚠️ Horas Marcha negativas ({horasMarcha}). Ajustando a 0");
                horasMarcha = TimeSpan.Zero;
            }

            // Validar que no supere la duración teórica (solo si está activo)
            if (horasMarcha > duracionTeorica && turno.Estado == "En Proceso")
            {
                _logger.LogWarning($"⚠️ Horas Marcha ({horasMarcha.TotalHours:F2}h) excede " +
                                   $"duración teórica ({duracionTeorica.TotalHours:F2}h). Limitando...");
                horasMarcha = duracionTeorica;
            }

            // 5. Obtener paradas
            var paradas = await _context.Paradas
                .Where(p => p.TurnoProduccionID == turnoId)
                .ToListAsync();

            // 5a. Calcular paradas clasificadas
            var paradasMecanicas = paradas
                .Where(p => !string.IsNullOrEmpty(p.TipoParada) && p.TipoParada.Contains("Mecanica", StringComparison.OrdinalIgnoreCase))
                .Sum(p => ((p.FechaHoraFin ?? DateTime.Now) - p.FechaHoraInicio).TotalMinutes);

            var paradasElectricas = paradas
                .Where(p => !string.IsNullOrEmpty(p.TipoParada) && p.TipoParada.Contains("Electrica", StringComparison.OrdinalIgnoreCase))
                .Sum(p => ((p.FechaHoraFin ?? DateTime.Now) - p.FechaHoraInicio).TotalMinutes);

            var paradasOperativas = paradas
                .Where(p => !string.IsNullOrEmpty(p.TipoParada) && p.TipoParada.Contains("Operativa", StringComparison.OrdinalIgnoreCase))
                .Sum(p => ((p.FechaHoraFin ?? DateTime.Now) - p.FechaHoraInicio).TotalMinutes);

            var paradasCircunstanciales = paradas
                .Where(p => !string.IsNullOrEmpty(p.TipoParada) && p.TipoParada.Contains("Circunstancial", StringComparison.OrdinalIgnoreCase))
                .Sum(p => ((p.FechaHoraFin ?? DateTime.Now) - p.FechaHoraInicio).TotalMinutes);

            // 5b. Calcular Stock Lleno (para mostrar por separado)
            var paradasStockLleno = paradas
                .Where(p => !string.IsNullOrEmpty(p.TipoParada) && p.TipoParada.Contains("Stock Lleno", StringComparison.OrdinalIgnoreCase))
                .Sum(p => ((p.FechaHoraFin ?? DateTime.Now) - p.FechaHoraInicio).TotalMinutes);

            // 5c. Total de paradas (todas)
            var totalParadas = TimeSpan.FromMinutes(paradasMecanicas + paradasElectricas + paradasOperativas + paradasCircunstanciales + paradasStockLleno);
            var totalParadasMinutes = totalParadas.TotalMinutes;

            // 6. Eventos de carga
            var eventosCarga = await _context.EventosCarga
                .Where(e => e.TurnoProduccionID == turnoId)
                .OrderBy(e => e.FechaHora)
                .ToListAsync();

            // 7. Calcular tiempos por zona
            var tiempoAndenes = CalcularTiempoPorZona(eventosCarga, "Anden");
            var tiempoPaletizado = CalcularTiempoPorZona(eventosCarga, "Palet");
            var tiempoCambioCamion = CalcularTiempoCambioCamionDesdeEventos(eventosCarga, "Anden");

            // 8. Horas productivas = HorasMarcha - Todas las paradas
            var horasProductivasMinutes = horasMarcha.TotalMinutes - totalParadasMinutes;
            if (horasProductivasMinutes < 0) horasProductivasMinutes = 0;
            var horasProductivas = TimeSpan.FromMinutes(horasProductivasMinutes);

            // 9. Producción
            var bolsasRealizadas = await _context.LotesProduccion
                .Where(l => l.TurnoID == turnoId)
                .SumAsync(l => (int?)l.CantidadBolsas) ?? 0;

            var bolsasRotas = await _context.LotesProduccion
                .Where(l => l.TurnoID == turnoId)
                .SumAsync(l => (int?)l.BolsasRotas) ?? 0;

            var bolsasNetas = bolsasRealizadas - bolsasRotas;

            var pesoPromedioBolsa = await _context.LotesProduccion
                .Where(l => l.TurnoID == turnoId)
                .Include(l => l.Material)
                .Select(l => (decimal?)l.Material.PesoBolsa)
                .AverageAsync() ?? 50m;

            var toneladasProducidas = (bolsasNetas * pesoPromedioBolsa) / 1000m;

            // 10. Tn/h
            var horasProductivasDecimal = (decimal)horasProductivas.TotalHours;
            var tnPorHora = horasProductivasDecimal > 0
                ? toneladasProducidas / horasProductivasDecimal
                : 0m;

            // 11. KPI: Confiabilidad = (HorasMarcha - Paradas) / HorasMarcha * 100
            var factorConfiabilidad = horasMarcha.TotalMinutes > 0
                ? (decimal)((horasMarcha.TotalMinutes - totalParadasMinutes) / horasMarcha.TotalMinutes * 100.0)
                : 0m;

            // 12. Productividad = Tn/h real / Tn/h objetivo * 100
            var factorProduccion = OBJETIVO_TN_POR_HORA > 0
                ? (tnPorHora / OBJETIVO_TN_POR_HORA * 100m)
                : 0m;

            // 13. Eficiencia Global = Confiabilidad * Productividad / 100
            var eficienciaGlobal = Math.Round(factorConfiabilidad * factorProduccion / 100m, 2);

            // 14. Cumplimiento de horas
            var cumplimientoHoras = duracionTeorica.TotalHours > 0
                ? Math.Round((decimal)(horasProductivas.TotalHours / duracionTeorica.TotalHours * 100.0), 2)
                : 0m;

            // 15. Palets
            var paletsRealizados = (int)(bolsasNetas / BOLSAS_POR_PALET);

            // 16. Cantidad de andenes desde eventos
            var cantidadAndenes = eventosCarga
                .Where(e => !string.IsNullOrEmpty(e.ZonaCarga)
                            && e.ZonaCarga.IndexOf("Anden", StringComparison.OrdinalIgnoreCase) >= 0
                            && string.Equals(e.TipoEvento, "Inicio", StringComparison.OrdinalIgnoreCase))
                .Select(e => e.ZonaCarga.ToLowerInvariant())
                .Distinct()
                .Count();

            // 17. Objetivos diarios
            var objetivoPaletsTurno1 = ObtenerObjetivoPaletsTurno(1);
            var objetivoPaletsTurno2 = ObtenerObjetivoPaletsTurno(2);
            var objetivoPaletsTurno3 = ObtenerObjetivoPaletsTurno(3);
            var paletsObjetivoDiario = objetivoPaletsTurno1 + objetivoPaletsTurno2 + objetivoPaletsTurno3;

            // 18. Construir DTO
            var metricas = new MetricasTurnoDto
            {
                TurnoProduccionID = turnoId,
                TurnoNumero = turno.TurnoNumero,
                Fecha = turno.Fecha,

                HorasMarcha = horasMarcha,
                HorasProductivas = horasProductivas,
                TotalParadas = totalParadas,
                HorasTeoricasTurno = duracionTeorica,

                ParadasMecanicas = Math.Round(paradasMecanicas, 2),
                ParadasElectricas = Math.Round(paradasElectricas, 2),
                ParadasOperativas = Math.Round(paradasOperativas, 2),
                ParadasCircunstanciales = Math.Round(paradasCircunstanciales, 2),

                TiempoAndenes = Math.Round(tiempoAndenes, 2),
                TiempoPaletizado = Math.Round(tiempoPaletizado, 2),
                TiempoCambioCamion = Math.Round(tiempoCambioCamion, 2),
                TiempoStockLleno = Math.Round(paradasStockLleno, 2),

                BolsasRealizadas = bolsasRealizadas,
                BolsasRotas = bolsasRotas,
                BolsasNetas = bolsasNetas,
                ToneladasProducidas = Math.Round(toneladasProducidas, 2),
                ToneladasPorHora = Math.Round(tnPorHora, 2),

                CantidadAndenes = cantidadAndenes,
                PaletsRealizados = paletsRealizados,

                FactorConfiabilidad = Math.Round(factorConfiabilidad, 2),
                FactorProduccion = Math.Round(factorProduccion, 2),
                EficienciaGlobal = Math.Round(eficienciaGlobal, 2),

                ToneladasPorHoraObjetivo = OBJETIVO_TN_POR_HORA,
                HorasProductivasObjetivo = duracionTeorica,
                PaletsObjetivoDiario = paletsObjetivoDiario,
                PaletsObjetivoTurno = objetivoPaletsTurno,

                CumplimientoProduccion = Math.Round(factorProduccion, 2),
                CumplimientoHoras = cumplimientoHoras,
                CumplimientoPalets = objetivoPaletsTurno > 0
                    ? Math.Round((decimal)paletsRealizados / objetivoPaletsTurno * 100, 2)
                    : 0m
            };

            // 19. Respuesta con validaciones
            return Ok(new
            {
                success = true,
                data = metricas,
                validaciones = new
                {
                    HorasMarchaMinutes = Math.Round(horasMarcha.TotalMinutes, 2),
                    ParadasMinutes = Math.Round(totalParadasMinutes, 2),
                    TiempoCambioCamion = Math.Round(tiempoCambioCamion, 2),
                    TiempoStockLleno = Math.Round(paradasStockLleno, 2),
                    HorasProductivasMinutes = Math.Round(horasProductivas.TotalMinutes, 2),
                    SumaParciales = Math.Round(horasMarcha.TotalMinutes - horasProductivas.TotalMinutes, 2),
                    DuracionTeoricaHoras = Math.Round(duracionTeorica.TotalHours, 2),
                    EstadoTurno = turno.Estado,
                    ObjetivoPaletsTurno = objetivoPaletsTurno,
                    ObjetivoBolsasTurno = objetivoBolsasTurno
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al calcular métricas del turno {turnoId}", turnoId);
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    // ============================================
    // 📋 HELPERS
    // ============================================

    private double CalcularTiempoPorZona(IEnumerable<EventoCarga> eventos, string zona)
    {
        var eventosFiltrados = eventos
            .Where(e => string.Equals(e.ZonaCarga, zona, StringComparison.OrdinalIgnoreCase))
            .OrderBy(e => e.FechaHora)
            .ToList();

        double tiempoTotal = 0;
        DateTime? ultimoInicio = null;

        foreach (var evento in eventosFiltrados)
        {
            if (string.Equals(evento.TipoEvento, "Inicio", StringComparison.OrdinalIgnoreCase))
            {
                if (ultimoInicio.HasValue)
                {
                    tiempoTotal += (evento.FechaHora - ultimoInicio.Value).TotalMinutes;
                }
                ultimoInicio = evento.FechaHora;
            }
            else if (string.Equals(evento.TipoEvento, "Fin", StringComparison.OrdinalIgnoreCase) && ultimoInicio.HasValue)
            {
                tiempoTotal += (evento.FechaHora - ultimoInicio.Value).TotalMinutes;
                ultimoInicio = null;
            }
        }

        if (ultimoInicio.HasValue)
            tiempoTotal += (DateTime.Now - ultimoInicio.Value).TotalMinutes;

        return tiempoTotal;
    }

    private double CalcularTiempoCambioCamionDesdeEventos(IEnumerable<EventoCarga> eventos, string zona)
    {
        var andenes = eventos
            .Where(e => string.Equals(e.ZonaCarga, zona, StringComparison.OrdinalIgnoreCase))
            .OrderBy(e => e.FechaHora)
            .ToList();

        double tiempoTotal = 0;
        for (int i = 0; i + 1 < andenes.Count; i++)
        {
            var actual = andenes[i];
            var siguiente = andenes[i + 1];

            if (string.Equals(actual.TipoEvento, "Fin", StringComparison.OrdinalIgnoreCase)
                && string.Equals(siguiente.TipoEvento, "Inicio", StringComparison.OrdinalIgnoreCase))
            {
                tiempoTotal += (siguiente.FechaHora - actual.FechaHora).TotalMinutes;
            }
        }

        return tiempoTotal;
    }
}