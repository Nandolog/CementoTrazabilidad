using CementoTrazabilidad.Core.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CementoTrazabilidad.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext context, IConfiguration config)
    {
        // Verificar si ya hay usuarios
        if (await context.Usuarios.AnyAsync())
        {
            Console.WriteLine("✅ La base de datos ya tiene datos iniciales");
            return;
        }

        Console.WriteLine("🌱 Sembrando datos iniciales...");

        var adminPassword = config["Seed:AdminPassword"] ?? "admin123";
        var adminLegajo = config["Seed:AdminLegajo"] ?? "ADMIN001";

        // Crear Personal de prueba
        var personalAdmin = new Personal
        {
            Legajo = adminLegajo,
            Nombre = "Administrador Sistema",
            Rol = "Administrador",
            Activo = true
        };

        context.Personal.Add(personalAdmin);
        await context.SaveChangesAsync();

        // Crear Usuario Administrador
        var usuario = new Usuario
        {
            Legajo = adminLegajo,
            PersonalID = personalAdmin.PersonalID,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
            RolSistema = "Administrador",
            Activo = true,
            FechaCreacion = DateTime.Now
        };

        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();

        Console.WriteLine("✅ Usuario administrador creado:");
        Console.WriteLine($"   Legajo: {adminLegajo}");
        Console.WriteLine($"   Contraseña: {adminPassword}");
    }
}