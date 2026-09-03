using CementoTrazabilidad.API.Authorization;
using CementoTrazabilidad.Core.Entidades;
using CementoTrazabilidad.Infrastructure.Data;
using CementoTrazabilidad.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CementoTrazabilidad.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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
                    MaterialNombre = c.ProduccionMaterial != null && c.ProduccionMaterial.Material != null
                ? c.ProduccionMaterial.Material.Nombre
                : "Sin material",
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
                    MaterialNombre = c.ProduccionMaterial != null && c.ProduccionMaterial.Material != null
                ? c.ProduccionMaterial.Material.Nombre
                : "Sin material",
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
        [RequieresTurnoActivo]
        public async Task<ActionResult<ConsumoBolsasDTO>> CreateConsumoBolsas(ConsumoBolsasCreateDTO dto)
        {
            // ✅ Si no se proporciona ProduccionMaterialID, buscar o crear uno
            int? produccionMaterialId = dto.ProduccionMaterialID;

            if (!produccionMaterialId.HasValue)
            {
                // ✅ SELECCIONAR MATERIAL SEGÚN TIPOCEMENTO
                Material? material = null;

                if (!string.IsNullOrEmpty(dto.TipoCemento))
                {
                    // Buscar material por el TipoCemento
                    material = dto.TipoCemento switch
                    {
                        "C32" => await _context.Materiales
                            .FirstOrDefaultAsync(m => m.Codigo.Contains("C32") || m.Nombre.Contains("C32")),
                        "F40" => await _context.Materiales
                            .FirstOrDefaultAsync(m => m.Codigo.Contains("F40") || m.Nombre.Contains("F40")),
                        _ => await _context.Materiales.FirstOrDefaultAsync(m => m.Activo)
                    };
                }

                // Si no se encontró material por TipoCemento, usar el primer material activo
                if (material == null)
                {
                    material = await _context.Materiales.FirstOrDefaultAsync(m => m.Activo);
                }

                if (material == null)
                    return BadRequest(new { success = false, message = "No hay materiales disponibles" });

                // ✅ Buscar producción existente con el material correcto
                var produccion = await _context.ProduccionMaterial
                    .FirstOrDefaultAsync(p => p.TurnoProduccionID == dto.TurnoProduccionID
                                               && p.MaterialID == material.MaterialID);

                if (produccion != null)
                {
                    produccionMaterialId = produccion.ProduccionMaterialID;
                }
                else
                {
                    // ✅ Crear nueva producción con el material correcto
                    var nuevaProduccion = new Core.Entidades.ProduccionMaterial
                    {
                        TurnoProduccionID = dto.TurnoProduccionID,
                        MaterialID = material.MaterialID,
                        BolsasElaboradas = dto.CantidadBolsas,
                        BolsasRotas = dto.BolsasDefectuosas,
                        HorasMarcha = 0
                    };
                    _context.ProduccionMaterial.Add(nuevaProduccion);
                    await _context.SaveChangesAsync();
                    produccionMaterialId = nuevaProduccion.ProduccionMaterialID;
                }
            }

            var consumo = new Core.Entidades.ConsumoBolsas
            {
                ProveedorBolsaID = dto.ProveedorBolsaID,
                TurnoProduccionID = dto.TurnoProduccionID,
                ProduccionMaterialID = produccionMaterialId,
                CantidadBolsas = dto.CantidadBolsas,
                BolsasDefectuosas = dto.BolsasDefectuosas,
                LoteBolsa = dto.LoteBolsa,
                TipoCemento = dto.TipoCemento,
                Observaciones = dto.Observaciones,
                FechaConsumo = DateTime.UtcNow
            };

            _context.ConsumoBolsas.Add(consumo);
            await _context.SaveChangesAsync();

            // Obtener el resultado con el material
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
                    MaterialNombre = c.ProduccionMaterial != null && c.ProduccionMaterial.Material != null
                        ? c.ProduccionMaterial.Material.Nombre
                        : "Sin material",
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
        [RequieresTurnoActivo]
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