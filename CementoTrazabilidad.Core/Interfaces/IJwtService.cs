using CementoTrazabilidad.Core.Entidades;
using System.Security.Claims;

namespace CementoTrazabilidad.Core.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(Usuario usuario);
        ClaimsPrincipal? ValidateToken(string token);
    }
}