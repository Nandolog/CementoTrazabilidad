using CementoTrazabilidad.Infrastructure.Data;
using CementoTrazabilidad.Infrastructure.Services;
using CementoTrazabilidad.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CementoTrazabilidad.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ControlPesoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ControlPesoService _controlPesoService;
        private readonly ILogger<ControlPesoController> _logger;

        public ControlPesoController(
            ApplicationDbContext context,
            ControlPesoService controlPesoService,
            ILogger<ControlPesoController> logger)
        {
            _context = context;
            _controlPesoService = controlPesoService;
            _logger = logger;
        }

        // ============================================
        // 📋 1. REGISTRAR CONTROL DE PESO
        // ============================================
        [HttpPost]
        [Authorize(Roles = "Administrador,Supervisor,JefeTurno,Operario")]
        public async Task<IActionResult> Registrar([FromBody] CrearControlPesoDto dto)
        {
            try
            {
                if (dto.Detalles == null || dto.Detalles.Count != 8)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Debe registrar los 8 pesos de las boquillas"
                    });
                }

                var turno = await _context.TurnosProduccion.FindAsync(dto.TurnoProduccionID);
                if (turno == null)
                    return NotFound(new { success = false, message = "Turno no encontrado" });

                if (turno.Estado != "En Proceso")
                    return BadRequest(new
                    {
                        success = false,
                        message = "Solo se puede registrar control de peso en turnos en proceso"
                    });

                var control = await _controlPesoService.RegistrarControlPeso(dto, User.Identity?.Name ?? "Sistema");

                var detalles = await _context.ControlesPesoDetalle
                    .Where(d => d.ControlPesoID == control.ControlPesoID)
                    .OrderBy(d => d.NumeroBoquilla)
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = $"Control de peso #{control.NumeroControl} registrado",
                    data = new
                    {
                        controlPesoID = control.ControlPesoID,
                        numeroControl = control.NumeroControl,
                        pesoObjetivo = control.PesoObjetivo,
                        pesoPromedioBalanza = control.PesoPromedioBalanza,
                        diferenciaPromedio = control.DiferenciaPromedio,
                        desviacionEstandar = control.DesviacionEstandar,
                        estadoControl = control.EstadoControl,
                        dentroDeTolerancia = control.DentroDeTolerancia,
                        detalles = detalles.Select(d => new
                        {
                            boquilla = d.NumeroBoquilla,
                            pesoControlador = d.PesoControlador,
                            pesoBalanza = d.PesoBalanza,
                            diferencia = d.Diferencia,
                            ajusteAplicado = d.AjusteAplicado,
                            dentroDeTolerancia = d.DentroDeTolerancia
                        })
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al registrar control de peso");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // ============================================
        // 📋 2. OBTENER CONTROLES POR TURNO
        // ============================================
        [HttpGet("turno/{turnoId}")]
        public async Task<IActionResult> GetPorTurno(int turnoId)
        {
            try
            {
                var controles = await _context.ControlesPeso
                    .Where(c => c.TurnoProduccionID == turnoId)
                    .Include(c => c.Detalles)
                    .OrderBy(c => c.NumeroControl)
                    .ToListAsync();

                var resultado = controles.Select(c => new ControlPesoDto
                {
                    ControlPesoID = c.ControlPesoID,
                    TurnoProduccionID = c.TurnoProduccionID,
                    FechaHora = c.FechaHora,
                    NumeroControl = c.NumeroControl,
                    OperadorResponsable = c.OperadorResponsable,
                    Observacion = c.Observaciones,
                    PesoObjetivo = c.PesoObjetivo,
                    PesoPromedioControlador = c.PesoPromedioControlador,
                    PesoPromedioBalanza = c.PesoPromedioBalanza,
                    DiferenciaPromedio = c.DiferenciaPromedio,
                    DesviacionEstandar = c.DesviacionEstandar,
                    DentroDeTolerancia = c.DentroDeTolerancia,
                    EstadoControl = c.EstadoControl,
                    Detalles = c.Detalles.Select(d => new ControlPesoDetalleDto
                    {
                        NumeroBoquilla = d.NumeroBoquilla,
                        PesoControlador = d.PesoControlador,
                        PesoBalanza = d.PesoBalanza,
                        AjusteAplicado = d.AjusteAplicado,
                        Observacion = d.Observacion
                    }).ToList()
                }).ToList();

                return Ok(new
                {
                    success = true,
                    data = resultado,
                    count = resultado.Count,
                    resumenTurno = new
                    {
                        totalControles = resultado.Count,
                        controlesOK = resultado.Count(c => c.EstadoControl == "OK"),
                        controlesFueraTolerancia = resultado.Count(c => c.EstadoControl == "FUERA_TOLERANCIA"),
                        controlesCriticos = resultado.Count(c => c.EstadoControl == "CRITICO"),
                        desviacionPromedioTurno = resultado.Any()
                            ? Math.Round(resultado.Average(c => c.DesviacionEstandar), 3)
                            : 0
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error al obtener controles del turno {turnoId}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // ============================================
        // 📋 3. ESTADÍSTICAS POR BOQUILLA
        // ============================================
        [HttpGet("estadisticas-boquillas/turno/{turnoId}")]
        public async Task<IActionResult> GetEstadisticasBoquillas(int turnoId)
        {
            try
            {
                var detalles = await _context.ControlesPesoDetalle
                    .Include(d => d.ControlPeso)
                    .Where(d => d.ControlPeso.TurnoProduccionID == turnoId)
                    .ToListAsync();

                var estadisticas = detalles
                    .GroupBy(d => d.NumeroBoquilla)
                    .Select(g => new
                    {
                        Boquilla = g.Key,
                        PesoPromedioBalanza = Math.Round(g.Average(d => d.PesoBalanza), 3),
                        PesoPromedioControlador = Math.Round(g.Average(d => d.PesoControlador), 3),
                        DiferenciaPromedio = Math.Round(g.Average(d => d.Diferencia), 3),
                        DesviacionEstandar = Math.Round(CalcularDesviacion(g.Select(d => d.PesoBalanza).ToList()), 3),
                        CantidadControles = g.Count(),
                        ControlesFueraTolerancia = g.Count(d => !d.DentroDeTolerancia),
                        AjustePromedio = Math.Round(g.Average(d => d.AjusteAplicado), 3)
                    })
                    .OrderBy(e => e.Boquilla)
                    .ToList();

                return Ok(new
                {
                    success = true,
                    data = estadisticas,
                    count = estadisticas.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error al obtener estadísticas de boquillas");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // ============================================
        // 📋 4. DASHBOARD CONTROL DE PESO
        // ============================================
        [HttpGet("dashboard/turno/{turnoId}")]
        public async Task<IActionResult> GetDashboardControlPeso(int turnoId)
        {
            try
            {
                var controles = await _context.ControlesPeso
                    .Where(c => c.TurnoProduccionID == turnoId)
                    .Include(c => c.Detalles)
                    .OrderBy(c => c.NumeroControl)
                    .ToListAsync();

                if (!controles.Any())
                {
                    return Ok(new
                    {
                        success = true,
                        data = new
                        {
                            tieneControles = false,
                            mensaje = "Aún no se han registrado controles de peso en este turno"
                        }
                    });
                }

                var ultimoControl = controles.Last();
                var desviacionPromedio = controles.Average(c => c.DesviacionEstandar);

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        tieneControles = true,
                        totalControles = controles.Count,
                        controlesOK = controles.Count(c => c.EstadoControl == "OK"),
                        controlesFueraTolerancia = controles.Count(c => c.EstadoControl == "FUERA_TOLERANCIA"),
                        controlesCriticos = controles.Count(c => c.EstadoControl == "CRITICO"),

                        ultimoControl = new
                        {
                            numero = ultimoControl.NumeroControl,
                            fechaHora = ultimoControl.FechaHora,
                            pesoPromedioBalanza = ultimoControl.PesoPromedioBalanza,
                            diferenciaPromedio = ultimoControl.DiferenciaPromedio,
                            desviacionEstandar = ultimoControl.DesviacionEstandar,
                            estado = ultimoControl.EstadoControl
                        },

                        tendencia = controles.Select(c => new
                        {
                            numero = c.NumeroControl,
                            pesoPromedio = c.PesoPromedioBalanza,
                            diferencia = c.DiferenciaPromedio,
                            desviacion = c.DesviacionEstandar,
                            estado = c.EstadoControl
                        }),

                        boquillas = controles.SelectMany(c => c.Detalles)
                            .GroupBy(d => d.NumeroBoquilla)
                            .Select(g => new
                            {
                                boquilla = g.Key,
                                pesoPromedio = Math.Round(g.Average(d => d.PesoBalanza), 2),
                                diferenciaPromedio = Math.Round(g.Average(d => d.Diferencia), 2),
                                controlesFueraTolerancia = g.Count(d => !d.DentroDeTolerancia)
                            })
                            .OrderBy(b => b.boquilla)
                            .ToList(),

                        desviacionPromedioTurno = Math.Round(desviacionPromedio, 3),
                        porcentajeAceptacion = Math.Round(
                            (decimal)controles.Count(c => c.EstadoControl == "OK") / controles.Count * 100, 1)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener dashboard de control de peso");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // ============================================
        // 📋 5. EXPORTAR A EXCEL
        // ============================================
        [HttpGet("exportar-excel/turno/{turnoId}")]
        public async Task<IActionResult> ExportarExcel(int turnoId)
        {
            try
            {
                var controles = await _context.ControlesPeso
                    .Where(c => c.TurnoProduccionID == turnoId)
                    .Include(c => c.Detalles)
                    .OrderBy(c => c.NumeroControl)
                    .ToListAsync();

                if (!controles.Any())
                    return NotFound(new { success = false, message = "No hay controles para exportar" });

                using var workbook = new ClosedXML.Excel.XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Control de Peso");

                worksheet.Cell(1, 1).Value = "N° Control";
                worksheet.Cell(1, 2).Value = "Fecha/Hora";
                worksheet.Cell(1, 3).Value = "Operador";
                worksheet.Cell(1, 4).Value = "Boquilla";
                worksheet.Cell(1, 5).Value = "Peso Controlador (kg)";
                worksheet.Cell(1, 6).Value = "Peso Balanza (kg)";
                worksheet.Cell(1, 7).Value = "Diferencia (kg)";
                worksheet.Cell(1, 8).Value = "Ajuste (kg)";
                worksheet.Cell(1, 9).Value = "Dentro Tolerancia";
                worksheet.Cell(1, 10).Value = "Observación";

                var headerRange = worksheet.Range(1, 1, 1, 10);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightBlue;

                int fila = 2;
                foreach (var control in controles)
                {
                    foreach (var detalle in control.Detalles.OrderBy(d => d.NumeroBoquilla))
                    {
                        worksheet.Cell(fila, 1).Value = control.NumeroControl;
                        worksheet.Cell(fila, 2).Value = control.FechaHora.ToString("dd/MM/yyyy HH:mm");
                        worksheet.Cell(fila, 3).Value = control.OperadorResponsable;
                        worksheet.Cell(fila, 4).Value = detalle.NumeroBoquilla;
                        worksheet.Cell(fila, 5).Value = detalle.PesoControlador;
                        worksheet.Cell(fila, 6).Value = detalle.PesoBalanza;
                        worksheet.Cell(fila, 7).Value = detalle.Diferencia;
                        worksheet.Cell(fila, 8).Value = detalle.AjusteAplicado;
                        worksheet.Cell(fila, 9).Value = detalle.DentroDeTolerancia ? "SÍ" : "NO";
                        worksheet.Cell(fila, 10).Value = detalle.Observacion ?? "";

                        if (!detalle.DentroDeTolerancia)
                        {
                            worksheet.Range(fila, 1, fila, 10).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightPink;
                        }

                        fila++;
                    }
                }

                worksheet.Columns().AdjustToContents();

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Seek(0, SeekOrigin.Begin);

                var fileName = $"ControlPeso_Turno{turnoId}_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";

                return File(stream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error al exportar Excel del turno {turnoId}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // ============================================
        // 📋 HELPERS
        // ============================================
        private decimal CalcularDesviacion(List<decimal> valores)
        {
            if (valores.Count <= 1) return 0;
            var promedio = valores.Average();
            var sumaCuadrados = valores.Sum(v => (v - promedio) * (v - promedio));
            return (decimal)Math.Sqrt((double)(sumaCuadrados / (valores.Count - 1)));
        }
    }
}