using System.ComponentModel.DataAnnotations;

namespace CementoTrazabilidad.Shared.DTOs;

// DTO para restablecer contraseña (por administrador)
public class ResetPasswordDto
{
    [Required(ErrorMessage = "La nueva contraseña es requerida")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres")]
    public string NuevaPassword { get; set; } = string.Empty;
}

public class PersonalDto
{
    public int PersonalID { get; set; }
    public string Legajo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public bool TieneUsuario { get; set; }
}

public class CreatePersonalDto
{
    [Required(ErrorMessage = "El legajo es requerido")]
    [MaxLength(20)]
    public string Legajo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es requerido")]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El rol es requerido")]
    [MaxLength(50)]
    public string Rol { get; set; } = string.Empty;

    // Campos para crear usuario simultáneamente
    public bool CrearUsuario { get; set; } = false;
    
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
    public string? Password { get; set; }
    
    public string? RolSistema { get; set; } // Operario, Supervisor, Administrador
}

public class UpdatePersonalDto
{
    [Required(ErrorMessage = "El legajo es requerido")]
    [MaxLength(20)]
    public string Legajo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es requerido")]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El rol es requerido")]
    [MaxLength(50)]
    public string Rol { get; set; } = string.Empty;

    public bool Activo { get; set; }
}
public class AsignarPersonalDto
{
    public int PersonalId { get; set; }
    public string? RolTurno { get; set; }
}