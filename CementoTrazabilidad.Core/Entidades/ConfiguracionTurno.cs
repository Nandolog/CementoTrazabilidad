namespace CementoTrazabilidad.Core.Entidades
{
    public class ConfiguracionTurno
    {
        public int ConfiguracionTurnoID { get; set; }
        public int TurnoNumero { get; set; }
        public DateOnly Fecha { get; set; }
        public bool OverrideActivo { get; set; }
        public string Motivo { get; set; }
        public string UsuarioModifico { get; set; }
        public DateTime FechaModificacion { get; set; }
    }
}