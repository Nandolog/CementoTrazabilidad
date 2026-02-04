namespace CementoTrazabilidad.Shared.DTOs;

public class MetricasDiariasDto
{
    public DateOnly Fecha { get; set; }
    
    // Totales acumulados de los 3 turnos
    public TimeSpan HorasMarchaTotales { get; set; }
    public TimeSpan HorasProductivasTotales { get; set; }
    public TimeSpan TotalParadasDiarias { get; set; }
    
    public decimal ToneladasProducidasDiarias { get; set; }
    public int BolsasTotalesDiarias { get; set; }
    public int PaletsTotalesDiarios { get; set; }
    
    // FACTORES DIARIOS (calculados sobre el total del día)
    public decimal FactorCorreccionDiario { get; set; } // FC = (HsProductivasTotales / HsMarchaTotales) * 100
    public decimal FactorProduccionDiario { get; set; } // FP = (TnDiarias/h / TnObjetivo/h) * 100
    
    public decimal ToneladasPorHoraDiarias { get; set; }
    
    // Objetivos
    public decimal ToneladasPorHoraObjetivo { get; set; } = 80m;
    public TimeSpan HorasProductivasObjetivoDiarias { get; set; } = TimeSpan.FromHours(23.1); // 7.7h * 3
}