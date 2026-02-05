using CementoTrazabilidad.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using CementoTrazabilidad.Core.Interfaces;
using CementoTrazabilidad.Infrastructure.Repositories;
using CementoTrazabilidad.Infrastructure.Services;
using CementoTrazabilidad.API.Services;  // ✅ AGREGAR ESTA LÍNEA
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddControllers();

// CORS - Configuración ÚNICA
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowDevelopment", policy =>
    {
        policy.WithOrigins(
            "http://localhost:5198",    // API misma
            "http://localhost:5165",    // Blazor
            "http://localhost:5000",    // Blazor dev server
            "https://localhost:5001",   // Blazor HTTPS
            "http://localhost:7000"     // Otro puerto común
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

// Swagger configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CementoTrazabilidad API",
        Version = "v1",
        Description = "API para el sistema de trazabilidad de cemento"
    });

    // Configurar seguridad JWT en Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Introduce tu token JWT así: Bearer {tu_token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IExcelExportService, ExcelExportService>();
builder.Services.AddScoped<ITurnoValidationService, TurnoValidationService>();  // ✅ AGREGAR AQUÍ

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "DefaultKeyMinimum32CharactersLong!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "CementoTrazabilidad.API";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "CementoTrazabilidad.Client";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// ✅ MODIFICAR ESTE BLOQUE
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        db.Database.Migrate();
        Console.WriteLine("✅ Migraciones aplicadas correctamente");
        
        // ✅ AGREGAR ESTA LÍNEA - Sembrar datos iniciales
        await DbInitializer.SeedAsync(db);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error al aplicar migraciones: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "CementoTrazabilidad API v1");
        options.RoutePrefix = "swagger"; // Acceder en /swagger
        options.DocumentTitle = "CementoTrazabilidad API Documentation";
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowDevelopment");  // SOLO UNA política CORS
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Endpoint raíz para evitar 404
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();