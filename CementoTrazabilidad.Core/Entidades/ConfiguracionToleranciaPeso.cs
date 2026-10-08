using System;

namespace CementoTrazabilidad.Core.Entidades
{
    public class ConfiguracionToleranciaPeso
    {
        public int ConfiguracionToleranciaPesoID { get; set; }
        public int MaterialID { get; set; }
        public Material Material { get; set; }

        public decimal PesoObjetivo { get; set; }
        public decimal ToleranciaMinima { get; set; }
        public decimal ToleranciaMaxima { get; set; }
        public decimal ToleranciaAjuste { get; set; }

        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}