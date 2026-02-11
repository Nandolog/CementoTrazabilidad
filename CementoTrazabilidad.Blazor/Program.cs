using CementoTrazabilidad.Blazor;
using CementoTrazabilidad.Blazor.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using Blazored.LocalStorage;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// ✅ CORRECCIÓN: Leer URL de la API desde appsettings.json
var apiUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5198/";
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiUrl) });

Console.WriteLine($"🎯 API URL configurada: {apiUrl}");

// Servicios de autenticación
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();


// Servicios de aplicación
builder.Services.AddScoped<IClientAuthService, ClientAuthService>();
builder.Services.AddScoped<ITurnoService, TurnoService>();
builder.Services.AddScoped<IMaterialService, MaterialService>();
builder.Services.AddScoped<IPersonalService, PersonalService>();
builder.Services.AddScoped<IConsumoBolsasService, ConsumoBolsasService>();
builder.Services.AddScoped<ILoteService, LoteService>();
builder.Services.AddScoped<IMetricasService, MetricasService>();
builder.Services.AddScoped<IDespachoService, DespachoService>();
builder.Services.AddScoped<IDateTimeService, DateTimeService>();

builder.RootComponents.Add<CementoTrazabilidad.Blazor.App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

await builder.Build().RunAsync();