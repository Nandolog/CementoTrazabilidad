using CementoTrazabilidad.Shared.DTOs;

namespace CementoTrazabilidad.Blazor.Services;

public interface IPersonalService
{
    Task<List<PersonalDto>> GetPersonalAsync();
    Task<List<PersonalDto>> GetPersonalActivoAsync();
    Task<PersonalDto?> GetPersonalByIdAsync(int id);
    Task<PersonalDto?> GetPersonalByLegajoAsync(string legajo);
    Task<List<PersonalDto>> SearchPersonalAsync(string term);
    Task<bool> CreatePersonalAsync(CreatePersonalDto request);
    Task<bool> UpdatePersonalAsync(int id, UpdatePersonalDto request);
    Task<(bool success, string message)> DeletePersonalAsync(int id);
    Task<(bool success, string message)> ToggleActivoAsync(int id);
    Task<bool> ActualizarEstadoPersonalAsync(int personalId, bool nuevoEstado);
    Task<(bool success, string message)> ResetPasswordAsync(int personalId, ResetPasswordDto request); // ⬅️ NUEVO
}