using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CementoTrazabilidad.Core.Entidades
{
    public class TurnoProduccion
    {
        public int TurnoProduccionID { get; set; }

        [Required]
        public DateOnly Fecha { get; set; }

        [Required]
        [Range(1, 4)]
        public int TurnoNumero { get; set; }

        [Required]
        [MaxLength(50)]
        public string Estado { get; set; } = "Programado";

        public DateTime FechaHoraInicio { get; set; }
        public DateTime? FechaHoraFin { get; set; }

        [MaxLength(500)]
        public string? Observaciones { get; set; }

        // Navigation properties
        public virtual ICollection<PersonalTurno> PersonalTurno { get; set; } = new List<PersonalTurno>();
        public virtual ICollection<ProduccionMaterial> Producciones { get; set; } = new List<ProduccionMaterial>();
        public virtual ICollection<Parada> Paradas { get; set; } = new List<Parada>();
        
        // ✅ NUEVO: Relaciones con sistema de bolsas
        public virtual ICollection<ConsumoBolsas> ConsumosBolsas { get; set; } = new List<ConsumoBolsas>();
        public virtual ICollection<RegistroDefectoBolsa> DefectosBolsas { get; set; } = new List<RegistroDefectoBolsa>();
    }
}