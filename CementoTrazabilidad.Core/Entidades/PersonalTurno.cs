using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CementoTrazabilidad.Core.Entidades
{
    public class PersonalTurno
    {
        public int PersonalTurnoID { get; set; }
        public int TurnoProduccionID { get; set; }
        public int PersonalID { get; set; }
        public string RolTurno { get; set; } = string.Empty; // "Operario", "Supervisor", "JefeTurno"

        // Navigation properties
        public virtual TurnoProduccion Turno { get; set; } = null!;
        public virtual Personal Personal { get; set; } = null!;
    }
}
