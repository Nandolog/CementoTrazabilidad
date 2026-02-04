using System.ComponentModel.DataAnnotations;

namespace CementoTrazabilidad.Shared.DTOs;

public class LoginRequest
{
    [Required(ErrorMessage = "El legajo es requerido")]
    public string Legajo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es requerida")]
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public UsuarioInfo? Usuario { get; set; }
}

public class UsuarioInfo
{
    public int UsuarioID { get; set; }
    public string Legajo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public int PersonalID { get; set; }
}
public class ChangePasswordRequest
{
    [Required(ErrorMessage = "La contraseña actual es requerida")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "La nueva contraseña es requerida")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
    public string NewPassword { get; set; } = string.Empty;
}