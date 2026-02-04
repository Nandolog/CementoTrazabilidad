
using CementoTrazabilidad.Shared.DTOs;

namespace CementoTrazabilidad.Blazor.Services
{
    public interface IDespachoService
    {
        Task<List<DespachoDTO>> GetDespachosAsync();
        Task<DespachoDTO?> GetDespachoByIdAsync(int id);
        Task<bool> CreateDespachoAsync(CreateDespachoDTO despacho);
        Task<bool> UpdateDespachoAsync(int id, UpdateDespachoDTO despacho);
        Task<bool> DeleteDespachoAsync(int id);
    }
}