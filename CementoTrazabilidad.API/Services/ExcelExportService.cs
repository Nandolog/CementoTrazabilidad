using ClosedXML.Excel;
using CementoTrazabilidad.Shared.DTOs;
using System.Drawing;

namespace CementoTrazabilidad.API.Services;

public interface IExcelExportService
{
    byte[] GenerarReporteTurno(MetricasTurnoDto metricas, TurnoDto turno, List<ParadasDetalladasDto> paradas);
    byte[] GenerarReporteDiario(List<MetricasTurnoDto> metricasTurnos, List<TurnoDto> turnos, MetricasDiariasDto metricasDiarias);
    byte[] GenerarReporteMensual(List<MetricasTurnoDto> metricasTurnos, List<TurnoDto> turnos, int año, int mes); // ✅ NUEVO
}

public class ExcelExportService : IExcelExportService
{
    public byte[] GenerarReporteTurno(MetricasTurnoDto metricas, TurnoDto turno, List<ParadasDetalladasDto> paradas)
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

        // ============ SECCIÓN: TIEMPOS ============
        row += 2;
        ws.Cell(row, 1).Value = "TIEMPOS DEL TURNO";
        FormatearEncabezadoSeccion(ws.Range(row, 1, row, 4), XLColor.DarkGreen);
        
        row++;
        var dataTiempos = new[]
        {
            new { Concepto = "Horas de Marcha", Tiempo = FormatearHoras(metricas.HorasMarcha), Horas = metricas.HorasMarcha.TotalHours },
            new { Concepto = "Horas Productivas", Tiempo = FormatearHoras(metricas.HorasProductivas), Horas = metricas.HorasProductivas.TotalHours },
            new { Concepto = "Objetivo Horas", Tiempo = FormatearHoras(metricas.HorasProductivasObjetivo), Horas = metricas.HorasProductivasObjetivo.TotalHours },
            new { Concepto = "Total Paradas", Tiempo = FormatearHoras(metricas.TotalParadas), Horas = metricas.TotalParadas.TotalHours }
        };

        ws.Cell(row, 1).Value = "Concepto";
        ws.Cell(row, 2).Value = "Tiempo (HH:MM)";
        ws.Cell(row, 3).Value = "Horas";
        FormatearEncabezadoTabla(ws.Range(row, 1, row, 3));
        
