using CementoTrazabilidad.Core.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using CementoTrazabilidad.Core.Entidades;

namespace CementoTrazabilidad.Core.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<Usuario?> ObtenerPorLegajoAsync(string legajo);
        Task<Usuario?> ObtenerPorIdAsync(int usuarioID);
        Task ActualizarAsync(Usuario usuario);
    }
}