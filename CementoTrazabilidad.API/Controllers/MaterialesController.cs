using CementoTrazabilidad.Core.Entidades;
using CementoTrazabilidad.Infrastructure.Data;
using CementoTrazabilidad.Shared.DTOs; // ← Asegúrate de que esta línea esté presente
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CementoTrazabilidad.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MaterialesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MaterialesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool? activos = true)
        {
            var query = _context.Set<Material>().AsQueryable();

            if (activos.HasValue)
                query = query.Where(m => m.Activo == activos.Value);

            var materiales = await query.OrderBy(m => m.Codigo).ToListAsync();
            return Ok(materiales);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var material = await _context.Set<Material>().FindAsync(id);
            if (material == null)
                return NotFound(new { message = $"Material con ID {id} no encontrado" });

            return Ok(material);
        }

        [HttpGet("codigo/{codigo}")]
        public async Task<IActionResult> GetByCodigo(string codigo)
        {
            var material = await _context.Set<Material>()
                .FirstOrDefaultAsync(m => m.Codigo == codigo);

            if (material == null)
                return NotFound(new { message = $"Material con código {codigo} no encontrado" });

            return Ok(material);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador,Supervisor")]
        public async Task<IActionResult> Create([FromBody] CreateMaterialDto dto)
        {
            var existe = await _context.Set<Material>()
                .AnyAsync(m => m.Codigo == dto.Codigo);

            if (existe)
                return BadRequest(new { message = $"El código {dto.Codigo} ya está registrado" });

            var material = new Material
            {
                Codigo = dto.Codigo,
                Descripcion = dto.Descripcion,
                PesoBolsa = dto.PesoPorBolsa,
                DensidadKGm3 = dto.DensidadKGm3, // ← AGREGAR
                Activo = true
            };

            _context.Set<Material>().Add(material);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = material.MaterialID }, material);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador,Supervisor")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateMaterialDto dto)
        {
            var material = await _context.Set<Material>().FindAsync(id);
            if (material == null)
                return NotFound(new { message = $"Material con ID {id} no encontrado" });

            material.Descripcion = dto.Descripcion;
            material.PesoBolsa = dto.PesoPorBolsa;
            material.DensidadKGm3 = dto.DensidadKGm3; // ← AGREGAR
            material.Activo = dto.Activo;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Material actualizado exitosamente", material });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int id)
        {
            var material = await _context.Set<Material>().FindAsync(id);
            if (material == null)
                return NotFound(new { message = $"Material con ID {id} no encontrado" });

            // Verificar si hay producción asociada
            var tieneProduccion = await _context.Set<ProduccionMaterial>()
                .AnyAsync(p => p.MaterialID == id);

            if (tieneProduccion)
                return BadRequest(new { message = "No se puede eliminar material con producción asociada" });

            _context.Set<Material>().Remove(material);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Material con ID {id} eliminado exitosamente" });
        }

        [HttpGet("{id}/produccion")]
        public async Task<IActionResult> GetProduccionMaterial(int id, [FromQuery] DateTime? desde = null)
        {
            // CORRECCIÓN: Buscar por MaterialID, no TurnoProduccionID
            var query = _context.Set<ProduccionMaterial>()
                .Include(p => p.Turno)  // ← Se llama Turno (propiedad de navegación)
                .Where(p => p.MaterialID == id);  // ← Filtrar por MaterialID

            if (desde.HasValue)
                query = query.Where(p => p.Turno.Fecha >= DateOnly.FromDateTime(desde.Value));

            var produccion = await query
                .OrderByDescending(p => p.Turno.Fecha)
                .ThenByDescending(p => p.Turno.TurnoNumero)
                .ToListAsync();

            return Ok(new
            {
                materialId = id,
                material = await _context.Set<Material>()
                    .Where(m => m.MaterialID == id)
                    .Select(m => new { m.Codigo, m.Descripcion }) // ✅ Usar Descripcion en lugar de Nombre
                    .FirstOrDefaultAsync(),
                totalBolsasElaboradas = produccion.Sum(p => p.BolsasElaboradas),
                totalBolsasRotas = produccion.Sum(p => p.BolsasRotas),
                totalBolsasNetas = produccion.Sum(p => p.BolsasElaboradas - p.BolsasRotas),
                totalHorasMarcha = produccion.Sum(p => p.HorasMarcha),
                produccion = produccion.Select(p => new
                {
                    p.ProduccionMaterialID,
                    p.BolsasElaboradas,
                    p.BolsasRotas,
                    p.BolsasNetas,
                    p.HorasMarcha,
                    turno = new
                    {
                        p.Turno.TurnoProduccionID,
                        p.Turno.Fecha,
                        p.Turno.TurnoNumero,
                        p.Turno.Estado
                    }
                })
            });
        }

        [HttpGet("{id}/resumen-mensual")]
        public async Task<IActionResult> GetResumenMensual(int id, [FromQuery] int? año = null)
        {
            año ??= DateTime.Now.Year;

            var produccion = await _context.Set<ProduccionMaterial>()
                .Include(p => p.Turno)
                .Where(p => p.MaterialID == id && p.Turno.Fecha.Year == año)
                .ToListAsync();

            var resumenMensual = produccion
                .GroupBy(p => p.Turno.Fecha.Month)
                .Select(g => new
                {
                    mes = g.Key,
                    nombreMes = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key),
                    totalBolsasElaboradas = g.Sum(p => p.BolsasElaboradas),
                    totalBolsasRotas = g.Sum(p => p.BolsasRotas),
                    totalBolsasNetas = g.Sum(p => p.BolsasElaboradas - p.BolsasRotas),
                    totalHorasMarcha = g.Sum(p => p.HorasMarcha),
                    promedioBolsasPorHora = g.Sum(p => p.BolsasElaboradas) / (g.Sum(p => p.HorasMarcha) > 0 ? g.Sum(p => p.HorasMarcha) : 1)
                })
                .OrderBy(r => r.mes)
                .ToList();

            return Ok(new
            {
                materialId = id,
                año = año,
                totalAnualBolsas = resumenMensual.Sum(r => r.totalBolsasNetas),
                resumenMensual = resumenMensual
            });
        }
    }
}