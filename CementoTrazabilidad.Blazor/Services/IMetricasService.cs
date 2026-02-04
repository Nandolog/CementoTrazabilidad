using CementoTrazabilidad.Shared.DTOs;

namespace CementoTrazabilidad.Blazor.Services;

public interface IMetricasService
{
    Task<MetricasTurnoDto?> GetMetricasTurnoAsync(int turnoId);
    Task<List<ParadasDetalladasDto>> GetParadasDetalladasAsync(int turnoId);
    Task<List<DistribucionTiempoDto>> GetDistribucionTiempoAsync(int turnoId);
}