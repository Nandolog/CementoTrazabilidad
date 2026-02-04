using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CementoTrazabilidad.Core.Entidades
{
    public class Material
    {
        public int MaterialID { get; set; }

        [Required, MaxLength(50)]
        public string Codigo { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(255)]
        public string Descripcion { get; set; } = string.Empty;

        public decimal PesoBolsa { get; set; } // en kg
        public decimal DensidadKGm3 { get; set; } = 1500.0m; // ← AGREGAR con valor por defecto
        public string UnidadMedida { get; set; } = "kg";
        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual ICollection<ProduccionMaterial> Producciones { get; set; } = new List<ProduccionMaterial>();
        public virtual ICollection<Despacho> Despachos { get; set; } = new List<Despacho>();
    }

}
