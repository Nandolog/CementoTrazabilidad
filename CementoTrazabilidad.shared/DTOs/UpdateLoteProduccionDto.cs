using System.ComponentModel.DataAnnotations;

namespace CementoTrazabilidad.Shared.DTOs
{
    public class UpdateLoteProduccionDto
    {
        [Required(ErrorMessage = "El ID del lote es requerido")]
        public int LoteID { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Las bolsas rotas no pueden ser negativas")]
        public int BolsasRotas { get; set; }

        [Range(0, 24, ErrorMessage = "Las horas de marcha deben estar entre 0 y 24")]
        public decimal HorasMarcha { get; set; }

        public string? Observaciones { get; set; }
    }
}