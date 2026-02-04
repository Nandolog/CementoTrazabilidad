using System.ComponentModel.DataAnnotations;

namespace CementoTrazabilidad.Shared.DTOs
{
    public class ProduccionModel
    {
        public int? LoteID { get; set; }
        [Required]
        public string NumeroLote { get; set; } = string.Empty;
        [Required]
        public int MaterialID { get; set; }
        public string MaterialNombre { get; set; } = string.Empty;
        [Required]
        public int CantidadBolsas { get; set; }
        public int BolsasRotas { get; set; }
        public double HorasMarcha { get; set; }
        public string? Observaciones { get; set; }
    }
}