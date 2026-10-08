using CementoTrazabilidad.Core.Entidades;
using CementoTrazabilidad.Core.Interfaces;
using CementoTrazabilidad.Infrastructure.Data;
using CementoTrazabilidad.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace CementoTrazabilidad.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IJwtService _jwtService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthService authService,
            IJwtService jwtService,
            ApplicationDbContext context,
            ILogger<AuthController> logger)
        {
            _authService = authService;
            _jwtService = jwtService;
            _context = context;
            _logger = logger;
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

                string nombreCompleto = usuario.Legajo;

                if (usuario.Personal != null)
                {
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

        // ============================================================
        // ✅ ENDPOINT: CAMBIAR CONTRASEÑA (USUARIO AUTENTICADO)
        // ============================================================
        [HttpPost("cambiar-password")]
        [Authorize]
        public async Task<IActionResult> CambiarPassword([FromBody] CambiarPasswordDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // 1. Obtener el usuario autenticado desde el token JWT
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                               ?? User.FindFirst("userId")?.Value
                               ?? User.FindFirst("sub")?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    _logger.LogWarning("⚠️ No se pudo obtener el ID del usuario autenticado del token");
                    return Unauthorized(new { success = false, message = "Usuario no identificado" });
                }

                // 2. Buscar el usuario en la base de datos
                var usuario = await _context.Usuarios.FindAsync(userId);
                if (usuario == null)
                {
                    _logger.LogWarning($"⚠️ Usuario {userId} no encontrado");
                    return NotFound(new { success = false, message = "Usuario no encontrado" });
                }

                // 3. Verificar la contraseña actual
                bool passwordCorrecta = BCrypt.Net.BCrypt.Verify(dto.PasswordActual, usuario.PasswordHash);

                if (!passwordCorrecta)
                {
                    _logger.LogWarning($"⚠️ Contraseña actual incorrecta para usuario {usuario.Legajo}");
                    return BadRequest(new { success = false, message = "La contraseña actual es incorrecta" });
                }

                // 4. Validar que la nueva contraseña sea diferente a la actual
                if (BCrypt.Net.BCrypt.Verify(dto.PasswordNueva, usuario.PasswordHash))
                {
                    return BadRequest(new { success = false, message = "La nueva contraseña debe ser diferente a la actual" });
                }

                // 5. Actualizar la contraseña con BCrypt
                usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.PasswordNueva);

                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Contraseña cambiada exitosamente para usuario {usuario.Legajo}");

                return Ok(new
                {
                    success = true,
                    message = "Contraseña cambiada exitosamente. Use su nueva contraseña en el próximo inicio de sesión."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al cambiar contraseña");
                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
            }
        }

        // ============================================================
        // ✅ ENDPOINT: GENERAR HASH (SOLO DESARROLLO)
        // ============================================================
        [HttpGet("generate-hash")]
        [AllowAnonymous]
        public IActionResult GenerateHash([FromQuery] string password = "Admin123!")
        {
            try
            {
                string hash = BCrypt.Net.BCrypt.HashPassword(password);

                return Ok(new
                {
                    success = true,
                    password = password,
                    hash = hash,
                    message = "✅ Copia este hash y actualiza la base de datos"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }
    }
}