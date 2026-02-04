using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CementoTrazabilidad.Core.Entidades
{
    /// <summary>
    /// Representa un proveedor/marca de bolsas (Coselapa, Primo Tedesco, Bolpar)
    /// </summary>
    public class ProveedorBolsa
    {
        [Key]
        public int ProveedorBolsaID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Descripcion { get; set; }

        [MaxLength(100)]
        public string? RazonSocial { get; set; }

        [MaxLength(20)]
        public string? CUIT { get; set; }

        [MaxLength(200)]
        public string? Contacto { get; set; }

        [MaxLength(100)]
        public string? Telefono { get; set; }

        [MaxLength(200)]
        public string? Email { get; set; }

        public bool Activo { get; set; } = true;

        // Propiedades de navegación
        public virtual ICollection<ConsumoBolsas> Consumos { get; set; } = new List<ConsumoBolsas>();
        public virtual ICollection<LoteProveedorBolsa> Lotes { get; set; } = new List<LoteProveedorBolsa>();
    }
}