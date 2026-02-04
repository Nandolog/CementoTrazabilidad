using System.Collections.Generic;
using System.Threading.Tasks;
using CementoTrazabilidad.Shared.DTOs;

namespace CementoTrazabilidad.Blazor.Services
{
    public interface ITurnoService
    {
        // Métodos existentes
        Task<List<TurnoDto>> GetTurnosDelDiaAsync(DateOnly fecha);
        Task<TurnoDto?> GetTurnoActualAsync();
        Task<TurnoDetalleDto?> GetTurnoDetalleAsync(int turnoId);
        Task<bool> IniciarTurnoAsync(CreateTurnoDto request);  // Para crear e iniciar
        Task<bool> FinalizarTurnoAsync(int turnoId);
        Task<bool> RegistrarProduccionAsync(CreateProduccionDto request);
        Task<bool> RegistrarParadaAsync(CreateParadaDto request);
        Task<List<TurnoDto>> GetTurnosPorRangoAsync(DateOnly inicio, DateOnly fin);

        // Nuevos métodos necesarios
        Task<List<TurnoDto>> GetTurnosAsync();
        Task<List<TurnoDto>> GetTurnosAbiertosAsync();
        Task<TurnoDto?> GetTurnoByIdAsync(int id);
        Task<TurnoResumenDto?> GetTurnoResumenAsync(int id);
        Task<bool> CreateTurnoAsync(CreateTurnoDto turno);
        Task<bool> IniciarTurnoAsync(int turnoId);  // Sobrecarga para iniciar por ID
        Task<bool> AsignarPersonalAsync(int turnoId, AsignarPersonalDto asignacion);
        Task<List<PersonalTurnoDto>> GetPersonalTurnoAsync(int turnoId);
        Task<bool> RemoverPersonalAsync(int turnoId, int personalId);
        Task<List<ParadaDto>> GetParadasTurnoAsync(int turnoId);
        Task<TurnoProduccionDto?> GetTurnoActivoAsync();
    }
}