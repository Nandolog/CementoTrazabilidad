// Archivo: CementoTrazabilidad.Infrastructure/Data/DesignTimeDbContextFactory.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace CementoTrazabilidad.Infrastructure.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Obtener la ruta del directorio actual
            var basePath = Directory.GetCurrentDirectory();
            Console.WriteLine($"Directorio actual: {basePath}");

            // Buscar appsettings.json (puede estar en el proyecto API)
            var appsettingsPath = Path.Combine(basePath, "appsettings.json");

            // Si no existe aquí, buscar en el directorio padre (API project)
            if (!File.Exists(appsettingsPath))
            {
                appsettingsPath = Path.Combine(basePath, "..", "CementoTrazabilidad.API", "appsettings.json");
            }

            if (!File.Exists(appsettingsPath))
            {
                throw new FileNotFoundException($"No se encontró appsettings.json. Buscado en: {appsettingsPath}");
            }

            Console.WriteLine($"Usando appsettings.json en: {appsettingsPath}");

            // Configurar la construcción del DbContext
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(appsettingsPath))
                .AddJsonFile(Path.GetFileName(appsettingsPath), optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.Development.json", optional: true)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                // Intentar con nombre alternativo
                connectionString = configuration.GetConnectionString("CementoTrazabilidadConnection");

                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException(
                        "No se encontró la cadena de conexión. Buscó: 'DefaultConnection' y 'CementoTrazabilidadConnection'");
                }
            }

            Console.WriteLine($"Cadena de conexión encontrada: {connectionString.Substring(0, Math.Min(50, connectionString.Length))}...");

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}