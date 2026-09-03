using CementoTrazabilidad.Core.Entidades;
using CementoTrazabilidad.Infrastructure.Data;
using CementoTrazabilidad.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CementoTrazabilidad.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
   // [Authorize]
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
            var query = _context.Materiales.AsQueryable();

            if (activos.HasValue)
                query = query.Where(m => m.Activo == activos.Value);

            var materiales = await query
                .OrderBy(m => m.Codigo)
                .Select(m => new MaterialDto  // ✅ Mapear a DTO
                {
                    MaterialID = m.MaterialID,
                    Codigo = m.Codigo,
                    Descripcion = m.Descripcion,
                    PesoPorBolsa = m.PesoBolsa,  // ✅ Mapear PesoBolsa a PesoPorBolsa
                    DensidadKGm3 = m.DensidadKGm3,
                    Activo = m.Activo
                })
                .ToListAsync();

            return Ok(new { success = true, data = materiales, count = materiales.Count });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var material = await _context.Materiales
                .Where(m => m.MaterialID == id)
                .Select(m => new MaterialDto
                {
                    MaterialID = m.MaterialID,
                    Codigo = m.Codigo,
                    Descripcion = m.Descripcion,
                    PesoPorBolsa = m.PesoBolsa,
                    DensidadKGm3 = m.DensidadKGm3,
                    Activo = m.Activo
                })
                .FirstOrDefaultAsync();

            if (material == null)
                return NotFound(new { success = false, message = $"Material con ID {id} no encontrado" });

            return Ok(new { success = true, data = material });
        }

        [HttpGet("codigo/{codigo}")]
        public async Task<IActionResult> GetByCodigo(string codigo)
        {
            var material = await _context.Materiales
                .Where(m => m.Codigo == codigo)
                .Select(m => new MaterialDto
                {
                    MaterialID = m.MaterialID,
                    Codigo = m.Codigo,
                    Descripcion = m.Descripcion,
                    PesoPorBolsa = m.PesoBolsa,
                    DensidadKGm3 = m.DensidadKGm3,
                    Activo = m.Activo
                })
                .FirstOrDefaultAsync();

            if (material == null)
                return NotFound(new { success = false, message = $"Material con código {codigo} no encontrado" });

            return Ok(new { success = true, data = material });
        }

        [HttpPost]
        [Authorize(Roles = "Administrador,Supervisor")]
        public async Task<IActionResult> Create([FromBody] CreateMaterialDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Datos inválidos", errors = ModelState });

            var existe = await _context.Materiales
                .AnyAsync(m => m.Codigo == dto.Codigo);

            if (existe)
                return BadRequest(new { success = false, message = $"El código {dto.Codigo} ya está registrado" });

            var material = new Material
            {
                Codigo = dto.Codigo,
                Descripcion = dto.Descripcion,
                PesoBolsa = dto.PesoPorBolsa,
                DensidadKGm3 = dto.DensidadKGm3,
                Activo = true
            };

            _context.Materiales.Add(material);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = material.MaterialID }, new
            {
                success = true,
                message = "Material creado exitosamente",
                data = new MaterialDto
                {
                    MaterialID = material.MaterialID,
                    Codigo = material.Codigo,
                    Descripcion = material.Descripcion,
                    PesoPorBolsa = material.PesoBolsa,
                    DensidadKGm3 = material.DensidadKGm3,
                    Activo = material.Activo
                }
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador,Supervisor")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateMaterialDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Datos inválidos", errors = ModelState });

            var material = await _context.Materiales.FindAsync(id);
            if (material == null)
                return NotFound(new { success = false, message = $"Material con ID {id} no encontrado" });

            material.Descripcion = dto.Descripcion;
            material.PesoBolsa = dto.PesoPorBolsa;
            material.DensidadKGm3 = dto.DensidadKGm3;
            material.Activo = dto.Activo;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Material actualizado exitosamente",
                data = new MaterialDto
                {
                    MaterialID = material.MaterialID,
                    Codigo = material.Codigo,
                    Descripcion = material.Descripcion,
                    PesoPorBolsa = material.PesoBolsa,
                    DensidadKGm3 = material.DensidadKGm3,
                    Activo = material.Activo
                }
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int id)
        {
            var material = await _context.Materiales.FindAsync(id);
            if (material == null)
                return NotFound(new { success = false, message = $"Material con ID {id} no encontrado" });

            // Verificar si hay producción asociada
            var tieneProduccion = await _context.ProduccionMaterial
                .AnyAsync(p => p.MaterialID == id);

            if (tieneProduccion)
                return BadRequest(new { success = false, message = "No se puede eliminar material con producción asociada" });

            _context.Materiales.Remove(material);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = $"Material con ID {id} eliminado exitosamente" });
        }

        [HttpGet("{id}/produccion")]
        public async Task<IActionResult> GetProduccionMaterial(int id, [FromQuery] DateTime? desde = null)
        {
            var query = _context.ProduccionMaterial
                .Include(p => p.Turno)
                .Where(p => p.MaterialID == id);

            if (desde.HasValue)
                query = query.Where(p => p.Turno.Fecha >= DateOnly.FromDateTime(desde.Value));

            var produccion = await query
                .OrderByDescending(p => p.Turno.Fecha)
                .ThenByDescending(p => p.Turno.TurnoNumero)
                .ToListAsync();

            var material = await _context.Materiales
                .Where(m => m.MaterialID == id)
                .Select(m => new { m.Codigo, m.Descripcion })
                .FirstOrDefaultAsync();

            return Ok(new
            {
                success = true,
                materialId = id,
                material = material,
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

            var produccion = await _context.ProduccionMaterial
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
                    totalHorasMarcha = g.Sum(p => (double)p.HorasMarcha),
                    promedioBolsasPorHora = g.Sum(p => p.BolsasElaboradas) / (g.Sum(p => (double)p.HorasMarcha) > 0
                        ? g.Sum(p => (double)p.HorasMarcha)
                        : 1)
                })
                .OrderBy(r => r.mes)
                .ToList();

            return Ok(new
            {
                success = true,
                materialId = id,
                año = año,
                totalAnualBolsas = resumenMensual.Sum(r => r.totalBolsasNetas),
                resumenMensual = resumenMensual
            });
        }
    }
}