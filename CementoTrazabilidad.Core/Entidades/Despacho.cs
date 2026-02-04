using System;

namespace CementoTrazabilidad.Core.Entidades
{
    public class Despacho
    {
        public int DespachoID { get; set; }
        public int? TurnoProduccionID { get; set; }
        public int? MaterialID { get; set; }
        public string Modalidad { get; set; } = string.Empty;
        public string Destino { get; set; } = string.Empty;
        public int? Bolsas { get; set; }
        public decimal? Toneladas { get; set; }
        public int? Camiones { get; set; }
        public DateTime? FechaHoraDespacho { get; set; }

        // Navigation properties
        public virtual TurnoProduccion? Turno { get; set; }
        // ✅ TEMPORALMENTE COMENTADA para evitar conflicto
        // public virtual Material? Material { get; set; }
    }
}
