namespace CementoTrazabilidad.Core.Entidades
{
    public class RegistroStockPalets
    {
        public int RegistroStockPaletsID { get; set; }
        
        public int TurnoProduccionID { get; set; }
        public virtual TurnoProduccion TurnoProduccion { get; set; } = null!;
        
        public int PersonalID { get; set; }
        public virtual Personal PersonalRegistro { get; set; } = null!;
        
        // Stock Inicial
        public int StockInicialC32 { get; set; }
        public int StockInicialF40 { get; set; }
        
        // Stock Final
        public int? StockFinalC32 { get; set; }
        public int? StockFinalF40 { get; set; }
        
        // Calculados (producción neta del turno)
        public int ProduccionNetaC32 => (StockFinalC32 ?? StockInicialC32) - StockInicialC32;
        public int ProduccionNetaF40 => (StockFinalF40 ?? StockInicialF40) - StockInicialF40;
        public int ProduccionTotalNeta => ProduccionNetaC32 + ProduccionNetaF40;
        
        // Timestamps
        public DateTime FechaHoraRegistroInicial { get; set; }
        public DateTime? FechaHoraRegistroFinal { get; set; }
        
        public string? ObservacionesInicio { get; set; }
        public string? ObservacionesFin { get; set; }
        
        public bool Activo { get; set; } = true;
    }
}