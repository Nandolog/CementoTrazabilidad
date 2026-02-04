using CementoTrazabilidad.Shared.DTOs;

namespace CementoTrazabilidad.Blazor.Services;

public interface IMaterialService
{
    Task<List<MaterialDto>> GetMaterialesAsync(bool? soloActivos = true);
    Task<MaterialDto?> GetMaterialAsync(int id);
    Task<bool> CreateMaterialAsync(CreateMaterialDto request);
    Task<bool> UpdateMaterialAsync(int id, UpdateMaterialDto request);
    Task<bool> DeleteMaterialAsync(int id);
}