using CementoTrazabilidad.Infrastructure.Data;
using CementoTrazabilidad.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CementoTrazabilidad.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProveedoresBolsaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProveedoresBolsaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/ProveedoresBolsa
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProveedorBolsaDTO>>> GetProveedoresBolsa()
        {
            var proveedores = await _context.ProveedoresBolsa
                .Where(p => p.Activo)
                .Select(p => new ProveedorBolsaDTO
                {
                    ProveedorBolsaID = p.ProveedorBolsaID,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    Activo = p.Activo
                })
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            return Ok(proveedores);
        }

        // GET: api/ProveedoresBolsa/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProveedorBolsaDTO>> GetProveedorBolsa(int id)
        {
            var proveedor = await _context.ProveedoresBolsa
                .Where(p => p.ProveedorBolsaID == id)
                .Select(p => new ProveedorBolsaDTO
                {
                    ProveedorBolsaID = p.ProveedorBolsaID,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    Activo = p.Activo
                })
                .FirstOrDefaultAsync();

            if (proveedor == null)
            {
                return NotFound();
            }

            return Ok(proveedor);
        }

        // POST: api/ProveedoresBolsa
        [HttpPost]
        public async Task<ActionResult<ProveedorBolsaDTO>> CreateProveedorBolsa(ProveedorBolsaDTO dto)
        {
            var proveedor = new Core.Entidades.ProveedorBolsa
            {
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                Activo = true
            };

            _context.ProveedoresBolsa.Add(proveedor);
            await _context.SaveChangesAsync();

            dto.ProveedorBolsaID = proveedor.ProveedorBolsaID;
            dto.Activo = proveedor.Activo;

            return CreatedAtAction(nameof(GetProveedorBolsa), new { id = proveedor.ProveedorBolsaID }, dto);
        }

        // PUT: api/ProveedoresBolsa/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProveedorBolsa(int id, ProveedorBolsaDTO dto)
        {
            if (id != dto.ProveedorBolsaID)
            {
                return BadRequest();
            }

            var proveedor = await _context.ProveedoresBolsa.FindAsync(id);
            if (proveedor == null)
            {
                return NotFound();
            }

            proveedor.Nombre = dto.Nombre;
            proveedor.Descripcion = dto.Descripcion;
            proveedor.Activo = dto.Activo;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ProveedorBolsaExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/ProveedoresBolsa/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProveedorBolsa(int id)
        {
            var proveedor = await _context.ProveedoresBolsa.FindAsync(id);
            if (proveedor == null)
            {
                return NotFound();
            }

            // Soft delete: solo marcar como inactivo
            proveedor.Activo = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<bool> ProveedorBolsaExists(int id)
        {
            return await _context.ProveedoresBolsa.AnyAsync(p => p.ProveedorBolsaID == id);
        }
    }
}