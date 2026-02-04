namespace CementoTrazabilidad.Core.Entidades;

public class Personal
{
    public int PersonalID { get; set; }
    public string Legajo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;

    // Navigation properties
    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    public virtual ICollection<PersonalTurno> Turnos { get; set; } = new List<PersonalTurno>();
}