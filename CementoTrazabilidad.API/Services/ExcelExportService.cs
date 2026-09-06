using ClosedXML.Excel;
using CementoTrazabilidad.Shared.DTOs;
using System.Drawing;

namespace CementoTrazabilidad.API.Services;

public interface IExcelExportService
{
    byte[] GenerarReporteTurno(MetricasTurnoDto metricas, TurnoDto turno, List<ParadasDetalladasDto> paradas, List<ConsumoBolsasDTO> consumos, List<PersonalTurnoDto> personal, RegistroStockPaletsDto stockPalets);
    byte[] GenerarReporteDiario(List<MetricasTurnoDto> metricasTurnos, List<TurnoDto> turnos, MetricasDiariasDto metricasDiarias);
    byte[] GenerarReporteMensual(List<MetricasTurnoDto> metricasTurnos, List<TurnoDto> turnos, int año, int mes);
}

public class ExcelExportService : IExcelExportService
{
    public byte[] GenerarReporteTurno(MetricasTurnoDto metricas, TurnoDto turno, List<ParadasDetalladasDto> paradas, List<ConsumoBolsasDTO> consumos, List<PersonalTurnoDto> personal, RegistroStockPaletsDto stockPalets)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add($"Turno {metricas.TurnoNumero}");

        // ============ ENCABEZADO ============
        var row = 1;
        ws.Cell(row, 1).Value = "REPORTE DE PRODUCCIÓN - TURNO";
        ws.Range(row, 1, row, 6).Merge().Style
            .Font.SetBold().Font.SetFontSize(16)
            .Fill.SetBackgroundColor(XLColor.DarkBlue)
            .Font.SetFontColor(XLColor.White)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        row += 2;
        ws.Cell(row, 1).Value = "Fecha:";
        ws.Cell(row, 2).Value = metricas.Fecha.ToString("dd/MM/yyyy");
        ws.Cell(row, 4).Value = "Turno:";
        ws.Cell(row, 5).Value = $"Turno {metricas.TurnoNumero}";

        row++;
        ws.Cell(row, 1).Value = "Estado:";
        ws.Cell(row, 2).Value = turno.Estado;
        ws.Cell(row, 4).Value = "Horario:";
        ws.Cell(row, 5).Value = ObtenerHorarioTurno(metricas.TurnoNumero);

        // ============ SECCIÓN: PERSONAL DEL TURNO ============
        row += 2;
        ws.Cell(row, 1).Value = "PERSONAL DEL TURNO";
        FormatearEncabezadoSeccion(ws.Range(row, 1, row, 4), XLColor.Purple);

        row++;
        ws.Cell(row, 1).Value = "Nombre";
        ws.Cell(row, 2).Value = "Rol";
        ws.Cell(row, 3).Value = "Legajo";
        ws.Cell(row, 4).Value = "Rol en Turno";
        FormatearEncabezadoTabla(ws.Range(row, 1, row, 4));

        row++;
        if (personal != null && personal.Any())
        {
            foreach (var p in personal)
            {
                ws.Cell(row, 1).Value = p.PersonalNombre ?? "Sin nombre";
                ws.Cell(row, 2).Value = p.RolPersonal ?? "N/A";
                ws.Cell(row, 3).Value = p.PersonalLegajo ?? "N/A";
                ws.Cell(row, 4).Value = p.RolTurno ?? "Operario";
                row++;
            }
        }
        else
        {
            ws.Cell(row, 1).Value = "No hay personal asignado";
            ws.Range(row, 1, row, 4).Merge().Style.Font.SetItalic();
            row++;
        }

        // ============ SECCIÓN: STOCK DE PALETS ============
        row += 2;
        ws.Cell(row, 1).Value = "STOCK DE PALETS";
        FormatearEncabezadoSeccion(ws.Range(row, 1, row, 4), XLColor.Teal);

        row++;
        ws.Cell(row, 1).Value = "Tipo";
        ws.Cell(row, 2).Value = "C32";
        ws.Cell(row, 3).Value = "F40";
        ws.Cell(row, 4).Value = "Total";
        FormatearEncabezadoTabla(ws.Range(row, 1, row, 4));

        row++;
        if (stockPalets != null)
        {
            ws.Cell(row, 1).Value = "Stock Inicial";
            ws.Cell(row, 2).Value = stockPalets.StockInicialC32;
            ws.Cell(row, 3).Value = stockPalets.StockInicialF40;
            ws.Cell(row, 4).Value = stockPalets.StockInicialC32 + stockPalets.StockInicialF40;
            ws.Cell(row, 4).Style.Font.SetBold();
            row++;

            if (stockPalets.StockFinalC32.HasValue && stockPalets.StockFinalF40.HasValue)
            {
                ws.Cell(row, 1).Value = "Stock Final";
                ws.Cell(row, 2).Value = stockPalets.StockFinalC32;
                ws.Cell(row, 3).Value = stockPalets.StockFinalF40;
                ws.Cell(row, 4).Value = stockPalets.StockFinalC32.Value + stockPalets.StockFinalF40.Value;
                ws.Cell(row, 4).Style.Font.SetBold();
                row++;

                ws.Cell(row, 1).Value = "Variación";
                ws.Cell(row, 2).Value = "Pendiente";
                ws.Cell(row, 3).Value = "Pendiente";
                ws.Cell(row, 4).Value = "Pendiente";
                ws.Range(row, 1, row, 4).Style.Fill.SetBackgroundColor(XLColor.LightYellow);
                row++;
            }
            else
            {
                ws.Cell(row, 1).Value = "Stock Final";
                ws.Cell(row, 2).Value = "Pendiente";
                ws.Cell(row, 3).Value = "Pendiente";
                ws.Cell(row, 4).Value = "Pendiente";
                ws.Range(row, 1, row, 4).Style.Fill.SetBackgroundColor(XLColor.LightYellow);
                row++;
            }
        }
        else
        {
            ws.Cell(row, 1).Value = "No hay stock registrado";
            ws.Range(row, 1, row, 4).Merge().Style.Font.SetItalic();
            row++;
        }

