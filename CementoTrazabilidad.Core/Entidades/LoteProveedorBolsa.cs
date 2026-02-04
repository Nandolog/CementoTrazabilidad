using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CementoTrazabilidad.Core.Entidades
{
    /// <summary>
    /// Representa un lote específico de bolsas de un proveedor
    /// </summary>
    public class LoteProveedorBolsa
    {
        [Key]
        public int LoteProveedorBolsaID { get; set; }

        [Required]
        public int ProveedorBolsaID { get; set; }

        [Required]
        [MaxLength(50)]
        public string NumeroLote { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string TipoCemento { get; set; } = string.Empty; // C32, F40

        public DateOnly FechaRecepcion { get; set; }

        public DateOnly? FechaVencimiento { get; set; }

        [Required]
        public int CantidadInicial { get; set; }

        public int CantidadActual { get; set; }

        public int CantidadDefectuosa { get; set; } = 0;

        [MaxLength(20)]
        public string Estado { get; set; } = "Disponible"; // Disponible, En Uso, Agotado, Bloqueado

        [Column(TypeName = "decimal(10,2)")]
        public decimal? PrecioUnitario { get; set; }

        [MaxLength(500)]
        public string? Observaciones { get; set; }

        // Navigation properties
        [ForeignKey(nameof(ProveedorBolsaID))]
        public virtual ProveedorBolsa ProveedorBolsa { get; set; } = null!;

        public virtual ICollection<RegistroDefectoBolsa> Defectos { get; set; } = new List<RegistroDefectoBolsa>();
    }
}