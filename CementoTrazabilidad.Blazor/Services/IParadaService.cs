using CementoTrazabilidad.Shared.DTOs;

namespace CementoTrazabilidad.Blazor.Services;

public interface IParadaService
{
    Task<List<ParadaDto>> GetParadasTurnoAsync(int turnoId);
    Task<bool> CrearParadaAsync(ParadaDto parada);
    Task<bool> ActualizarParadaAsync(int id, ParadaDto parada);
    Task<bool> EliminarParadaAsync(int id);
    Task<(bool HayCargasActivas, List<string> ZonasActivas)> GetCargasActivasAsync(int turnoId);
    Task<bool> FinalizarParadaAsync(int id, DateTime? fechaHoraFin = null);
}