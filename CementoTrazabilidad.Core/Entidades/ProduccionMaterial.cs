using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CementoTrazabilidad.Core.Entidades
{
    public class ProduccionMaterial
    {
        public int ProduccionMaterialID { get; set; }
        
        [Required]
        public int TurnoProduccionID { get; set; }
        
        [Required]
        public int MaterialID { get; set; }
        
        [Required]
        public int BolsasElaboradas { get; set; }
        
        public int BolsasRotas { get; set; }
        
        [Required]
        public decimal HorasMarcha { get; set; }

        // Navigation properties
        [ForeignKey(nameof(TurnoProduccionID))]
        public virtual TurnoProduccion Turno { get; set; } = null!;
        
        [ForeignKey(nameof(MaterialID))]
        public virtual Material Material { get; set; } = null!;

        // ✅ NUEVO: Relaciones con el sistema de bolsas
        public virtual ICollection<ConsumoBolsas> ConsumoBolsas { get; set; } = new List<ConsumoBolsas>();
        public virtual ICollection<RegistroDefectoBolsa> DefectosBolsas { get; set; } = new List<RegistroDefectoBolsa>();

        // Calculated properties
        public int BolsasNetas => BolsasElaboradas - BolsasRotas;
    }
}
