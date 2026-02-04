using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CementoTrazabilidad.Core.Entidades
{
    /// <summary>
    /// Registra el consumo de bolsas por turno y producción
    /// </summary>
    public class ConsumoBolsas
    {
        [Key]
        public int ConsumoBolsasID { get; set; }

        [Required]
        public int ProveedorBolsaID { get; set; }

        [Required]
        public int TurnoProduccionID { get; set; }

        // ✅ NUEVO: Relacionar con producción específica (opcional)
        public int? ProduccionMaterialID { get; set; }

        [Required]
        public int CantidadBolsas { get; set; }

        // ✅ NUEVO: Bolsas defectuosas/rotas del lote
        public int BolsasDefectuosas { get; set; } = 0;

        public DateTime FechaConsumo { get; set; } = DateTime.UtcNow;

        [MaxLength(50)]
        public string? LoteBolsa { get; set; }

        // ✅ NUEVO: Tipo de cemento para este consumo
        [MaxLength(10)]
        public string? TipoCemento { get; set; } // C32, F40

        [MaxLength(500)]
        public string? Observaciones { get; set; }

        // Navigation properties
        [ForeignKey(nameof(ProveedorBolsaID))]
        public virtual ProveedorBolsa ProveedorBolsa { get; set; } = null!;

        [ForeignKey(nameof(TurnoProduccionID))]
        public virtual TurnoProduccion TurnoProduccion { get; set; } = null!;

        // ✅ NUEVO: Relación opcional con ProduccionMaterial
        [ForeignKey(nameof(ProduccionMaterialID))]
        public virtual ProduccionMaterial? ProduccionMaterial { get; set; }
    }
}