using CementoTrazabilidad.Core.Entidades;
using System.Threading.Tasks;

namespace CementoTrazabilidad.Core.Interfaces
{
    public interface IAuthService
    {
        Task<Usuario?> AuthenticateAsync(string legajo, string password);
        Task<bool> ChangePasswordAsync(int usuarioId, string currentPassword, string newPassword);
        Task<bool> ResetPasswordByAdminAsync(int personalId, string newPassword); // ⬅️ NUEVO
    }
}