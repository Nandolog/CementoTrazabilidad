using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CementoTrazabilidad.Core.Entidades;

namespace CementoTrazabilidad.Core.Interfaces
{
    public interface ITurnoService
    {
        // CRUD básico
        Task<TurnoProduccion> CrearTurnoAsync(TurnoProduccion turno);
        Task<TurnoProduccion?> ObtenerTurnoPorIdAsync(int id);
        Task<IEnumerable<TurnoProduccion>> ObtenerTodosTurnosAsync();
        Task<TurnoProduccion> ActualizarTurnoAsync(TurnoProduccion turno);
        Task<bool> EliminarTurnoAsync(int id);

        // Consultas específicas
        Task<IEnumerable<TurnoProduccion>> ObtenerTurnosPorFechaAsync(DateOnly fecha);
        Task<IEnumerable<TurnoProduccion>> ObtenerTurnosPorRangoAsync(DateOnly inicio, DateOnly fin);
        Task<IEnumerable<TurnoProduccion>> ObtenerTurnosDelDiaAsync();
        Task<IEnumerable<TurnoProduccion>> ObtenerTurnosAbiertosAsync();

        // Gestión de estado
        Task<TurnoProduccion> IniciarTurnoAsync(int turnoId, DateTime fechaHoraInicio);
        Task<bool> RemoverPersonalDeTurnoAsync(int personalTurnoId);
        Task<IEnumerable<PersonalTurno>> ObtenerPersonalPorTurnoAsync(int turnoId);
        Task<IEnumerable<Personal>> ObtenerPersonalDisponibleParaTurnoAsync(int turnoId);

        // Validaciones
        Task<bool> ExisteTurnoParaFechaAsync(DateOnly fecha, int turnoNumero);
        Task<bool> EsTurnoValidoParaProduccionAsync(int turnoId);
    }  // ← Cierre de la interfaz
}  // ← Cierre del namespace
