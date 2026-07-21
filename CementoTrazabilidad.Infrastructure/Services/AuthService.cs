using BCrypt.Net;  // ✅ AGREGAR ESTA LÍNEA
using CementoTrazabilidad.Core.Interfaces;
using CementoTrazabilidad.Core.Entidades;
using CementoTrazabilidad.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CementoTrazabilidad.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;

        public AuthService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Usuario?> AuthenticateAsync(string legajo, string password)
        {
            try
            {
                var usuario = await _context.Usuarios
                    .Include(u => u.Personal)
                    .FirstOrDefaultAsync(u => u.Legajo == legajo && u.Activo);

                if (usuario == null)
                {
                    Console.WriteLine($"❌ Usuario no encontrado: {legajo}");
                    return null;
                }

                // ✅ AHORA CON BCrypt
                if (!BCrypt.Net.BCrypt.Verify(password, usuario.PasswordHash))
                {
                    Console.WriteLine($"❌ Contraseña incorrecta para: {legajo}");
                    return null;
                }

                Console.WriteLine($"✅ Autenticación exitosa: {legajo}");

                usuario.FechaUltimoLogin = DateTime.Now;
                await _context.SaveChangesAsync();

                return usuario;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en AuthenticateAsync: {ex.Message}");
                return null;
            }
        }

        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public async Task<bool> ChangePasswordAsync(int usuarioId, string currentPassword, string newPassword)
        {
            try
            {
                var usuario = await _context.Usuarios.FindAsync(usuarioId);
                if (usuario == null)
                    return false;

                if (!BCrypt.Net.BCrypt.Verify(currentPassword, usuario.PasswordHash))
                    return false;

                usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en ChangePasswordAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ResetPasswordByAdminAsync(int personalId, string newPassword)
        {
            try
            {
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.PersonalID == personalId);

                if (usuario == null)
                {
                    Console.WriteLine($"❌ No se encontró usuario para PersonalID: {personalId}");
                    return false;
                }

                usuario.PasswordHash = HashPassword(newPassword);
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ Contraseña restablecida para usuario: {usuario.Legajo}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en ResetPasswordByAdminAsync: {ex.Message}");
                return false;
            }
        }
    }
}