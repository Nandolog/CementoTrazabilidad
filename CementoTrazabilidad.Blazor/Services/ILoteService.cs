using CementoTrazabilidad.Shared.DTOs;

namespace CementoTrazabilidad.Blazor.Services;

public interface ILoteService
{
    Task<List<LoteProduccionDto>> GetLotesPorTurnoAsync(int turnoId);
    Task<(bool success, string message)> CrearLoteAsync(CreateLoteProduccionDto dto);
    Task<ResultadoTrazabilidadDto?> BuscarTrazabilidadAsync(ConsultaTrazabilidadDto consulta);
    Task<LoteProduccionDto?> GetLoteByIdAsync(int loteId);
}