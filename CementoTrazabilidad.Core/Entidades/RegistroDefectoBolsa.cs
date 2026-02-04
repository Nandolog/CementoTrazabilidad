using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CementoTrazabilidad.Core.Entidades
{
    /// <summary>
    /// Registra bolsas defectuosas con trazabilidad completa
    /// </summary>
    public class RegistroDefectoBolsa
    {
        [Key]
        public int RegistroDefectoBolsaID { get; set; }

        [Required]
        public int LoteProveedorBolsaID { get; set; }

        [Required]
        public int TurnoProduccionID { get; set; }

        public int? ProduccionMaterialID { get; set; }

        [Required]
        public int Cantidad { get; set; }

        [Required]
        [MaxLength(100)]
        public string TipoDefecto { get; set; } = string.Empty; 
        // Ejemplos: Rotura, Impresión defectuosa, Sellado deficiente, 
        // Papel roto, Peso incorrecto, Dimensiones incorrectas

        [MaxLength(500)]
        public string? Descripcion { get; set; }

        public DateTime FechaHoraRegistro { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        public string? RegistradoPor { get; set; }

        // ✅ Clasificación de gravedad
        [MaxLength(20)]
        public string? Gravedad { get; set; } // Leve, Moderada, Grave

        // ✅ Indica si el proveedor fue notificado
        public bool ProveedorNotificado { get; set; } = false;

        public DateTime? FechaNotificacion { get; set; }

        // Navigation properties
        [ForeignKey(nameof(LoteProveedorBolsaID))]
        public virtual LoteProveedorBolsa LoteProveedorBolsa { get; set; } = null!;

        [ForeignKey(nameof(TurnoProduccionID))]
        public virtual TurnoProduccion TurnoProduccion { get; set; } = null!;

        [ForeignKey(nameof(ProduccionMaterialID))]
        public virtual ProduccionMaterial? ProduccionMaterial { get; set; }
    }
}