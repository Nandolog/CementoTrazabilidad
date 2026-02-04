using System.ComponentModel.DataAnnotations;

namespace CementoTrazabilidad.Core.Entidades;
public class Usuario
{
    public int UsuarioID { get; set; }
    public int PersonalID { get; set; }

    [Required, MaxLength(20)]
    public string Legajo { get; set; } = string.Empty;

    [Required, MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string RolSistema { get; set; } = "Operario";

    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public DateTime? FechaUltimoLogin { get; set; }

    // Navigation properties
    public virtual Personal Personal { get; set; } = null!;
}