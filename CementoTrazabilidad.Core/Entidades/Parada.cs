using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CementoTrazabilidad.Core.Entidades
{
    public class Parada
    {
        public int ParadaID { get; set; }
        public int TurnoProduccionID { get; set; }

        // ✅ Mapear a columna "Tipo" (varchar(30))
        [Column("Tipo")]
        public string TipoParada { get; set; } = string.Empty;

        // ✅ Mapear a columna "Decripcion" (typo en BD) (varchar(200))
        [Column("Decripcion")]
        public string Descripcion { get; set; } = string.Empty;

        public DateTime FechaHoraInicio { get; set; }
        public DateTime? FechaHoraFin { get; set; }

        // ✅ COLUMNA CALCULADA - DatabaseGenerated para que EF Core NO intente escribirla
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int? DuracionMinutos { get; set; }

        // ✅ NUEVOS CAMPOS EN BASE DE DATOS (Quitar NotMapped)
        [Column("MotivoFalla")]
        public string? MotivoFalla { get; set; }

        [Column("AccionCorrectiva")]
        public string? AccionCorrectiva { get; set; }

        [Column("Responsable")]
        public string? Responsable { get; set; }

        // Navigation properties
        public virtual TurnoProduccion Turno { get; set; } = null!;

        // ✅ Propiedades calculadas (NO mapeadas a BD)
        [NotMapped]
        public TimeSpan Duracion => FechaHoraFin.HasValue ?
            FechaHoraFin.Value - FechaHoraInicio :
            TimeSpan.Zero;

        [NotMapped]
        public string Estado => FechaHoraFin.HasValue ? "Finalizada" : "Activa";

        [NotMapped]
        public string ImpactoProductivo => DuracionMinutosCalculado switch
        {
            > 120 => "Alto",
            > 30 => "Medio",
            _ => "Bajo"
        };

        [NotMapped]
        private int DuracionMinutosCalculado => FechaHoraFin.HasValue
            ? (int)(FechaHoraFin.Value - FechaHoraInicio).TotalMinutes
            : 0;

        // ✅ PROPIEDADES QUE SIGUEN SIN EXISTIR EN BD
        [NotMapped]
        public int? PersonalResponsableID { get; set; }

        [NotMapped]
        public virtual Personal? PersonalResponsable { get; set; }
    }
}