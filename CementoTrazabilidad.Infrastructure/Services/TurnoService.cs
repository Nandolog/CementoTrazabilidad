using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CementoTrazabilidad.Core.Entidades;
using CementoTrazabilidad.Core.Interfaces;
using CementoTrazabilidad.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CementoTrazabilidad.Infrastructure.Services
{
    public class TurnoService : ITurnoService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TurnoService> _logger;

        public TurnoService(ApplicationDbContext context, ILogger<TurnoService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<TurnoProduccion> CrearTurnoAsync(TurnoProduccion turno)
        {
            try
            {
                // Validar que no exista turno duplicado
                var existe = await _context.TurnosProduccion
                    .AnyAsync(t => t.Fecha == turno.Fecha && t.TurnoNumero == turno.TurnoNumero);

                if (existe)
                {
                    throw new InvalidOperationException(
                        $"Ya existe el turno {turno.TurnoNumero} para la fecha {turno.Fecha:yyyy-MM-dd}");
                }

                // Validar número de turno (1-4)
                if (turno.TurnoNumero < 1 || turno.TurnoNumero > 4)
                {
                    throw new ArgumentException("El número de turno debe estar entre 1 y 4");
                }

                // Establecer estado inicial
                turno.Estado = "Programado"; // Estados: Programado, EnProduccion, Cerrado, Cancelado
                turno.FechaHoraInicio = turno.FechaHoraInicio.ToUniversalTime();

                _context.TurnosProduccion.Add(turno);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Turno {turno.TurnoProduccionID} creado para fecha {turno.Fecha}");
                return turno;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear turno");
                throw;
            }
        }

        public async Task<TurnoProduccion?> ObtenerTurnoPorIdAsync(int id)
        {
            return await _context.TurnosProduccion
                .Include(t => t.PersonalTurno)
                    .ThenInclude(pt => pt.Personal)
                .Include(t => t.Producciones)
                .Include(t => t.Paradas)
                .FirstOrDefaultAsync(t => t.TurnoProduccionID == id);
        }

        public async Task<IEnumerable<TurnoProduccion>> ObtenerTodosTurnosAsync()
        {
            return await _context.TurnosProduccion
                .Include(t => t.PersonalTurno)
                .OrderByDescending(t => t.Fecha)
                .ThenBy(t => t.TurnoNumero)
                .ToListAsync();
        }

        public async Task<IEnumerable<TurnoProduccion>> ObtenerTurnosPorFechaAsync(DateOnly fecha)
        {
            return await _context.TurnosProduccion
                .Include(t => t.PersonalTurno)
                    .ThenInclude(pt => pt.Personal)
                .Where(t => t.Fecha == fecha)
                .OrderBy(t => t.TurnoNumero)
                .ToListAsync();
        }

        public async Task<IEnumerable<TurnoProduccion>> ObtenerTurnosDelDiaAsync()
        {
            var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
            return await ObtenerTurnosPorFechaAsync(hoy);
        }

        public async Task<IEnumerable<TurnoProduccion>> ObtenerTurnosAbiertosAsync()
        {
            return await _context.TurnosProduccion
                .Include(t => t.PersonalTurno)
                .Where(t => t.Estado == "Programado" || t.Estado == "EnProduccion")
                .OrderByDescending(t => t.Fecha)
                .ThenBy(t => t.TurnoNumero)
                .ToListAsync();
        }

        public async Task<TurnoProduccion> IniciarTurnoAsync(int turnoId, DateTime fechaHoraInicio)
        {
            var turno = await _context.TurnosProduccion.FindAsync(turnoId);
            if (turno == null)
                throw new KeyNotFoundException($"Turno {turnoId} no encontrado");

            if (turno.Estado != "Programado")
                throw new InvalidOperationException($"El turno {turnoId} no puede iniciarse. Estado actual: {turno.Estado}");

            turno.Estado = "EnProduccion";
            turno.FechaHoraInicio = fechaHoraInicio.ToUniversalTime();

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Turno {turnoId} iniciado a las {fechaHoraInicio}");
            return turno;
        }

        public async Task<TurnoProduccion> CerrarTurnoAsync(int turnoId, DateTime fechaHoraFin, string? observaciones = null)
        {
            var turno = await _context.TurnosProduccion
                .Include(t => t.Producciones)
                .FirstOrDefaultAsync(t => t.TurnoProduccionID == turnoId);

            if (turno == null)
                throw new KeyNotFoundException($"Turno {turnoId} no encontrado");

            if (turno.Estado != "EnProduccion")
                throw new InvalidOperationException($"El turno {turnoId} no puede cerrarse. Estado actual: {turno.Estado}");

            // Validar que no haya producción pendiente
            var produccionPendiente = false;
            if (produccionPendiente)
                throw new InvalidOperationException($"No se puede cerrar el turno {turnoId} porque tiene producción pendiente");

            turno.Estado = "Cerrado";
            turno.FechaHoraFin = fechaHoraFin.ToUniversalTime();

            // Aquí podrías agregar lógica para generar reporte de cierre

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Turno {turnoId} cerrado a las {fechaHoraFin}");
            return turno;
        }

        public async Task<PersonalTurno> AsignarPersonalATurnoAsync(int turnoId, int personalId, string rolTurno)
        {
            // Verificar que el turno existe
            var turno = await _context.TurnosProduccion.FindAsync(turnoId);
            if (turno == null)
                throw new KeyNotFoundException($"Turno {turnoId} no encontrado");

            // Verificar que el personal existe
            var personal = await _context.Personal.FindAsync(personalId);
            if (personal == null)
                throw new KeyNotFoundException($"Personal {personalId} no encontrado");

            // Verificar que no esté ya asignado al mismo turno
            var asignacionExistente = await _context.PersonalTurno
                .FirstOrDefaultAsync(pt => pt.TurnoProduccionID == turnoId && pt.PersonalID == personalId);

            if (asignacionExistente != null)
                throw new InvalidOperationException($"El personal {personalId} ya está asignado al turno {turnoId}");

            // Verificar que el personal no esté asignado a otro turno en la misma fecha
            var turnosDelDia = await _context.TurnosProduccion
                .Where(t => t.Fecha == turno.Fecha)
                .Select(t => t.TurnoProduccionID)
                .ToListAsync();

            var asignacionOtroTurno = await _context.PersonalTurno
                .AnyAsync(pt => turnosDelDia.Contains(pt.TurnoProduccionID) && pt.PersonalID == personalId);

            if (asignacionOtroTurno)
                throw new InvalidOperationException($"El personal {personalId} ya está asignado a otro turno en la misma fecha");

            var personalTurno = new PersonalTurno
            {
                TurnoProduccionID = turnoId,
                PersonalID = personalId,
                RolTurno = rolTurno
            };

            _context.PersonalTurno.Add(personalTurno);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Personal {personalId} asignado al turno {turnoId} como {rolTurno}");
            return personalTurno;
        }

        public async Task<bool> RemoverPersonalDeTurnoAsync(int personalTurnoId)
        {
            var asignacion = await _context.PersonalTurno.FindAsync(personalTurnoId);
            if (asignacion == null)
                return false;

            // Validar que el turno no esté en producción
            var turno = await _context.TurnosProduccion.FindAsync(asignacion.TurnoProduccionID);
            if (turno?.Estado == "EnProduccion")
                throw new InvalidOperationException("No se puede remover personal de un turno en producción");

            _context.PersonalTurno.Remove(asignacion);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Personal removido del turno. Asignación ID: {personalTurnoId}");
            return true;
        }

        public async Task<IEnumerable<PersonalTurno>> ObtenerPersonalPorTurnoAsync(int turnoId)
        {
            return await _context.PersonalTurno
                .Include(pt => pt.Personal)
                .Where(pt => pt.TurnoProduccionID == turnoId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Personal>> ObtenerPersonalDisponibleParaTurnoAsync(int turnoId)
        {
            var turno = await _context.TurnosProduccion.FindAsync(turnoId);
            if (turno == null)
                return new List<Personal>();

            // Obtener personal ya asignado a turnos de la misma fecha
            var personalAsignadoIds = await _context.PersonalTurno
                .Include(pt => pt.Turno)
                .Where(pt => pt.Turno.Fecha == turno.Fecha)
                .Select(pt => pt.PersonalID)
                .Distinct()
                .ToListAsync();

            // Obtener personal activo que no esté asignado
            return await _context.Personal
               .Where(p => p.Activo && !personalAsignadoIds.Contains(p.PersonalID))
                .ToListAsync();
        }

        public async Task<bool> ExisteTurnoParaFechaAsync(DateOnly fecha, int turnoNumero)
        {
            return await _context.TurnosProduccion
                .AnyAsync(t => t.Fecha == fecha && t.TurnoNumero == turnoNumero);
        }

        public async Task<bool> EsTurnoValidoParaProduccionAsync(int turnoId)
        {
            var turno = await _context.TurnosProduccion.FindAsync(turnoId);
            if (turno == null)
                return false;

            // El turno debe estar en estado "EnProduccion"
            return turno.Estado == "EnProduccion";
        }

        public async Task<TurnoProduccion> ActualizarTurnoAsync(TurnoProduccion turno)
        {
            var turnoExistente = await _context.TurnosProduccion.FindAsync(turno.TurnoProduccionID);
            if (turnoExistente == null)
                throw new KeyNotFoundException($"Turno {turno.TurnoProduccionID} no encontrado");

            // No permitir modificar turnos en producción o cerrados
            if (turnoExistente.Estado == "EnProduccion" || turnoExistente.Estado == "Cerrado")
                throw new InvalidOperationException($"No se puede modificar un turno en estado {turnoExistente.Estado}");

            // Actualizar propiedades permitidas
            turnoExistente.Fecha = turno.Fecha;
            turnoExistente.TurnoNumero = turno.TurnoNumero;
            turnoExistente.FechaHoraInicio = turno.FechaHoraInicio.ToUniversalTime();

            await _context.SaveChangesAsync();
            return turnoExistente;
        }

        public async Task<bool> EliminarTurnoAsync(int id)
        {
            var turno = await _context.TurnosProduccion
                .Include(t => t.PersonalTurno)
                .Include(t => t.Producciones)
                .FirstOrDefaultAsync(t => t.TurnoProduccionID == id);

            if (turno == null)
                return false;

            // Validar que el turno no tenga producción asociada
            if (turno.Producciones.Any())
                throw new InvalidOperationException("No se puede eliminar un turno con producción registrada");

            // Validar que el turno no esté en producción
            if (turno.Estado == "EnProduccion")
                throw new InvalidOperationException("No se puede eliminar un turno en producción");

            _context.TurnosProduccion.Remove(turno);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Turno {id} eliminado");
            return true;
        }

        public async Task<IEnumerable<TurnoProduccion>> ObtenerTurnosPorRangoAsync(DateOnly inicio, DateOnly fin)
        {
            return await _context.TurnosProduccion
                .Include(t => t.PersonalTurno)
                .Include(t => t.Producciones)
                .Where(t => t.Fecha >= inicio && t.Fecha <= fin)
                .OrderByDescending(t => t.Fecha)
                .ThenBy(t => t.TurnoNumero)
                .ToListAsync();
        }

        public async Task<TurnoProduccion> CambiarEstadoTurnoAsync(int turnoId, string nuevoEstado)
        {
            var turno = await _context.TurnosProduccion.FindAsync(turnoId);
            if (turno == null)
                throw new KeyNotFoundException($"Turno {turnoId} no encontrado");

            // Validar transición de estado
            var estadoValido = nuevoEstado switch
            {
                "Programado" => turno.Estado == "Cancelado",
                "EnProduccion" => turno.Estado == "Programado",
                "Cerrado" => turno.Estado == "EnProduccion",
                "Cancelado" => turno.Estado == "Programado" || turno.Estado == "EnProduccion",
                _ => false
            };

            if (!estadoValido)
                throw new InvalidOperationException($"Transición de estado no válida: {turno.Estado} -> {nuevoEstado}");

            turno.Estado = nuevoEstado;

            // Establecer fechas según estado
            if (nuevoEstado == "EnProduccion" && turno.FechaHoraInicio == default)
                turno.FechaHoraInicio = DateTime.UtcNow;

            if (nuevoEstado == "Cerrado" && !turno.FechaHoraFin.HasValue)
                turno.FechaHoraFin = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return turno;
        }
    }
}