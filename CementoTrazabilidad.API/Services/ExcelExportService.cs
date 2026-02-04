using ClosedXML.Excel;
using CementoTrazabilidad.Shared.DTOs;
using System.Drawing;

namespace CementoTrazabilidad.API.Services;

public interface IExcelExportService
{
    byte[] GenerarReporteTurno(MetricasTurnoDto metricas, TurnoDto turno, List<ParadasDetalladasDto> paradas);
    byte[] GenerarReporteDiario(List<MetricasTurnoDto> metricasTurnos, List<TurnoDto> turnos, MetricasDiariasDto metricasDiarias);
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
            new { Indicador = "Factor de Corrección (FC)", Valor = $"{metricas.FactorCorreccion:N2}%", Objetivo = "≥90%", Cumplimiento = metricas.FactorCorreccion },
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

    // ============ MÉTODOS AUXILIARES ============
    
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