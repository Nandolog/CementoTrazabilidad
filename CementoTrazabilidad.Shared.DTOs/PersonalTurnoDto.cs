namespace CementoTrazabilidad.Shared.DTOs
{
    public class PersonalTurnoDto
    {
        public int PersonalTurnoID { get; set; }
        public int TurnoProduccionID { get; set; }
        public int PersonalID { get; set; }
        public string RolTurno { get; set; }
        public string PersonalNombre { get; set; }
        public string PersonalLegajo { get; set; }
        public string RolPersonal { get; set; }
        public bool Activo { get; set; }
    }
}