namespace CementoTrazabilidad.Core.Entidades
{
    public class ProgramacionProduccion
    {
        public int ProgramacionProduccionID { get; set; }
        public DateOnly Fecha { get; set; }
        public bool Activa { get; set; }
        public string Motivo { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}