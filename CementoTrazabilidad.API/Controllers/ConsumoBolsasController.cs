using CementoTrazabilidad.Infrastructure.Data;
using CementoTrazabilidad.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CementoTrazabilidad.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConsumoBolsasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ConsumoBolsasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/ConsumoBolsas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ConsumoBolsasDTO>>> GetConsumoBolsas()
        {
            var consumos = await _context.ConsumoBolsas
                .Include(c => c.ProveedorBolsa)
                .Include(c => c.ProduccionMaterial)
                    .ThenInclude(p => p!.Material)
                .Select(c => new ConsumoBolsasDTO
                {
                    ConsumoBolsasID = c.ConsumoBolsasID,
                    ProveedorBolsaID = c.ProveedorBolsaID,
                    ProveedorNombre = c.ProveedorBolsa.Nombre,
                    TurnoProduccionID = c.TurnoProduccionID,
                    ProduccionMaterialID = c.ProduccionMaterialID,
                    MaterialDescripcion = c.ProduccionMaterial != null ? c.ProduccionMaterial.Material.Descripcion : null,
                    CantidadBolsas = c.CantidadBolsas,
                    BolsasDefectuosas = c.BolsasDefectuosas,
                    FechaConsumo = c.FechaConsumo,
                    LoteBolsa = c.LoteBolsa,
                    TipoCemento = c.TipoCemento,
                    Observaciones = c.Observaciones
                })
                .OrderByDescending(c => c.FechaConsumo)
                .ToListAsync();

            return Ok(consumos);
        }

        // GET: api/ConsumoBolsas/turno/5
        [HttpGet("turno/{turnoId}")]
        public async Task<ActionResult<IEnumerable<ConsumoBolsasDTO>>> GetConsumosByTurno(int turnoId)
        {
            var consumos = await _context.ConsumoBolsas
                .Where(c => c.TurnoProduccionID == turnoId)
                .Include(c => c.ProveedorBolsa)
                .Include(c => c.ProduccionMaterial)
                    .ThenInclude(p => p!.Material)
                .Select(c => new ConsumoBolsasDTO
                {
                    ConsumoBolsasID = c.ConsumoBolsasID,
                    ProveedorBolsaID = c.ProveedorBolsaID,
                    ProveedorNombre = c.ProveedorBolsa.Nombre,
                    TurnoProduccionID = c.TurnoProduccionID,
                    ProduccionMaterialID = c.ProduccionMaterialID,
                    MaterialDescripcion = c.ProduccionMaterial != null ? c.ProduccionMaterial.Material.Descripcion : null,
                    CantidadBolsas = c.CantidadBolsas,
                    BolsasDefectuosas = c.BolsasDefectuosas,
                    FechaConsumo = c.FechaConsumo,
                    LoteBolsa = c.LoteBolsa,
                    TipoCemento = c.TipoCemento,
                    Observaciones = c.Observaciones
                })
                .ToListAsync();

            return Ok(consumos);
        }

        // POST: api/ConsumoBolsas
        [HttpPost]
        public async Task<ActionResult<ConsumoBolsasDTO>> CreateConsumoBolsas(ConsumoBolsasCreateDTO dto)
        {
            var consumo = new Core.Entidades.ConsumoBolsas
            {
                ProveedorBolsaID = dto.ProveedorBolsaID,
                TurnoProduccionID = dto.TurnoProduccionID,
                ProduccionMaterialID = dto.ProduccionMaterialID,
                CantidadBolsas = dto.CantidadBolsas,
                BolsasDefectuosas = dto.BolsasDefectuosas,
                LoteBolsa = dto.LoteBolsa,
                TipoCemento = dto.TipoCemento,
                Observaciones = dto.Observaciones,
                FechaConsumo = DateTime.UtcNow
            };

            _context.ConsumoBolsas.Add(consumo);
            await _context.SaveChangesAsync();

            var result = await _context.ConsumoBolsas
                .Where(c => c.ConsumoBolsasID == consumo.ConsumoBolsasID)
                .Include(c => c.ProveedorBolsa)
                .Include(c => c.ProduccionMaterial)
                    .ThenInclude(p => p!.Material)
                .Select(c => new ConsumoBolsasDTO
                {
                    ConsumoBolsasID = c.ConsumoBolsasID,
                    ProveedorBolsaID = c.ProveedorBolsaID,
                    ProveedorNombre = c.ProveedorBolsa.Nombre,
                    TurnoProduccionID = c.TurnoProduccionID,
                    ProduccionMaterialID = c.ProduccionMaterialID,
                    MaterialDescripcion = c.ProduccionMaterial != null ? c.ProduccionMaterial.Material.Descripcion : null,
                    CantidadBolsas = c.CantidadBolsas,
                    BolsasDefectuosas = c.BolsasDefectuosas,
                    FechaConsumo = c.FechaConsumo,
                    LoteBolsa = c.LoteBolsa,
                    TipoCemento = c.TipoCemento,
                    Observaciones = c.Observaciones
                })
                .FirstOrDefaultAsync();

            return CreatedAtAction(nameof(GetConsumoBolsas), new { id = consumo.ConsumoBolsasID }, result);
        }

        // DELETE: api/ConsumoBolsas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConsumoBolsas(int id)
        {
            var consumo = await _context.ConsumoBolsas.FindAsync(id);
            if (consumo == null)
            {
                return NotFound();
            }

            _context.ConsumoBolsas.Remove(consumo);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}