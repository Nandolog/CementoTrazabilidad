using CementoTrazabilidad.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CementoTrazabilidad.Infrastructure.Services;

public interface ITurnoValidationService
{
    Task<bool> UsuarioEstaEnTurnoActivoAsync(int usuarioId);
    Task<bool> UsuarioPuedeModificarTurnoAsync(int usuarioId, int turnoId);
    Task<int?> ObtenerTurnoActivoDelUsuarioAsync(int usuarioId);
    Task<string> ObtenerRolUsuarioAsync(int usuarioId);
}

public class TurnoValidationService : ITurnoValidationService
{
    private readonly ApplicationDbContext _context;

    public TurnoValidationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> UsuarioEstaEnTurnoActivoAsync(int usuarioId)
    {
        // Obtener el personal asociado al usuario
        var usuario = await _context.Usuarios
            .Include(u => u.Personal)
            .FirstOrDefaultAsync(u => u.UsuarioID == usuarioId);

        if (usuario?.Personal == null)
            return false;

        // Administradores siempre tienen acceso
        if (usuario.Personal.Rol == "Administrador")
            return true;

        // Verificar si hay un turno activo donde esté asignado
        var hoy = DateOnly.FromDateTime(DateTime.Today);

        var turnoActivo = await _context.TurnosProduccion
            .Where(t => t.Fecha == hoy && t.Estado == "En Proceso")
            .Select(t => t.TurnoProduccionID)
            .FirstOrDefaultAsync();

        if (turnoActivo == 0)
            return false;

        // ✅ CAMBIO: PersonalTurno en lugar de AsignacionPersonalTurno
        var estaAsignado = await _context.PersonalTurno
            .AnyAsync(pt => pt.TurnoProduccionID == turnoActivo 
                         && pt.PersonalID == usuario.PersonalID);

        return estaAsignado;
    }

    public async Task<bool> UsuarioPuedeModificarTurnoAsync(int usuarioId, int turnoId)
    {
        // Obtener usuario con personal
        var usuario = await _context.Usuarios
            .Include(u => u.Personal)
            .FirstOrDefaultAsync(u => u.UsuarioID == usuarioId);

        if (usuario?.Personal == null)
            return false;

        // Administradores siempre pueden modificar
        if (usuario.Personal.Rol == "Administrador")
            return true;

        // ✅ CAMBIO: PersonalTurno en lugar de AsignacionPersonalTurno
        var estaAsignado = await _context.PersonalTurno
            .AnyAsync(pt => pt.TurnoProduccionID == turnoId 
                         && pt.PersonalID == usuario.PersonalID);

        // Verificar también que el turno esté activo
        var turnoActivo = await _context.TurnosProduccion
            .AnyAsync(t => t.TurnoProduccionID == turnoId && t.Estado == "En Proceso");

        return estaAsignado && turnoActivo;
    }

    public async Task<int?> ObtenerTurnoActivoDelUsuarioAsync(int usuarioId)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Personal)
            .FirstOrDefaultAsync(u => u.UsuarioID == usuarioId);

        if (usuario?.Personal == null)
            return null;

        var hoy = DateOnly.FromDateTime(DateTime.Today);

        // ✅ CAMBIO: PersonalTurno en lugar de AsignacionPersonalTurno
        var turnoActivo = await _context.TurnosProduccion
            .Where(t => t.Fecha == hoy && t.Estado == "En Proceso")
            .Join(_context.PersonalTurno,
                t => t.TurnoProduccionID,
                pt => pt.TurnoProduccionID,
                (t, pt) => new { t, pt })
            .Where(x => x.pt.PersonalID == usuario.PersonalID)
            .Select(x => (int?)x.t.TurnoProduccionID)
            .FirstOrDefaultAsync();

        return turnoActivo;
    }

    public async Task<string> ObtenerRolUsuarioAsync(int usuarioId)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Personal)
            .FirstOrDefaultAsync(u => u.UsuarioID == usuarioId);

        return usuario?.Personal?.Rol ?? "Sin Rol";
    }
}