using System.ComponentModel.DataAnnotations;

namespace CementoTrazabilidad.Shared.DTOs
{
    public class RegistroStockPaletsDto
    {
        public int RegistroStockPaletsID { get; set; }
        public int TurnoProduccionID { get; set; }
        public int PersonalID { get; set; }
        public string? PersonalNombre { get; set; }
        
        public int StockInicialC32 { get; set; }
        public int StockInicialF40 { get; set; }
        public int? StockFinalC32 { get; set; }
        public int? StockFinalF40 { get; set; }
        
        public int ProduccionNetaC32 { get; set; }
        public int ProduccionNetaF40 { get; set; }
        public int ProduccionTotalNeta { get; set; }
        
        public DateTime FechaHoraRegistroInicial { get; set; }
        public DateTime? FechaHoraRegistroFinal { get; set; }
        
        public string? ObservacionesInicio { get; set; }
        public string? ObservacionesFin { get; set; }
    }

    public class CreateStockInicialDto
    {
        public int TurnoProduccionID { get; set; }
        public int PersonalID { get; set; }
        
        [Range(0, 10000, ErrorMessage = "El stock C32 debe estar entre 0 y 10000")]
        public int StockInicialC32 { get; set; }
        
        [Range(0, 10000, ErrorMessage = "El stock F40 debe estar entre 0 y 10000")]
        public int StockInicialF40 { get; set; }
        
        public string? ObservacionesInicio { get; set; }
    }

    public class UpdateStockFinalDto
    {
        [Range(0, 10000, ErrorMessage = "El stock C32 debe estar entre 0 y 10000")]
        public int StockFinalC32 { get; set; }
        
        [Range(0, 10000, ErrorMessage = "El stock F40 debe estar entre 0 y 10000")]
        public int StockFinalF40 { get; set; }
        
        public string? ObservacionesFin { get; set; }
    }
}