using Microsoft.AspNetCore.Mvc;
using CementoTrazabilidad.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using CementoTrazabilidad.Core.Entidades;
using Microsoft.AspNetCore.Authorization;
using System.Security.Cryptography;
using System.Text;

namespace CementoTrazabilidad.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TestController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("crear-usuario-admin")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateAdminUser()
        {
            try
            {
                Console.WriteLine("🔍 Iniciando creación de usuario administrador...");

                // 1. Verificar si YA EXISTE un personal con legajo ADMIN001
                var personal = await _context.Personal
                    .FirstOrDefaultAsync(p => p.Legajo == "ADMIN001");

                if (personal == null)
                {
                    Console.WriteLine("📝 Creando nuevo personal ADMIN001...");
                    // Usar las propiedades REALES según tu clase
                    personal = new Personal
                    {
                        Legajo = "ADMIN001",
                        Nombre = "Administrador Sistema",
                        Rol = "Administrador",
                        Activo = true
                        // NO hay: Apellido, DNI, FechaAlta
                    };
                    _context.Personal.Add(personal);
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"✅ Personal creado con ID: {personal.PersonalID}");
                }
                else
                {
                    Console.WriteLine($"✅ Personal ya existe con ID: {personal.PersonalID}");
                }

                // 2. Verificar si YA EXISTE el usuario ADMIN001
                var usuarioExistente = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Legajo == "ADMIN001");

                if (usuarioExistente != null)
                {
                    Console.WriteLine($"⚠️ Usuario ADMIN001 ya existe. Actualizando contraseña a hash...");

                    // Actualizar contraseña existente a hash
                    var updatedHashedPassword = HashPassword("operario123");
                    usuarioExistente.PasswordHash = updatedHashedPassword;
                    await _context.SaveChangesAsync();

                    return Ok(new
                    {
                        success = true,
                        message = "✅ Usuario ADMIN001 actualizado con hash",
                        credentials = "ADMIN001 / operario123"
                    });
                }

                Console.WriteLine("📝 Creando nuevo usuario ADMIN001...");

                // 3. Crear HASH de la contraseña
                var hashedPassword = HashPassword("operario123");

                // 4. Crear NUEVO usuario
                var usuario = new Usuario
                {
                    PersonalID = personal.PersonalID,
                    Legajo = "ADMIN001",
                    PasswordHash = hashedPassword,
                    RolSistema = "Administrador",
                    Activo = true,
                    FechaCreacion = DateTime.Now
                };

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ Usuario creado con ID: {usuario.UsuarioID}");

                return Ok(new
                {
                    success = true,
                    message = "✅ Usuario administrador creado exitosamente",
                    credentials = "ADMIN001 / operario123",
                    role = "Administrador",
                    personalId = personal.PersonalID,
                    usuarioId = usuario.UsuarioID,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
                Console.WriteLine($"❌ StackTrace: {ex.StackTrace}");

                return StatusCode(500, new
                {
                    success = false,
                    error = ex.Message,
                    innerError = ex.InnerException?.Message,
                    timestamp = DateTime.Now
                });
            }
        }

        // Método auxiliar para hashear (mismo que en AuthService)
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        [HttpPost("convertir-todos-a-hash")]
        [AllowAnonymous]
        public async Task<IActionResult> ConvertirTodosUsuariosHash()
        {
            try
            {
                var usuarios = await _context.Usuarios.ToListAsync();
                int convertidos = 0;

                foreach (var usuario in usuarios)
                {
                    // Si la contraseña NO parece ser un hash (los hashes SHA256 Base64 son 44 chars)
                    if (!string.IsNullOrEmpty(usuario.PasswordHash) &&
                        usuario.PasswordHash.Length < 40)
                    {
                        Console.WriteLine($"🔧 Convirtiendo password de {usuario.Legajo}...");
                        usuario.PasswordHash = HashPassword(usuario.PasswordHash);
                        convertidos++;
                    }
                }

                if (convertidos > 0)
                {
                    await _context.SaveChangesAsync();
                }

                return Ok(new
                {
                    success = true,
                    message = $"✅ {convertidos} usuarios convertidos a hash SHA256",
                    totalUsuarios = usuarios.Count,
                    convertidos = convertidos
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }

        [HttpGet("simple-test")]
        [AllowAnonymous]
        public IActionResult SimpleTest()
        {
            return Ok(new
            {
                success = true,
                message = "✅ TestController funcionando correctamente",
                controller = "TestController",
                timestamp = DateTime.Now
            });
        }

        [HttpGet("public")]
        [AllowAnonymous]
        public IActionResult PublicInfo()
        {
            return Ok(new
            {
                success = true,
                message = "✅ Este es un endpoint público",
                service = "CementoTrazabilidad API",
                version = "1.0.0",
                timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("database")]
        [Authorize]
        public async Task<IActionResult> TestDatabase()
        {
            try
            {
                var legajo = User.FindFirst("Legajo")?.Value ?? "Desconocido";
                var usuariosCount = await _context.Usuarios.CountAsync();
                var personalCount = await _context.Personal.CountAsync();

                return Ok(new
                {
                    success = true,
                    message = $"✅ Conexión exitosa (Usuario: {legajo})",
                    database = _context.Database.GetDbConnection().Database,
                    server = _context.Database.GetDbConnection().DataSource,
                    usuariosCount,
                    personalCount,
                    authenticatedUser = legajo,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    error = ex.Message,
                    timestamp = DateTime.Now
                });
            }
        }
    }
}