        row++;
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
            new { Indicador = "Factor de Confiabilidad (FC)", Valor = $"{metricas.FactorCorreccion:N2}%", Objetivo = "≥90%", Cumplimiento = metricas.FactorCorreccion },
            new { Indicador = "Factor de Producción (FP)", Valor = $"{metricas.FactorProduccion:N2}%", Objetivo = "≥90%", Cumplimiento = metricas.FactorProduccion },
            new { Indicador = "Toneladas/Hora", Valor = $"{metricas.ToneladasPorHora:N2}", Objetivo = "80.00", Cumplimiento = metricas.CumplimientoProduccion },
            new { Indicador = "Horas Productivas", Valor = $"{metricas.HorasProductivas.TotalHours:N2}h", Objetivo = "7.70h", Cumplimiento = metricas.CumplimientoHoras }
        };

        foreach (var kpi in dataKPIs)
        {
            ws.Cell(row, 1).Value = kpi.Indicador;
            ws.Cell(row, 2).Value = kpi.Valor;
            ws.Cell(row, 3).Value = kpi.Objetivo;
            ws.Cell(row, 4).Value = $"{kpi.Cumplimiento:N2}%";
            
            // Color según cumplimiento
            var color = kpi.Cumplimiento >= 90 ? XLColor.Green : kpi.Cumplimiento >= 70 ? XLColor.Orange : XLColor.Red;
            ws.Cell(row, 4).Style.Fill.SetBackgroundColor(color).Font.SetFontColor(XLColor.White).Font.SetBold();
            
            row++;
        }

        // ============ SECCIÓN: PRODUCCIÓN ============
        row++;
        ws.Cell(row, 1).Value = "PRODUCCIÓN";
        FormatearEncabezadoSeccion(ws.Range(row, 1, row, 3), XLColor.DarkOrange);
        
        row++;
        ws.Cell(row, 1).Value = "Bolsas Realizadas:";
        ws.Cell(row, 2).Value = metricas.BolsasRealizadas;
        row++;
        ws.Cell(row, 1).Value = "Bolsas Rotas:";
        ws.Cell(row, 2).Value = metricas.BolsasRotas;
        row++;
        ws.Cell(row, 1).Value = "Bolsas Netas:";
        ws.Cell(row, 2).Value = metricas.BolsasNetas;
        ws.Cell(row, 2).Style.Font.SetBold();
        row++;
        ws.Cell(row, 1).Value = "Toneladas Producidas:";
        ws.Cell(row, 2).Value = metricas.ToneladasProducidas;
        ws.Cell(row, 2).Style.NumberFormat.Format = "0.00";
        row++;
        ws.Cell(row, 1).Value = "Palets Realizados:";
        ws.Cell(row, 2).Value = metricas.PaletsRealizados;
        row++;
        ws.Cell(row, 1).Value = "Andenes Utilizados:";
        ws.Cell(row, 2).Value = metricas.CantidadAndenes;
        
        // ✅ AGREGAR: Nota explicativa si es valor estimado
        row++;
        ws.Cell(row, 1).Value = "Nota:";
        ws.Cell(row, 2).Value = "Los andenes varían según demanda del cliente";
        ws.Cell(row, 2).Style.Font.SetItalic().Font.SetFontSize(9);
        ws.Cell(row, 2).Style.Font.SetFontColor(XLColor.Gray);

        // ============ SECCIÓN: PARADAS ============
        row += 2;
        ws.Cell(row, 1).Value = "PARADAS CLASIFICADAS";
        FormatearEncabezadoSeccion(ws.Range(row, 1, row, 4), XLColor.DarkRed);
        
        row++;
        ws.Cell(row, 1).Value = "Tipo de Parada";
        ws.Cell(row, 2).Value = "Tiempo (HH:MM)";
        ws.Cell(row, 3).Value = "Horas";
        ws.Cell(row, 4).Value = "Minutos";
        FormatearEncabezadoTabla(ws.Range(row, 1, row, 4));
        
        row++;
        var dataParadas = new[]
        {
            new { Tipo = "MECÁNICAS", Minutos = metricas.ParadasMecanicas },
            new { Tipo = "ELÉCTRICAS", Minutos = metricas.ParadasElectricas },
            new { Tipo = "OPERATIVAS", Minutos = metricas.ParadasOperativas },
            new { Tipo = "CIRCUNSTANCIALES", Minutos = metricas.ParadasCircunstanciales }
        };

        foreach (var parada in dataParadas)
        {
            ws.Cell(row, 1).Value = parada.Tipo;
            ws.Cell(row, 2).Value = FormatearMinutosHHMM(parada.Minutos);
            ws.Cell(row, 3).Value = parada.Minutos / 60.0;
            ws.Cell(row, 3).Style.NumberFormat.Format = "0.00";
            ws.Cell(row, 4).Value = parada.Minutos;
            ws.Cell(row, 4).Style.NumberFormat.Format = "0";
            row++;
        }

        // TOTAL PARADAS
        ws.Cell(row, 1).Value = "TOTAL PARADAS";
        ws.Cell(row, 2).Value = FormatearHoras(metricas.TotalParadas);
        ws.Cell(row, 3).Value = metricas.TotalParadas.TotalHours;
        ws.Cell(row, 3).Style.NumberFormat.Format = "0.00";
        ws.Cell(row, 4).Value = metricas.TotalParadas.TotalMinutes;
        ws.Range(row, 1, row, 4).Style.Fill.SetBackgroundColor(XLColor.LightGray).Font.SetBold();

        // ✅ SECCIÓN: INFORMACIÓN ADICIONAL (después de PARADAS)
        row += 2;
        ws.Cell(row, 1).Value = "INFORMACIÓN DE CARGA";
        FormatearEncabezadoSeccion(ws.Range(row, 1, row, 3), XLColor.DarkGray);
        
        row++;
        ws.Cell(row, 1).Value = "Andenes Utilizados:";
        ws.Cell(row, 2).Value = metricas.CantidadAndenes;
        
        row++;
        ws.Cell(row, 1).Value = "Capacidad Promedio/Anden:";
        ws.Cell(row, 2).Value = metricas.CantidadAndenes > 0 
            ? $"~{metricas.BolsasNetas / metricas.CantidadAndenes:N0} bolsas" 
            : "N/A";
        
        row++;
        ws.Cell(row, 1).Value = "Observación:";
        ws.Cell(row, 2).Value = "La cantidad de andenes varía según pedidos del cliente";
        ws.Range(row, 1, row, 2).Style.Fill.SetBackgroundColor(XLColor.LightYellow);

        // Ajustar columnas
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
        ws.Cell(row, 1).Value = "Factor de Corrección Diario (FC)";
        ws.Cell(row, 2).Value = $"{metricasDiarias.FactorCorreccionDiario:N2}%";
        ws.Cell(row, 3).Value = "≥90%";
        ws.Cell(row, 4).Value = metricasDiarias.FactorCorreccionDiario >= 90 ? "✅ CUMPLE" : "❌ NO CUMPLE";
        var colorFC = metricasDiarias.FactorCorreccionDiario >= 90 ? XLColor.Green : XLColor.Red;
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
            ws.Cell(row, 2).Value = metricas.FactorCorreccion;
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

        // Crear hoja adicional para cada turno
        foreach (var metricas in metricasTurnos)
        {
            var turno = turnos.First(t => t.TurnoProduccionID == metricas.TurnoProduccionID);
            var wsTurno = workbook.Worksheets.Add($"Turno {metricas.TurnoNumero}");
            
            // Copiar datos del turno individual (simplificado)
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
        
        // ===== HOJA 1: RESUMEN MENSUAL =====
        var wsResumen = workbook.Worksheets.Add("Resumen Mensual");
        GenerarHojaResumenMensual(wsResumen, metricasTurnos, turnos, año, mes);
        
        // ===== HOJA 2: COMPARATIVO POR DÍA =====
        var wsComparativo = workbook.Worksheets.Add("Comparativo Diario");
        GenerarHojaComparativoDiario(wsComparativo, metricasTurnos, turnos);
        
        // ===== HOJA 3: DETALLE POR TURNO =====
        var wsDetalle = workbook.Worksheets.Add("Detalle por Turno");
        GenerarHojaDetalleTurnos(wsDetalle, metricasTurnos, turnos);
        
        // ===== HOJA 4: GRÁFICO DE PARADAS =====
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
        ws.Cell(row, 1).Value = "FC:"; ws.Cell(row, 2).Value = $"{metricas.FactorCorreccion:N2}%";
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
        
        // Encabezado
        ws.Cell(row, 1).Value = $"REPORTE MENSUAL DE PRODUCCIÓN - {año}/{mes:D2}";
        ws.Range(row, 1, row, 8).Merge().Style
            .Font.SetBold().Font.SetFontSize(18)
            .Fill.SetBackgroundColor(XLColor.DarkBlue)
            .Font.SetFontColor(XLColor.White)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        
        row += 2;
        
        // TOTALES MENSUALES
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
        
        // PROMEDIOS MENSUALES
        ws.Cell(row, 1).Value = "PROMEDIOS MENSUALES";
        FormatearEncabezadoSeccion(ws.Range(row, 1, row, 4), XLColor.DarkOrange);
        
        row++;
        var cantidadTurnos = metricasTurnos.Count;
        var promedioFC = metricasTurnos.Average(m => m.FactorCorreccion);
        var promedioFP = metricasTurnos.Average(m => m.FactorProduccion);
        var promedioTnH = metricasTurnos.Average(m => m.ToneladasPorHora);
        
        ws.Cell(row, 1).Value = "Cantidad de Turnos:";
        ws.Cell(row, 2).Value = cantidadTurnos;
        row++;
        
        ws.Cell(row, 1).Value = "Factor Corrección Promedio:";
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
        
        // DISTRIBUCIÓN POR TURNO (1, 2, 3)
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
                ws.Cell(row, 5).Value = turnosX.Average(t => t.FactorCorreccion);
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
        
        // Encabezado
        ws.Cell(row, 1).Value = "COMPARATIVO DIARIO - TODOS LOS TURNOS";
        ws.Range(row, 1, row, 10).Merge().Style
            .Font.SetBold().Font.SetFontSize(16)
            .Fill.SetBackgroundColor(XLColor.DarkBlue)
            .Font.SetFontColor(XLColor.White)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        
        row += 2;
        
        // Cabecera de tabla
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
        
        // Datos de todos los turnos
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
            ws.Cell(row, 7).Value = metricas.FactorCorreccion;
            ws.Cell(row, 7).Style.NumberFormat.Format = "0.00";
            ws.Cell(row, 8).Value = metricas.FactorProduccion;
            ws.Cell(row, 8).Style.NumberFormat.Format = "0.00";
            ws.Cell(row, 9).Value = metricas.ToneladasPorHora;
            ws.Cell(row, 9).Style.NumberFormat.Format = "0.00";
            ws.Cell(row, 10).Value = metricas.HorasProductivas.TotalHours;
            ws.Cell(row, 10).Style.NumberFormat.Format = "0.00";
            
            // Color según estado
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
            
            // Encabezado del turno
            ws.Cell(row, 1).Value = $"TURNO {metricas.TurnoNumero} - {metricas.Fecha:dd/MM/yyyy} - {turno.Estado}";
            ws.Range(row, 1, row, 6).Merge().Style
                .Font.SetBold().Font.SetFontSize(12)
                .Fill.SetBackgroundColor(XLColor.DarkGray)
                .Font.SetFontColor(XLColor.White);
        
            row++;
        
            // Datos clave
            ws.Cell(row, 1).Value = "Bolsas Netas:";
            ws.Cell(row, 2).Value = metricas.BolsasNetas;
            ws.Cell(row, 3).Value = "Toneladas:";
            ws.Cell(row, 4).Value = metricas.ToneladasProducidas;
            ws.Cell(row, 4).Style.NumberFormat.Format = "0.00";
            row++;
        
            ws.Cell(row, 1).Value = "FC:";
            ws.Cell(row, 2).Value = $"{metricas.FactorCorreccion:N2}%";
            ws.Cell(row, 3).Value = "FP:";
            ws.Cell(row, 4).Value = $"{metricas.FactorProduccion:N2}%";
            row++;
        
            ws.Cell(row, 1).Value = "Paradas Totales:";
            ws.Cell(row, 2).Value = FormatearHoras(metricas.TotalParadas);
            ws.Cell(row, 3).Value = "Hrs Productivas:";
            ws.Cell(row, 4).Value = FormatearHoras(metricas.HorasProductivas);
        
            row += 2; // Espacio entre turnos
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
        
        // Totales por tipo
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
}