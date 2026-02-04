using CementoTrazabilidad.Core.Entidades;
using Microsoft.EntityFrameworkCore;

namespace CementoTrazabilidad.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Verificar si ya hay usuarios
        if (await context.Usuarios.AnyAsync())
        {
            Console.WriteLine("✅ La base de datos ya tiene datos iniciales");
            return;
        }

        Console.WriteLine("🌱 Sembrando datos iniciales...");

        // Crear Personal de prueba
        var personalAdmin = new Personal
        {
            Legajo = "ADMIN001",
            Nombre = "Administrador Sistema",
            Rol = "Administrador",
            Activo = true
        };

        context.Personal.Add(personalAdmin);
        await context.SaveChangesAsync();

        // Crear Usuario Administrador
        var usuario = new Usuario
        {
            Legajo = "ADMIN001",
            PersonalID = personalAdmin.PersonalID,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"), // ✅ Contraseña: admin123
            RolSistema = "Administrador",
            Activo = true,
            FechaCreacion = DateTime.Now
        };

        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();

        Console.WriteLine("✅ Usuario administrador creado:");
        Console.WriteLine("   Legajo: ADMIN001");
        Console.WriteLine("   Contraseña: admin123");
    }
}