        // ============ SECCIÓN: TIEMPOS ============
        row += 2;
        ws.Cell(row, 1).Value = "TIEMPOS DEL TURNO";
        FormatearEncabezadoSeccion(ws.Range(row, 1, row, 4), XLColor.DarkGreen);

        row++;
        ws.Cell(row, 1).Value = "Concepto";
        ws.Cell(row, 2).Value = "Tiempo (HH:MM)";
        ws.Cell(row, 3).Value = "Horas";
        FormatearEncabezadoTabla(ws.Range(row, 1, row, 3));

        row++;
        var dataTiempos = new[]
        {
            new { Concepto = "Horas de Marcha", Tiempo = FormatearHoras(metricas.HorasMarcha), Horas = metricas.HorasMarcha.TotalHours },
            new { Concepto = "Horas Productivas", Tiempo = FormatearHoras(metricas.HorasProductivas), Horas = metricas.HorasProductivas.TotalHours },
            new { Concepto = "Objetivo Horas", Tiempo = FormatearHoras(metricas.HorasProductivasObjetivo), Horas = metricas.HorasProductivasObjetivo.TotalHours },
            new { Concepto = "Total Paradas", Tiempo = FormatearHoras(metricas.TotalParadas), Horas = metricas.TotalParadas.TotalHours }
        };

        foreach (var item in dataTiempos)
        {
            ws.Cell(row, 1).Value = item.Concepto;
            ws.Cell(row, 2).Value = item.Tiempo;
            ws.Cell(row, 3).Value = item.Horas;
            ws.Cell(row, 3).Style.NumberFormat.Format = "0.00";
            row++;
        }

        // ============ SECCIÓN: KPIs ============
        row++;
        ws.Cell(row, 1).Value = "INDICADORES (KPIs)";
        FormatearEncabezadoSeccion(ws.Range(row, 1, row, 4), XLColor.DarkBlue);

        row++;
        ws.Cell(row, 1).Value = "Indicador";
        ws.Cell(row, 2).Value = "Valor";
        ws.Cell(row, 3).Value = "Objetivo";
        ws.Cell(row, 4).Value = "Cumplimiento";
        FormatearEncabezadoTabla(ws.Range(row, 1, row, 4));

        row++;
        var dataKPIs = new[]
        {
            new { Indicador = "Factor de Confiabilidad (FC)", Valor = $"{metricas.FactorConfiabilidad:N2}%", Objetivo = "≥90%", Cumplimiento = metricas.FactorConfiabilidad },
            new { Indicador = "Factor de Producción (FP)", Valor = $"{metricas.FactorProduccion:N2}%", Objetivo = "≥90%", Cumplimiento = metricas.FactorProduccion },
            new { Indicador = "Toneladas/Hora", Valor = $"{metricas.ToneladasPorHora:N2}", Objetivo = "80.00", Cumplimiento = metricas.CumplimientoProduccion },
            new { Indicador = "Horas Productivas", Valor = $"{metricas.HorasProductivas.TotalHours:N2}h", Objetivo = metricas.HorasProductivasObjetivo.TotalHours.ToString("N2") + "h", Cumplimiento = metricas.CumplimientoHoras }
        };

        foreach (var kpi in dataKPIs)
        {
            ws.Cell(row, 1).Value = kpi.Indicador;
            ws.Cell(row, 2).Value = kpi.Valor;
            ws.Cell(row, 3).Value = kpi.Objetivo;
            ws.Cell(row, 4).Value = $"{kpi.Cumplimiento:N2}%";

            var color = kpi.Cumplimiento >= 90 ? XLColor.Green : kpi.Cumplimiento >= 70 ? XLColor.Orange : XLColor.Red;
            ws.Cell(row, 4).Style.Fill.SetBackgroundColor(color).Font.SetFontColor(XLColor.White).Font.SetBold();

            row++;
        }

