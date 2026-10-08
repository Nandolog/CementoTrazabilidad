using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace CementoTrazabilidad.Shared.DTOs
{
    public class CambiarPasswordDto
    {
        [Required(ErrorMessage = "La contraseña actual es obligatoria")]
        public string PasswordActual { get; set; } = string.Empty;

        [Required(ErrorMessage = "La nueva contraseña es obligatoria")]
        [MinLength (8,ErrorMessage = "La nueva contraseña debe tener al menos 8 caracteres")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "La nueva contraseña debe contener al menos una letra mayúscula, una letra minúscula, un número y un carácter especial")]
        public string PasswordNueva { get; set; } = string.Empty;
        [Required(ErrorMessage = "La confirmación de la nueva contraseña es obligatoria")]
        [Compare(nameof(PasswordNueva),ErrorMessage ="Las contraseñas no coinciden")]
        public string ConfirmarPassword { get; set; } = string.Empty;
    }

    public class CambiarPasswordResponseDto
    {
        public bool Success { get; set; }
        public string Mensaje { get; set; } = string.Empty;
    }
}
