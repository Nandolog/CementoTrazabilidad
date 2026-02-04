using CementoTrazabilidad.Core.Entidades;
using CementoTrazabilidad.Core.Interfaces;
using CementoTrazabilidad.Shared.DTOs;  // ← Usar DTOs del Shared
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;  // ← Necesario para [Required]
using System.Security.Claims;

namespace CementoTrazabilidad.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IJwtService _jwtService;

        public AuthController(IAuthService authService, IJwtService jwtService)
        {
            _authService = authService;
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var usuario = await _authService.AuthenticateAsync(request.Legajo, request.Password);

                if (usuario == null)
                    return Unauthorized(new LoginResponse
                    {
                        Success = false,
                        Message = "Legajo o contraseña incorrectos"
                    });

                if (!usuario.Activo)
                    return Unauthorized(new LoginResponse
                    {
                        Success = false,
                        Message = "Usuario inactivo"
                    });

                var token = _jwtService.GenerateToken(usuario);

                // Obtener información del Personal relacionado
                string nombreCompleto = usuario.Legajo; // Valor por defecto

                if (usuario.Personal != null)
                {
                    // Personal solo tiene "Nombre", no "Apellido"
                    nombreCompleto = usuario.Personal.Nombre ?? usuario.Legajo;
                }

                return Ok(new LoginResponse
                {
                    Success = true,
                    Message = "✅ Login exitoso",
                    Token = token,
                    Usuario = new UsuarioInfo
                    {
                        UsuarioID = usuario.UsuarioID,
                        Legajo = usuario.Legajo,
                        Nombre = nombreCompleto,
                        Rol = usuario.RolSistema ?? "Usuario",
                        PersonalID = usuario.PersonalID
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new LoginResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }

        [HttpGet("profile")]
        [Authorize]
        public IActionResult GetProfile()
        {
            var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var legajo = User.FindFirst("Legajo")?.Value;
            var nombre = User.FindFirst(ClaimTypes.Name)?.Value;
            var rol = User.FindFirst(ClaimTypes.Role)?.Value;

            return Ok(new
            {
                usuarioId,
                legajo,
                nombre,
                rol,
                message = "✅ Perfil obtenido correctamente"
            });
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                var result = await _authService.ChangePasswordAsync(
                    usuarioId,
                    request.CurrentPassword,
                    request.NewPassword);

                if (!result)
                    return BadRequest(new { message = "Contraseña actual incorrecta" });

                return Ok(new { message = "✅ Contraseña actualizada exitosamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }

    
}