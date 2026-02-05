using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using CementoTrazabilidad.Infrastructure.Services;
using System.Security.Claims;

namespace CementoTrazabilidad.API.Authorization;

/// <summary>
/// Atributo para validar que el usuario esté en su turno activo antes de permitir la acción
/// </summary>
public class RequieresTurnoActivoAttribute : TypeFilterAttribute
{
    public RequieresTurnoActivoAttribute() : base(typeof(RequieresTurnoActivoFilter))
    {
    }
}

public class RequieresTurnoActivoFilter : IAsyncActionFilter
{
    private readonly ITurnoValidationService _turnoValidation;
    private readonly ILogger<RequieresTurnoActivoFilter> _logger;

    public RequieresTurnoActivoFilter(
        ITurnoValidationService turnoValidation,
        ILogger<RequieresTurnoActivoFilter> logger)
    {
        _turnoValidation = turnoValidation;
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Obtener usuario actual del JWT
        var usuarioIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(usuarioIdClaim) || !int.TryParse(usuarioIdClaim, out int usuarioId))
        {
            _logger.LogWarning("⚠️ Usuario no autenticado intentó realizar acción");
            context.Result = new UnauthorizedObjectResult(new 
            { 
                success = false, 
                message = "Usuario no autenticado" 
            });
            return;
        }

        // Obtener rol del usuario
        var rol = await _turnoValidation.ObtenerRolUsuarioAsync(usuarioId);
        
        // Administradores siempre pueden
        if (rol == "Administrador")
        {
            _logger.LogInformation($"✅ Administrador (Usuario {usuarioId}) accede sin restricción");
            await next();
            return;
        }

        // Verificar si está en turno activo
        var estaEnTurno = await _turnoValidation.UsuarioEstaEnTurnoActivoAsync(usuarioId);
        
        if (!estaEnTurno)
        {
            var turnoId = await _turnoValidation.ObtenerTurnoActivoDelUsuarioAsync(usuarioId);
            _logger.LogWarning($"❌ Usuario {usuarioId} intentó modificar fuera de su turno. Turno asignado: {turnoId ?? 0}");
            
            context.Result = new ObjectResult(new 
            { 
                success = false, 
                message = "⚠️ No puede realizar esta acción fuera de su turno asignado. Contacte al supervisor si necesita hacer cambios.",
                code = "FUERA_DE_TURNO"
            })
            {
                StatusCode = 403
            };
            return;
        }

        _logger.LogInformation($"✅ Usuario {usuarioId} validado - está en turno activo");
        await next();
    }
}