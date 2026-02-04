using CementoTrazabilidad.Core.Entidades;
using CementoTrazabilidad.Infrastructure.Data;
using CementoTrazabilidad.Infrastructure.Services;
using CementoTrazabilidad.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CementoTrazabilidad.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PersonalController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PersonalController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/personal
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var personal = await _context.Personal
                    .OrderBy(p => p.Nombre)
                    .ToListAsync();

                var personalDtos = new List<PersonalDto>();
                foreach (var p in personal)
                {
                    var tieneUsuario = await _context.Usuarios.AnyAsync(u => u.PersonalID == p.PersonalID);
                    personalDtos.Add(new PersonalDto
                    {
                        PersonalID = p.PersonalID,
                        Legajo = p.Legajo,
                        Nombre = p.Nombre,
                        Rol = p.Rol,
                        Activo = p.Activo,
                        TieneUsuario = tieneUsuario
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = personalDtos,
                    count = personalDtos.Count,
                    message = "Personal obtenido exitosamente"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al obtener personal: {ex.Message}"
                });
            }
        }

        // GET: api/personal/activos
        [HttpGet("activos")]
        public async Task<IActionResult> GetActivos()
        {
            try
            {
                var personal = await _context.Personal
                    .Where(p => p.Activo)
                    .OrderBy(p => p.Nombre)
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = personal,
                    count = personal.Count,
                    message = "Personal activo obtenido exitosamente"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al obtener personal activo: {ex.Message}"
                });
            }
        }

        // GET: api/personal/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var personal = await _context.Personal.FindAsync(id);
                if (personal == null)
                    return NotFound(new
                    {
                        success = false,
                        message = $"Personal con ID {id} no encontrado"
                    });

                return Ok(new
                {
                    success = true,
                    data = personal,
                    message = "Personal obtenido exitosamente"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al obtener personal: {ex.Message}"
                });
            }
        }

        // GET: api/personal/legajo/PER001
        [HttpGet("legajo/{legajo}")]
        public async Task<IActionResult> GetByLegajo(string legajo)
        {
            try
            {
                var personal = await _context.Personal
                    .FirstOrDefaultAsync(p => p.Legajo == legajo);

                if (personal == null)
                    return NotFound(new
                    {
                        success = false,
                        message = $"Personal con legajo {legajo} no encontrado"
                    });

                return Ok(new
                {
                    success = true,
                    data = personal,
                    message = "Personal obtenido exitosamente"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al obtener personal: {ex.Message}"
                });
            }
        }

        // POST: api/personal
        [HttpPost]
        [Authorize(Roles = "Administrador,Supervisor")]
        public async Task<IActionResult> Create([FromBody] CreatePersonalDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Datos inválidos",
                        errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                    });
                }

                // Verificar si el legajo ya existe
                var existe = await _context.Personal
                    .AnyAsync(p => p.Legajo == dto.Legajo);

                if (existe)
                    return BadRequest(new
                    {
                        success = false,
                        message = $"El legajo {dto.Legajo} ya está registrado"
                    });

                // Validar: Si quiere crear usuario, password es obligatorio
                if (dto.CrearUsuario && string.IsNullOrWhiteSpace(dto.Password))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "La contraseña es obligatoria para crear usuario"
                    });
                }

                // 1. Crear Personal
                var personal = new Personal
                {
                    Legajo = dto.Legajo,
                    Nombre = dto.Nombre,
                    Rol = dto.Rol,
                    Activo = true
                };

                _context.Personal.Add(personal);
                await _context.SaveChangesAsync();

                // 2. Crear Usuario si se solicitó
                Usuario? usuario = null;
                if (dto.CrearUsuario && !string.IsNullOrWhiteSpace(dto.Password))
                {
                    // Verificar que no exista usuario con ese legajo
                    var usuarioExiste = await _context.Usuarios
                        .AnyAsync(u => u.Legajo == dto.Legajo);

                    if (usuarioExiste)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(new
                        {
                            success = false,
                            message = $"Ya existe un usuario con legajo {dto.Legajo}"
                        });
                    }

                    usuario = new Usuario
                    {
                        PersonalID = personal.PersonalID,
                        Legajo = dto.Legajo,
                        PasswordHash = AuthService.HashPassword(dto.Password),
                        RolSistema = dto.RolSistema ?? "Operario",
                        Activo = true,
                        FechaCreacion = DateTime.Now
                    };

                    _context.Usuarios.Add(usuario);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                var personalDto = new PersonalDto
                {
                    PersonalID = personal.PersonalID,
                    Legajo = personal.Legajo,
                    Nombre = personal.Nombre,
                    Rol = personal.Rol,
                    Activo = personal.Activo,
                    TieneUsuario = usuario != null
                };

                return CreatedAtAction(nameof(GetById), new { id = personal.PersonalID }, new
                {
                    success = true,
                    data = personalDto,
                    usuarioCreado = usuario != null,
                    usuarioId = usuario?.UsuarioID,
                    message = usuario != null
                        ? "Personal y usuario creados exitosamente"
                        : "Personal creado exitosamente (sin usuario)"
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al crear personal: {ex.Message}",
                    innerError = ex.InnerException?.Message
                });
            }
        }

        // PUT: api/personal/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador,Supervisor")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePersonalDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Datos inválidos",
                        errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                    });
                }

                var personal = await _context.Personal.FindAsync(id);
                if (personal == null)
                    return NotFound(new
                    {
                        success = false,
                        message = $"Personal con ID {id} no encontrado"
                    });

                personal.Nombre = dto.Nombre;
                personal.Rol = dto.Rol;
                personal.Activo = dto.Activo;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Personal actualizado exitosamente",
                    data = personal
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al actualizar personal: {ex.Message}"
                });
            }
        }

        // DELETE: api/personal/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var personal = await _context.Personal.FindAsync(id);
                if (personal == null)
                    return NotFound(new
                    {
                        success = false,
                        message = $"Personal con ID {id} no encontrado"
                    });

                // Verificar si tiene producciones registradas (esto sí debe bloquear)
                var tieneProducciones = await _context.TurnosProduccion
                    .AnyAsync(tp => tp.PersonalTurno.Any(pt => pt.PersonalID == id));

                if (tieneProducciones)
                    return BadRequest(new
                    {
                        success = false,
                        message = "No se puede eliminar personal con producciones registradas. Desactive el personal en lugar de eliminarlo."
                    });

                // 1. Eliminar asignaciones de turno
                var asignacionesTurno = await _context.PersonalTurno
                    .Where(pt => pt.PersonalID == id)
                    .ToListAsync();

                if (asignacionesTurno.Any())
                {
                    _context.PersonalTurno.RemoveRange(asignacionesTurno);
                    await _context.SaveChangesAsync();
                }

                // 2. Eliminar usuario asociado si existe
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.PersonalID == id);

                if (usuario != null)
                {
                    _context.Usuarios.Remove(usuario);
                    await _context.SaveChangesAsync();
                }

                // 3. Eliminar personal
                _context.Personal.Remove(personal);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new
                {
                    success = true,
                    message = $"Personal eliminado exitosamente (incluidas {asignacionesTurno.Count} asignaciones de turno)"
                });
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new
                {
                    success = false,
                    message = "No se puede eliminar este personal porque está siendo usado en otros registros",
                    detalle = dbEx.InnerException?.Message
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al eliminar personal: {ex.Message}"
                });
            }
        }

        // PATCH: api/personal/5/toggle-activo
        [HttpPatch("{id}/toggle-activo")]
        [Authorize(Roles = "Administrador,Supervisor")]
        public async Task<IActionResult> ToggleActivo(int id)
        {
            try
            {
                var personal = await _context.Personal.FindAsync(id);
                if (personal == null)
                    return NotFound(new
                    {
                        success = false,
                        message = $"Personal con ID {id} no encontrado"
                    });

                personal.Activo = !personal.Activo;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = $"Estado del personal actualizado a {(personal.Activo ? "Activo" : "Inactivo")}"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al actualizar estado: {ex.Message}"
                });
            }
        }

        // GET: api/personal/count
        [HttpGet("count")]
        public async Task<IActionResult> GetCount()
        {
            try
            {
                var total = await _context.Personal.CountAsync();
                var activos = await _context.Personal.CountAsync(p => p.Activo);
                var inactivos = total - activos;

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        total,
                        activos,
                        inactivos
                    },
                    message = "Conteo obtenido exitosamente"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al obtener conteo: {ex.Message}"
                });
            }
        }

        // GET: api/personal/search?term=nombre
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string term)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Término de búsqueda debe tener al menos 2 caracteres"
                    });
                }

                var personal = await _context.Personal
                    .Where(p => p.Nombre.Contains(term) || p.Legajo.Contains(term) || p.Rol.Contains(term))
                    .OrderBy(p => p.Nombre)
                    .Take(20)
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = personal,
                    count = personal.Count,
                    message = "Búsqueda completada"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error en búsqueda: {ex.Message}"
                });
            }
        }

        // POST: api/personal/5/reset-password
        [HttpPost("{id}/reset-password")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetPasswordDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Datos inválidos",
                        errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                    });
                }

                // Verificar que el personal exista
                var personal = await _context.Personal.FindAsync(id);
                if (personal == null)
                    return NotFound(new
                    {
                        success = false,
                        message = $"Personal con ID {id} no encontrado"
                    });

                // Verificar que tenga usuario
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.PersonalID == id);

                if (usuario == null)
                    return BadRequest(new
                    {
                        success = false,
                        message = $"El personal {personal.Nombre} no tiene usuario asociado"
                    });

                // Resetear contraseña
                usuario.PasswordHash = AuthService.HashPassword(dto.NuevaPassword);
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ Admin restableció contraseña para: {usuario.Legajo}");

                return Ok(new
                {
                    success = true,
                    message = $"Contraseña restablecida exitosamente para {personal.Nombre}",
                    legajo = usuario.Legajo
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al restablecer contraseña: {ex.Message}"
                });
            }
        }
    }
}