        // ============ SECCIÓN: PRODUCCIÓN ============
        row += 2;
        ws.Cell(row, 1).Value = "PRODUCCIÓN";
        ws.Range(row, 1, row, 3).Merge();
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 1).Style.Font.FontColor = XLColor.White;
        ws.Range(row, 1, row, 3).Style.Fill.BackgroundColor = XLColor.DarkGreen;

        row++;
        ws.Cell(row, 1).Value = "Bolsas Realizadas:";
        ws.Cell(row, 2).Value = metricas.BolsasRealizadas;
        ws.Cell(row, 2).Style.NumberFormat.Format = "#,##0";
        row++;

        ws.Cell(row, 1).Value = "Bolsas Rotas:";
        ws.Cell(row, 2).Value = metricas.BolsasRotas;
        ws.Cell(row, 2).Style.NumberFormat.Format = "#,##0";
        row++;

        ws.Cell(row, 1).Value = "Bolsas Netas:";
        ws.Cell(row, 2).Value = metricas.BolsasNetas;
        ws.Cell(row, 2).Style.NumberFormat.Format = "#,##0";
        row++;

        ws.Cell(row, 1).Value = "Toneladas Producidas:";
        ws.Cell(row, 2).Value = metricas.ToneladasProducidas;
        ws.Cell(row, 2).Style.NumberFormat.Format = "#,##0.00";
        row++;

        if (metricas.CantidadAndenes > 0 || metricas.BolsasEnAnden > 0)
        {
            // Cantidad de Andenes
            ws.Cell(row, 1).Value = "Cantidad de Andenes:";
            ws.Cell(row, 2).Value = metricas.CantidadAndenes;
            ws.Cell(row, 2).Style.NumberFormat.Format = "#,##0";
            ws.Range(row, 1, row, 2).Style.Fill.SetBackgroundColor(XLColor.LightYellow);
            row++;

            // Total Bolsas en Andén
            ws.Cell(row, 1).Value = "Total Bolsas en Andén:";
            ws.Cell(row, 2).Value = metricas.BolsasEnAnden;
            ws.Cell(row, 2).Style.NumberFormat.Format = "#,##0";
            ws.Range(row, 1, row, 2).Style.Fill.SetBackgroundColor(XLColor.LightYellow);
            row++;

            // Opcional: Promedio de bolsas por andén
            if (metricas.CantidadAndenes > 0 && metricas.BolsasEnAnden > 0)
            {
                var promedioPorAnden = metricas.BolsasEnAnden / metricas.CantidadAndenes;
                ws.Cell(row, 1).Value = "Promedio por Andén:";
                ws.Cell(row, 2).Value = promedioPorAnden;
                ws.Cell(row, 2).Style.NumberFormat.Format = "#,##0";
                ws.Range(row, 1, row, 2).Style.Fill.SetBackgroundColor(XLColor.LightCyan);
                row++;
            }
        }

        // ============ SECCIÓN: PARADAS CLASIFICADAS ============
        row += 2;
        ws.Cell(row, 1).Value = "PARADAS CLASIFICADAS";
        ws.Range(row, 1, row, 4).Merge();
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 1).Style.Font.FontColor = XLColor.White;
        ws.Range(row, 1, row, 4).Style.Fill.BackgroundColor = XLColor.DarkRed;

        row++;
        ws.Cell(row, 1).Value = "Tipo de Parada";
        ws.Cell(row, 2).Value = "Tiempo (HH:MM)";
        ws.Cell(row, 3).Value = "Horas";
        ws.Cell(row, 4).Value = "Minutos";
        ws.Range(row, 1, row, 4).Style.Font.Bold = true;
        ws.Range(row, 1, row, 4).Style.Fill.BackgroundColor = XLColor.LightGray;

        row++;
        var dataParadasClasificadas = new[]
        {
            new { Tipo = "MECÁNICAS", Minutos = metricas.ParadasMecanicas },
            new { Tipo = "ELÉCTRICAS", Minutos = metricas.ParadasElectricas },
            new { Tipo = "OPERATIVAS", Minutos = metricas.ParadasOperativas },
            new { Tipo = "CIRCUNSTANCIALES", Minutos = metricas.ParadasCircunstanciales },
            new { Tipo = "STOCK LLENO", Minutos = metricas.TiempoStockLleno }
        };

        foreach (var parada in dataParadasClasificadas)
        {
            ws.Cell(row, 1).Value = parada.Tipo;
            ws.Cell(row, 2).Value = FormatearMinutosHHMM(parada.Minutos);
            ws.Cell(row, 3).Value = parada.Minutos / 60.0;
            ws.Cell(row, 3).Style.NumberFormat.Format = "0.00";
            ws.Cell(row, 4).Value = parada.Minutos;
            ws.Cell(row, 4).Style.NumberFormat.Format = "0";
            row++;
        }

        ws.Cell(row, 1).Value = "TOTAL PARADAS";
        ws.Cell(row, 2).Value = FormatearHoras(metricas.TotalParadas);
        ws.Cell(row, 3).Value = metricas.TotalParadas.TotalHours;
        ws.Cell(row, 3).Style.NumberFormat.Format = "0.00";
        ws.Cell(row, 4).Value = metricas.TotalParadas.TotalMinutes;
        ws.Range(row, 1, row, 4).Style.Font.Bold = true;
        ws.Range(row, 1, row, 4).Style.Fill.BackgroundColor = XLColor.LightGray;

        // ============ SECCIÓN: DETALLE DE PARADAS ============
        row += 2;
        ws.Cell(row, 1).Value = "DETALLE DE PARADAS";
        ws.Range(row, 1, row, 7).Merge();
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 1).Style.Font.FontColor = XLColor.White;
        ws.Range(row, 1, row, 7).Style.Fill.BackgroundColor = XLColor.DarkBlue;
        ws.Cell(row, 1).Style.Font.SetFontSize(14);
        row++;

        bool hayDetalles = paradas != null && paradas.Any(p => p.Paradas != null && p.Paradas.Any());

        if (hayDetalles)
        {
            var paradasOrdenadas = paradas
                .SelectMany(p => p.Paradas.Select(d => new { Tipo = p.TipoParada, Detalle = d }))
                .OrderByDescending(x => x.Detalle.Inicio)
                .ToList();

            int cantidadParadas = paradasOrdenadas.Count;

            // ✅ Si hay 1 parada, usar formato vertical (legible y detallado)
            if (cantidadParadas == 1)
            {
                var item = paradasOrdenadas.First();

                ws.Cell(row, 1).Value = "Tipo:";
                ws.Cell(row, 1).Style.Font.Bold = true;
                ws.Cell(row, 2).Value = item.Tipo;
                row++;

                ws.Cell(row, 1).Value = "Descripción:";
                ws.Cell(row, 1).Style.Font.Bold = true;
                ws.Cell(row, 2).Value = item.Detalle.Descripcion ?? "Sin descripción";
                row++;

                ws.Cell(row, 1).Value = "Inicio:";
                ws.Cell(row, 1).Style.Font.Bold = true;
                ws.Cell(row, 2).Value = item.Detalle.Inicio.ToString("dd/MM HH:mm");
                row++;

                ws.Cell(row, 1).Value = "Fin:";
                ws.Cell(row, 1).Style.Font.Bold = true;
                ws.Cell(row, 2).Value = item.Detalle.Fin?.ToString("dd/MM HH:mm") ?? "En curso";
                row++;

                ws.Cell(row, 1).Value = "Dur.(min):";
                ws.Cell(row, 1).Style.Font.Bold = true;
                ws.Cell(row, 2).Value = Math.Round(item.Detalle.Minutos, 0);
                ws.Cell(row, 2).Style.NumberFormat.Format = "0";
                row++;

                ws.Cell(row, 1).Value = "Motivo:";
                ws.Cell(row, 1).Style.Font.Bold = true;
                ws.Cell(row, 2).Value = item.Detalle.MotivoFalla ?? "No especificado";
                row++;

                ws.Cell(row, 1).Value = "Acción:";
                ws.Cell(row, 1).Style.Font.Bold = true;
                ws.Cell(row, 2).Value = item.Detalle.AccionCorrectiva ?? "No especificada";
                row++;
            }
            else
            {
                // ✅ Si hay 2 o más paradas, usar formato TABLA COMPACTA
                ws.Cell(row, 1).Value = "Tipo";
                ws.Cell(row, 2).Value = "Descripción";
                ws.Cell(row, 3).Value = "Inicio";
                ws.Cell(row, 4).Value = "Fin";
                ws.Cell(row, 5).Value = "Dur.";
                ws.Cell(row, 6).Value = "Motivo";
                ws.Cell(row, 7).Value = "Acción";
                ws.Range(row, 1, row, 7).Style.Font.Bold = true;
                ws.Range(row, 1, row, 7).Style.Fill.BackgroundColor = XLColor.LightGray;
                row++;

                foreach (var item in paradasOrdenadas)
                {
                    ws.Cell(row, 1).Value = item.Tipo;
                    ws.Cell(row, 2).Value = TruncarTexto(item.Detalle.Descripcion ?? "Sin descripción", 20);
                    ws.Cell(row, 3).Value = item.Detalle.Inicio.ToString("dd/MM HH:mm");
                    ws.Cell(row, 4).Value = item.Detalle.Fin?.ToString("dd/MM HH:mm") ?? "En curso";
                    ws.Cell(row, 5).Value = Math.Round(item.Detalle.Minutos, 0);
                    ws.Cell(row, 5).Style.NumberFormat.Format = "0";
                    ws.Cell(row, 6).Value = TruncarTexto(item.Detalle.MotivoFalla ?? "No especificado", 25);
                    ws.Cell(row, 7).Value = TruncarTexto(item.Detalle.AccionCorrectiva ?? "No especificada", 30);
                    row++;
                }

                // Información adicional: cantidad de paradas
                row++;
                ws.Cell(row, 1).Value = $"Total de paradas registradas: {cantidadParadas}";
                ws.Range(row, 1, row, 7).Merge();
                ws.Cell(row, 1).Style.Font.Italic = true;
                ws.Cell(row, 1).Style.Font.FontColor = XLColor.Gray;
                row++;
            }
        }
        else
        {
            ws.Cell(row, 1).Value = "No hay paradas registradas con detalles";
            ws.Range(row, 1, row, 2).Merge();
            ws.Cell(row, 1).Style.Font.FontColor = XLColor.Gray;
            row++;
        }

        // Ajustar ancho de columnas
        ws.Column(1).Width = 12;   // Tipo
        ws.Column(2).Width = 22;   // Descripción
        ws.Column(3).Width = 12;   // Inicio
        ws.Column(4).Width = 12;   // Fin
        ws.Column(5).Width = 8;    // Duración
        ws.Column(6).Width = 25;   // Motivo
        ws.Column(7).Width = 30;   // Acción

        // ============ SECCIÓN: CONSUMO DE BOLSAS ============
        if (consumos != null && consumos.Any())
        {
            row += 2;
            ws.Cell(row, 1).Value = "CONSUMO DE BOLSAS";
            ws.Range(row, 1, row, 4).Merge();
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 1).Style.Font.FontColor = XLColor.White;
            ws.Range(row, 1, row, 4).Style.Fill.BackgroundColor = XLColor.MediumBlue;

            row++;
            ws.Cell(row, 1).Value = "Proveedor";
            ws.Cell(row, 2).Value = "Cantidad";
            ws.Cell(row, 3).Value = "Bolsas Defectuosas";
            ws.Cell(row, 4).Value = "Observaciones";
            ws.Range(row, 1, row, 4).Style.Font.Bold = true;
            ws.Range(row, 1, row, 4).Style.Fill.BackgroundColor = XLColor.LightGray;

            row++;
            int totalCantidad = 0;
            int totalDefectuosas = 0;

            foreach (var c in consumos)
            {
                ws.Cell(row, 1).Value = string.IsNullOrWhiteSpace(c.ProveedorNombre) ? "Desconocido" : c.ProveedorNombre;
                ws.Cell(row, 2).Value = c.CantidadBolsas;
                ws.Cell(row, 3).Value = c.BolsasDefectuosas;
                ws.Cell(row, 4).Value = c.Observaciones ?? string.Empty;
                ws.Cell(row, 2).Style.NumberFormat.Format = "#,##0";
                ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0";

                totalCantidad += c.CantidadBolsas;
                totalDefectuosas += c.BolsasDefectuosas;
                row++;
            }

            ws.Cell(row, 1).Value = "TOTALES";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 2).Value = totalCantidad;
            ws.Cell(row, 2).Style.Font.Bold = true;
            ws.Cell(row, 2).Style.NumberFormat.Format = "#,##0";
            ws.Cell(row, 3).Value = totalDefectuosas;
            ws.Cell(row, 3).Style.Font.Bold = true;
            ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0";
            ws.Range(row, 1, row, 4).Style.Fill.BackgroundColor = XLColor.LightGray;

            ws.Columns().AdjustToContents();
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] GenerarReporteDiario(List<MetricasTurnoDto> metricasTurnos, List<TurnoDto> turnos, MetricasDiariasDto metricasDiarias)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Resumen Diario");

        // ============ ENCABEZADO ============
        var row = 1;
        ws.Cell(row, 1).Value = "REPORTE CONSOLIDADO DIARIO - PRODUCCIÓN";
        ws.Range(row, 1, row, 8).Merge().Style
            .Font.SetBold().Font.SetFontSize(18)
            .Fill.SetBackgroundColor(XLColor.DarkBlue)
            .Font.SetFontColor(XLColor.White)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        row += 2;
        ws.Cell(row, 1).Value = "Fecha:";
        ws.Cell(row, 2).Value = metricasDiarias.Fecha.ToString("dd/MM/yyyy");
        ws.Cell(row, 2).Style.Font.SetBold().Font.SetFontSize(14);

        // ============ FACTORES DIARIOS ============
        row += 2;
        ws.Cell(row, 1).Value = "FACTORES DIARIOS";
        FormatearEncabezadoSeccion(ws.Range(row, 1, row, 6), XLColor.DarkGreen);

        row++;
        ws.Cell(row, 1).Value = "Indicador";
        ws.Cell(row, 2).Value = "Valor";
        ws.Cell(row, 3).Value = "Objetivo";
        ws.Cell(row, 4).Value = "Estado";
        FormatearEncabezadoTabla(ws.Range(row, 1, row, 4));

        row++;
        ws.Cell(row, 1).Value = "Factor de Confiabilidad Diario (FC)";
        ws.Cell(row, 2).Value = $"{metricasDiarias.FactorConfiabilidadDiario:N2}%";
        ws.Cell(row, 3).Value = "≥90%";
        ws.Cell(row, 4).Value = metricasDiarias.FactorConfiabilidadDiario >= 90 ? "✅ CUMPLE" : "❌ NO CUMPLE";
        var colorFC = metricasDiarias.FactorConfiabilidadDiario >= 90 ? XLColor.Green : XLColor.Red;
        ws.Cell(row, 4).Style.Fill.SetBackgroundColor(colorFC).Font.SetFontColor(XLColor.White).Font.SetBold();

        row++;
        ws.Cell(row, 1).Value = "Factor de Producción Diario (FP)";
        ws.Cell(row, 2).Value = $"{metricasDiarias.FactorProduccionDiario:N2}%";
        ws.Cell(row, 3).Value = "≥90%";
        ws.Cell(row, 4).Value = metricasDiarias.FactorProduccionDiario >= 90 ? "✅ CUMPLE" : "❌ NO CUMPLE";
        var colorFP = metricasDiarias.FactorProduccionDiario >= 90 ? XLColor.Green : XLColor.Red;
        ws.Cell(row, 4).Style.Fill.SetBackgroundColor(colorFP).Font.SetFontColor(XLColor.White).Font.SetBold();

        // ============ TOTALES DIARIOS ============
        row += 2;
        ws.Cell(row, 1).Value = "TOTALES DIARIOS";
        FormatearEncabezadoSeccion(ws.Range(row, 1, row, 4), XLColor.DarkOrange);

        row++;
        ws.Cell(row, 1).Value = "Horas Marcha Total:";
        ws.Cell(row, 2).Value = FormatearHoras(metricasDiarias.HorasMarchaTotales);
        row++;
        ws.Cell(row, 1).Value = "Horas Productivas Total:";
        ws.Cell(row, 2).Value = FormatearHoras(metricasDiarias.HorasProductivasTotales);
        row++;
        ws.Cell(row, 1).Value = "Total Paradas:";
        ws.Cell(row, 2).Value = FormatearHoras(metricasDiarias.TotalParadasDiarias);
        row++;
        ws.Cell(row, 1).Value = "Toneladas Producidas:";
        ws.Cell(row, 2).Value = metricasDiarias.ToneladasProducidasDiarias;
        ws.Cell(row, 2).Style.NumberFormat.Format = "0.00";
        row++;
        ws.Cell(row, 1).Value = "Bolsas Totales:";
        ws.Cell(row, 2).Value = metricasDiarias.BolsasTotalesDiarias;
        row++;
        ws.Cell(row, 1).Value = "Palets Totales:";
        ws.Cell(row, 2).Value = metricasDiarias.PaletsTotalesDiarios;

        // ============ COMPARATIVO POR TURNOS ============
        row += 2;
        ws.Cell(row, 1).Value = "COMPARATIVO POR TURNOS";
        FormatearEncabezadoSeccion(ws.Range(row, 1, row, 8), XLColor.DarkBlue);

        row++;
        ws.Cell(row, 1).Value = "Turno";
        ws.Cell(row, 2).Value = "FC (%)";
        ws.Cell(row, 3).Value = "FP (%)";
        ws.Cell(row, 4).Value = "Tn/h";
        ws.Cell(row, 5).Value = "Bolsas";
        ws.Cell(row, 6).Value = "Palets";
        ws.Cell(row, 7).Value = "Horas Prod.";
        ws.Cell(row, 8).Value = "Estado";
        FormatearEncabezadoTabla(ws.Range(row, 1, row, 8));

        row++;
        foreach (var metricas in metricasTurnos.OrderBy(m => m.TurnoNumero))
        {
            ws.Cell(row, 1).Value = $"Turno {metricas.TurnoNumero}";
            ws.Cell(row, 2).Value = metricas.FactorConfiabilidad;
            ws.Cell(row, 2).Style.NumberFormat.Format = "0.00";
            ws.Cell(row, 3).Value = metricas.FactorProduccion;
            ws.Cell(row, 3).Style.NumberFormat.Format = "0.00";
            ws.Cell(row, 4).Value = metricas.ToneladasPorHora;
            ws.Cell(row, 4).Style.NumberFormat.Format = "0.00";
            ws.Cell(row, 5).Value = metricas.BolsasRealizadas;
            ws.Cell(row, 6).Value = metricas.PaletsRealizados;
            ws.Cell(row, 7).Value = metricas.HorasProductivas.TotalHours;
            ws.Cell(row, 7).Style.NumberFormat.Format = "0.00";

            var turno = turnos.FirstOrDefault(t => t.TurnoProduccionID == metricas.TurnoProduccionID);
            ws.Cell(row, 8).Value = turno?.Estado ?? "N/A";

            row++;
        }

        foreach (var metricas in metricasTurnos)
        {
            var turno = turnos.First(t => t.TurnoProduccionID == metricas.TurnoProduccionID);
            var wsTurno = workbook.Worksheets.Add($"Turno {metricas.TurnoNumero}");
            CopiarDatosTurno(wsTurno, metricas, turno);
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] GenerarReporteMensual(List<MetricasTurnoDto> metricasTurnos, List<TurnoDto> turnos, int año, int mes)
    {
        using var workbook = new XLWorkbook();

        var wsResumen = workbook.Worksheets.Add("Resumen Mensual");
        GenerarHojaResumenMensual(wsResumen, metricasTurnos, turnos, año, mes);

        var wsComparativo = workbook.Worksheets.Add("Comparativo Diario");
        GenerarHojaComparativoDiario(wsComparativo, metricasTurnos, turnos);

        var wsDetalle = workbook.Worksheets.Add("Detalle por Turno");
        GenerarHojaDetalleTurnos(wsDetalle, metricasTurnos, turnos);

        var wsParadas = workbook.Worksheets.Add("Análisis de Paradas");
        GenerarHojaAnalisisParadas(wsParadas, metricasTurnos);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    // ===== MÉTODOS AUXILIARES =====

    private void FormatearEncabezadoSeccion(IXLRange range, XLColor color)
    {
        range.Merge().Style
            .Font.SetBold().Font.SetFontSize(12)
            .Fill.SetBackgroundColor(color)
            .Font.SetFontColor(XLColor.White)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
    }

    private void FormatearEncabezadoTabla(IXLRange range)
    {
        range.Style
            .Font.SetBold()
            .Fill.SetBackgroundColor(XLColor.LightGray)
            .Border.SetOutsideBorder(XLBorderStyleValues.Medium);
    }

    private void CopiarDatosTurno(IXLWorksheet ws, MetricasTurnoDto metricas, TurnoDto turno)
    {
        var row = 1;
        ws.Cell(row, 1).Value = $"TURNO {metricas.TurnoNumero} - {metricas.Fecha:dd/MM/yyyy}";
        ws.Range(row, 1, row, 4).Merge().Style.Font.SetBold().Font.SetFontSize(14);

        row += 2;
        ws.Cell(row, 1).Value = "FC:"; ws.Cell(row, 2).Value = $"{metricas.FactorConfiabilidad:N2}%";
        row++;
        ws.Cell(row, 1).Value = "FP:"; ws.Cell(row, 2).Value = $"{metricas.FactorProduccion:N2}%";
        row++;
        ws.Cell(row, 1).Value = "Bolsas:"; ws.Cell(row, 2).Value = metricas.BolsasRealizadas;
        row++;
        ws.Cell(row, 1).Value = "Toneladas:"; ws.Cell(row, 2).Value = metricas.ToneladasProducidas;

        ws.Columns().AdjustToContents();
    }

    private void GenerarHojaResumenMensual(IXLWorksheet ws, List<MetricasTurnoDto> metricasTurnos, List<TurnoDto> turnos, int año, int mes)
    {
        var row = 1;

        ws.Cell(row, 1).Value = $"REPORTE MENSUAL DE PRODUCCIÓN - {año}/{mes:D2}";
        ws.Range(row, 1, row, 8).Merge().Style
            .Font.SetBold().Font.SetFontSize(18)
            .Fill.SetBackgroundColor(XLColor.DarkBlue)
            .Font.SetFontColor(XLColor.White)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        row += 2;

        ws.Cell(row, 1).Value = "TOTALES DEL MES";
        FormatearEncabezadoSeccion(ws.Range(row, 1, row, 4), XLColor.DarkGreen);

        row++;
        var totalBolsas = metricasTurnos.Sum(m => m.BolsasNetas);
        var totalToneladas = metricasTurnos.Sum(m => m.ToneladasProducidas);
        var totalHorasProductivas = TimeSpan.FromHours(metricasTurnos.Sum(m => m.HorasProductivas.TotalHours));
        var totalHorasParadas = TimeSpan.FromHours(metricasTurnos.Sum(m => m.TotalParadas.TotalHours));
        var totalPalets = metricasTurnos.Sum(m => m.PaletsRealizados);

        ws.Cell(row, 1).Value = "Total Bolsas Producidas:";
        ws.Cell(row, 2).Value = totalBolsas;
        ws.Cell(row, 2).Style.Font.SetBold().NumberFormat.Format = "#,##0";
        row++;

        ws.Cell(row, 1).Value = "Total Toneladas:";
        ws.Cell(row, 2).Value = totalToneladas;
        ws.Cell(row, 2).Style.Font.SetBold().NumberFormat.Format = "#,##0.00";
        row++;

        ws.Cell(row, 1).Value = "Total Palets:";
        ws.Cell(row, 2).Value = totalPalets;
        ws.Cell(row, 2).Style.Font.SetBold().NumberFormat.Format = "#,##0";
        row++;

        ws.Cell(row, 1).Value = "Horas Productivas Totales:";
        ws.Cell(row, 2).Value = FormatearHoras(totalHorasProductivas);
        ws.Cell(row, 2).Style.Font.SetBold();
        row++;

        ws.Cell(row, 1).Value = "Total Horas de Paradas:";
        ws.Cell(row, 2).Value = FormatearHoras(totalHorasParadas);
        ws.Cell(row, 2).Style.Font.SetBold();

        row += 2;

        ws.Cell(row, 1).Value = "PROMEDIOS MENSUALES";
        FormatearEncabezadoSeccion(ws.Range(row, 1, row, 4), XLColor.DarkOrange);

        row++;
        var cantidadTurnos = metricasTurnos.Count;
        var promedioFC = metricasTurnos.Average(m => m.FactorConfiabilidad);
        var promedioFP = metricasTurnos.Average(m => m.FactorProduccion);
        var promedioTnH = metricasTurnos.Average(m => m.ToneladasPorHora);

        ws.Cell(row, 1).Value = "Cantidad de Turnos:";
        ws.Cell(row, 2).Value = cantidadTurnos;
        row++;

        ws.Cell(row, 1).Value = "Factor Confiabilidad Promedio:";
        ws.Cell(row, 2).Value = promedioFC;
        ws.Cell(row, 2).Style.NumberFormat.Format = "0.00\"%\"";
        var colorFC = promedioFC >= 90 ? XLColor.Green : XLColor.Orange;
        ws.Cell(row, 2).Style.Fill.SetBackgroundColor(colorFC).Font.SetFontColor(XLColor.White).Font.SetBold();
        row++;

        ws.Cell(row, 1).Value = "Factor Producción Promedio:";
        ws.Cell(row, 2).Value = promedioFP;
        ws.Cell(row, 2).Style.NumberFormat.Format = "0.00\"%\"";
        var colorFP = promedioFP >= 90 ? XLColor.Green : XLColor.Orange;
        ws.Cell(row, 2).Style.Fill.SetBackgroundColor(colorFP).Font.SetFontColor(XLColor.White).Font.SetBold();
        row++;

        ws.Cell(row, 1).Value = "Toneladas/Hora Promedio:";
        ws.Cell(row, 2).Value = promedioTnH;
        ws.Cell(row, 2).Style.NumberFormat.Format = "0.00";

        row += 2;

        ws.Cell(row, 1).Value = "DISTRIBUCIÓN POR NÚMERO DE TURNO";
        FormatearEncabezadoSeccion(ws.Range(row, 1, row, 6), XLColor.DarkBlue);

        row++;
        ws.Cell(row, 1).Value = "Turno";
        ws.Cell(row, 2).Value = "Cantidad";
        ws.Cell(row, 3).Value = "Bolsas";
        ws.Cell(row, 4).Value = "Toneladas";
        ws.Cell(row, 5).Value = "FC Prom (%)";
        ws.Cell(row, 6).Value = "FP Prom (%)";
        FormatearEncabezadoTabla(ws.Range(row, 1, row, 6));

        row++;
        for (int turnoNum = 1; turnoNum <= 3; turnoNum++)
        {
            var turnosX = metricasTurnos.Where(m => m.TurnoNumero == turnoNum).ToList();
            if (turnosX.Any())
            {
                ws.Cell(row, 1).Value = $"Turno {turnoNum}";
                ws.Cell(row, 2).Value = turnosX.Count;
                ws.Cell(row, 3).Value = turnosX.Sum(t => t.BolsasNetas);
                ws.Cell(row, 4).Value = turnosX.Sum(t => t.ToneladasProducidas);
                ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, 5).Value = turnosX.Average(t => t.FactorConfiabilidad);
                ws.Cell(row, 5).Style.NumberFormat.Format = "0.00";
                ws.Cell(row, 6).Value = turnosX.Average(t => t.FactorProduccion);
                ws.Cell(row, 6).Style.NumberFormat.Format = "0.00";
                row++;
            }
        }

        ws.Columns().AdjustToContents();
    }

    private void GenerarHojaComparativoDiario(IXLWorksheet ws, List<MetricasTurnoDto> metricasTurnos, List<TurnoDto> turnos)
    {
        var row = 1;

        ws.Cell(row, 1).Value = "COMPARATIVO DIARIO - TODOS LOS TURNOS";
        ws.Range(row, 1, row, 10).Merge().Style
            .Font.SetBold().Font.SetFontSize(16)
            .Fill.SetBackgroundColor(XLColor.DarkBlue)
            .Font.SetFontColor(XLColor.White)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        row += 2;

        ws.Cell(row, 1).Value = "Fecha";
        ws.Cell(row, 2).Value = "Turno";
        ws.Cell(row, 3).Value = "Estado";
        ws.Cell(row, 4).Value = "Bolsas";
        ws.Cell(row, 5).Value = "Toneladas";
        ws.Cell(row, 6).Value = "Palets";
        ws.Cell(row, 7).Value = "FC (%)";
        ws.Cell(row, 8).Value = "FP (%)";
        ws.Cell(row, 9).Value = "Tn/h";
        ws.Cell(row, 10).Value = "Hrs Prod";
        FormatearEncabezadoTabla(ws.Range(row, 1, row, 10));

        row++;

        foreach (var metricas in metricasTurnos.OrderBy(m => m.Fecha).ThenBy(m => m.TurnoNumero))
        {
            var turno = turnos.First(t => t.TurnoProduccionID == metricas.TurnoProduccionID);

            ws.Cell(row, 1).Value = metricas.Fecha.ToString("dd/MM/yyyy");
            ws.Cell(row, 2).Value = $"T{metricas.TurnoNumero}";
            ws.Cell(row, 3).Value = turno.Estado;
            ws.Cell(row, 4).Value = metricas.BolsasNetas;
            ws.Cell(row, 5).Value = metricas.ToneladasProducidas;
            ws.Cell(row, 5).Style.NumberFormat.Format = "0.00";
            ws.Cell(row, 6).Value = metricas.PaletsRealizados;
            ws.Cell(row, 7).Value = metricas.FactorConfiabilidad;
            ws.Cell(row, 7).Style.NumberFormat.Format = "0.00";
            ws.Cell(row, 8).Value = metricas.FactorProduccion;
            ws.Cell(row, 8).Style.NumberFormat.Format = "0.00";
            ws.Cell(row, 9).Value = metricas.ToneladasPorHora;
            ws.Cell(row, 9).Style.NumberFormat.Format = "0.00";
            ws.Cell(row, 10).Value = metricas.HorasProductivas.TotalHours;
            ws.Cell(row, 10).Style.NumberFormat.Format = "0.00";

            if (turno.Estado == "Finalizado")
            {
                ws.Range(row, 1, row, 10).Style.Fill.SetBackgroundColor(XLColor.LightGreen);
            }
            else if (turno.Estado == "En Proceso")
            {
                ws.Range(row, 1, row, 10).Style.Fill.SetBackgroundColor(XLColor.LightYellow);
            }

            row++;
        }

        ws.Columns().AdjustToContents();
    }

    private void GenerarHojaDetalleTurnos(IXLWorksheet ws, List<MetricasTurnoDto> metricasTurnos, List<TurnoDto> turnos)
    {
        var row = 1;

        foreach (var metricas in metricasTurnos.OrderBy(m => m.Fecha).ThenBy(m => m.TurnoNumero))
        {
            var turno = turnos.First(t => t.TurnoProduccionID == metricas.TurnoProduccionID);

            ws.Cell(row, 1).Value = $"TURNO {metricas.TurnoNumero} - {metricas.Fecha:dd/MM/yyyy} - {turno.Estado}";
            ws.Range(row, 1, row, 6).Merge().Style
                .Font.SetBold().Font.SetFontSize(12)
                .Fill.SetBackgroundColor(XLColor.DarkGray)
                .Font.SetFontColor(XLColor.White);

            row++;

            ws.Cell(row, 1).Value = "Bolsas Netas:";
            ws.Cell(row, 2).Value = metricas.BolsasNetas;
            ws.Cell(row, 3).Value = "Toneladas:";
            ws.Cell(row, 4).Value = metricas.ToneladasProducidas;
            ws.Cell(row, 4).Style.NumberFormat.Format = "0.00";
            row++;

            ws.Cell(row, 1).Value = "FC:";
            ws.Cell(row, 2).Value = $"{metricas.FactorConfiabilidad:N2}%";
            ws.Cell(row, 3).Value = "FP:";
            ws.Cell(row, 4).Value = $"{metricas.FactorProduccion:N2}%";
            row++;

            ws.Cell(row, 1).Value = "Paradas Totales:";
            ws.Cell(row, 2).Value = FormatearHoras(metricas.TotalParadas);
            ws.Cell(row, 3).Value = "Hrs Productivas:";
            ws.Cell(row, 4).Value = FormatearHoras(metricas.HorasProductivas);

            row += 2;
        }

        ws.Columns().AdjustToContents();
    }

    private void GenerarHojaAnalisisParadas(IXLWorksheet ws, List<MetricasTurnoDto> metricasTurnos)
    {
        var row = 1;

        ws.Cell(row, 1).Value = "ANÁLISIS DE PARADAS MENSUALES";
        ws.Range(row, 1, row, 5).Merge().Style
            .Font.SetBold().Font.SetFontSize(16)
            .Fill.SetBackgroundColor(XLColor.DarkRed)
            .Font.SetFontColor(XLColor.White)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        row += 2;

        ws.Cell(row, 1).Value = "Tipo de Parada";
        ws.Cell(row, 2).Value = "Total Minutos";
        ws.Cell(row, 3).Value = "Total Horas";
        ws.Cell(row, 4).Value = "% del Total";
        FormatearEncabezadoTabla(ws.Range(row, 1, row, 4));

        row++;

        var totalMecanicas = metricasTurnos.Sum(m => m.ParadasMecanicas);
        var totalElectricas = metricasTurnos.Sum(m => m.ParadasElectricas);
        var totalOperativas = metricasTurnos.Sum(m => m.ParadasOperativas);
        var totalCircunstanciales = metricasTurnos.Sum(m => m.ParadasCircunstanciales);
        var totalGeneral = totalMecanicas + totalElectricas + totalOperativas + totalCircunstanciales;

        var paradas = new[]
        {
            new { Tipo = "MECÁNICAS", Minutos = totalMecanicas },
            new { Tipo = "ELÉCTRICAS", Minutos = totalElectricas },
            new { Tipo = "OPERATIVAS", Minutos = totalOperativas },
            new { Tipo = "CIRCUNSTANCIALES", Minutos = totalCircunstanciales }
        };

        foreach (var parada in paradas.OrderByDescending(p => p.Minutos))
        {
            ws.Cell(row, 1).Value = parada.Tipo;
            ws.Cell(row, 2).Value = parada.Minutos;
            ws.Cell(row, 2).Style.NumberFormat.Format = "#,##0";
            ws.Cell(row, 3).Value = parada.Minutos / 60.0;
            ws.Cell(row, 3).Style.NumberFormat.Format = "0.00";
            ws.Cell(row, 4).Value = totalGeneral > 0 ? (parada.Minutos / totalGeneral * 100) : 0;
            ws.Cell(row, 4).Style.NumberFormat.Format = "0.00\"%\"";
            row++;
        }

        row++;
        ws.Cell(row, 1).Value = "TOTAL";
        ws.Cell(row, 2).Value = totalGeneral;
        ws.Cell(row, 3).Value = totalGeneral / 60.0;
        ws.Cell(row, 4).Value = 100;
        ws.Range(row, 1, row, 4).Style.Fill.SetBackgroundColor(XLColor.LightGray).Font.SetBold();

        ws.Columns().AdjustToContents();
    }

    private string FormatearHoras(TimeSpan tiempo)
    {
        return $"{(int)tiempo.TotalHours:D2}:{tiempo.Minutes:D2}:{tiempo.Seconds:D2}";
    }

    private string FormatearMinutosHHMM(double minutos)
    {
        var tiempo = TimeSpan.FromMinutes(minutos);
        return $"{(int)tiempo.TotalHours:D2}:{tiempo.Minutes:D2}";
    }

    private string ObtenerHorarioTurno(int turnoNumero)
    {
        return turnoNumero switch
        {
            1 => "06:00 - 14:30 (8h 10m)",
            2 => "14:30 - 22:30 (7h 40m)",
            3 => "22:30 - 06:00 (7h 10m)",
            _ => "N/A"
        };
    }

    private string TruncarTexto(string texto, int maxLongitud)
    {
        if (string.IsNullOrEmpty(texto)) return texto;
        if (texto.Length <= maxLongitud) return texto;
        return texto.Substring(0, maxLongitud - 3) + "...";
    }
}