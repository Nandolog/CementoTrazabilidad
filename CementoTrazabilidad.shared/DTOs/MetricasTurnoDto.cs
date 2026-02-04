namespace CementoTrazabilidad.Shared.DTOs;

public class MetricasTurnoDto
{
    // Identificación
    public int TurnoProduccionID { get; set; }
    public int TurnoNumero { get; set; }
    public DateOnly Fecha { get; set; }
    
    // Tiempos del Turno
    public TimeSpan HorasMarcha { get; set; }
    public TimeSpan HorasProductivas { get; set; }
    public TimeSpan TotalParadas { get; set; }
    public TimeSpan HorasTeoricasTurno { get; set; } // TM=8:10, TT=7:40, TN=7:10
    
    // Paradas Clasificadas (en minutos)
    public double ParadasMecanicas { get; set; }
    public double ParadasElectricas { get; set; }
    public double ParadasOperativas { get; set; }
    public double ParadasCircunstanciales { get; set; }
    
    // Tiempos de Actividades (calculados desde EventosCarga)
    public double TiempoAndenes { get; set; } // Minutos en carga de andenes
    public double TiempoPaletizado { get; set; } // Minutos en paletizado
    public double TiempoCambioCamara { get; set; } // Si se registra como parada
    public double TiempoStockLleno { get; set; }
    
    // Producción
    public int BolsasRealizadas { get; set; }
    public int BolsasRotas { get; set; }
    public int BolsasNetas { get; set; }
    public decimal ToneladasProducidas { get; set; }
    public decimal ToneladasPorHora { get; set; }
    
    // Andenes y Palets
    public int CantidadAndenes { get; set; } // Andenes que se usaron
    public int PaletsRealizados { get; set; }
    
    // KPIs y Factores
    public decimal FactorCorreccion { get; set; } // FC = (HsProductivas / HsMarcha) * 100
    public decimal FactorProduccion { get; set; } // FP = (TnReal/h / TnObjetivo/h) * 100
    public decimal EficienciaGlobal { get; set; } // Tiempo productivo vs objetivo
    
    // Objetivos (hardcoded por ahora)
    public decimal ToneladasPorHoraObjetivo { get; set; } // 80 Tn/h
    public TimeSpan HorasProductivasObjetivo { get; set; } // 7.7h
    public int PaletsObjetivoDiario { get; set; } // 640
    public int PaletsObjetivoTurno { get; set; } // 640/3 = 213
    
    // Cumplimientos
    public decimal CumplimientoProduccion { get; set; } // vs objetivo Tn/h
    public decimal CumplimientoHoras { get; set; } // vs objetivo horas
    public decimal CumplimientoPalets { get; set; } // vs objetivo palets turno
}