using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CementoTrazabilidad.Core.Entidades
{
    public class Material
    {
        public int MaterialID { get; set; }

        [Required, MaxLength(50)]
        public string Codigo { get; set; } = string.Empty;

        // ✅ Esta es la propiedad que se mapea a "descripcion" en la BD
        [Column("descripcion")]
        [Required, MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        // ✅ Descripcion puede ser una propiedad calculada o eliminada
        [NotMapped]
        public string Descripcion
        {
            get => Nombre;
            set => Nombre = value;
        }

        [Column("PesoPorBolsa")]
        public decimal PesoBolsa { get; set; }

        [Column("DensildadKGm3")]
        public decimal DensidadKGm3 { get; set; } = 1500.0m;

        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual ICollection<ProduccionMaterial> Producciones { get; set; } = new List<ProduccionMaterial>();
        public virtual ICollection<Despacho> Despachos { get; set; } = new List<Despacho>();
    }
}