using CementoTrazabilidad.Core.Entidades;
using CementoTrazabilidad.Infrastructure.Data;
using CementoTrazabilidad.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CementoTrazabilidad.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LotesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<LotesController> _logger;

    public LotesController(ApplicationDbContext context, ILogger<LotesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("turno/{turnoId}")]
    public async Task<ActionResult<List<LoteProduccionDto>>> GetLotesPorTurno(int turnoId)
    {
        try
        {
            var lotes = await _context.LotesProduccion
                .Where(l => l.TurnoID == turnoId) // ← CORREGIDO
                .Select(l => new LoteProduccionDto
                {
                    LoteID = l.LoteID,
                    TurnoID = l.TurnoID, // ← CORREGIDO
                    FechaHoraInicio = l.FechaHoraInicio,
                    FechaHoraFin = l.FechaHoraFin,
                    CantidadBolsas = l.CantidadBolsas,
                    NumeroLote = l.NumeroLote,
                    TipoRegistro = l.TipoRegistro,
                    Observaciones = l.Observaciones,
                    MaterialID = l.MaterialID,
                    MaterialNombre = l.Material != null ? l.Material.Nombre : "Sin material"
                })
                .ToListAsync();

            return Ok(new { success = true, data = lotes });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al obtener lotes para turno {turnoId}");
            return StatusCode(500, new { success = false, message = $"Error: {ex.Message}" });
        }
    }
[HttpPost]
public async Task<ActionResult> CrearLote(CreateLoteProduccionDto dto)
{
    try
    {
        if (!ModelState.IsValid)
            return BadRequest(new { success = false, message = "Datos inválidos" });

        // ✅ LOG MUY VISIBLE
        Console.WriteLine("═══════════════════════════════════════════════════");
        Console.WriteLine($"📥 CREAR LOTE - TurnoID recibido: {dto.TurnoID}");
        Console.WriteLine($"📥 CantidadBolsas: {dto.CantidadBolsas}");
        Console.WriteLine($"📥 MaterialID: {dto.MaterialID}");
        Console.WriteLine("═══════════════════════════════════════════════════");

        _logger.LogInformation($"📥 DTO Recibido - TurnoID: {dto.TurnoID}, MaterialID: {dto.MaterialID}, CantidadBolsas: {dto.CantidadBolsas}");

        // 1. Buscar el turno - VERIFICAR QUE EXISTA
        var turno = await _context.TurnosProduccion
            .FirstOrDefaultAsync(t => t.TurnoProduccionID == dto.TurnoID);

        // ✅ SI NO EXISTE, BUSCAR EL TURNO MÁS RECIENTE "EN PROCESO"
        if (turno == null)
        {
            Console.WriteLine($"⚠️⚠️⚠️ TurnoID {dto.TurnoID} NO ENCONTRADO ⚠️⚠️⚠️");
            _logger.LogWarning($"⚠️ TurnoID {dto.TurnoID} no encontrado. Buscando turno activo...");
            
            turno = await _context.TurnosProduccion
                .Where(t => t.Estado == "En Proceso")
                .OrderByDescending(t => t.TurnoProduccionID)
                .FirstOrDefaultAsync();

            if (turno == null)
            {
                Console.WriteLine("🔍 No hay turnos 'En Proceso', buscando el más reciente...");
                // Si no hay turnos en proceso, usar el más reciente
                turno = await _context.TurnosProduccion
                    .OrderByDescending(t => t.TurnoProduccionID)
                    .FirstOrDefaultAsync();
            }

            if (turno == null)
            {
                Console.WriteLine("❌❌❌ NO HAY TURNOS EN LA BASE DE DATOS ❌❌❌");
                _logger.LogError("❌ No hay ningún turno disponible en la base de datos");
                return BadRequest(new 
                { 
                    success = false, 
                    message = "No hay turnos disponibles. Crea un turno primero."
                });
            }

            Console.WriteLine($"✅ Usando turno alternativo: ID={turno.TurnoProduccionID}, Número={turno.TurnoNumero}, Fecha={turno.Fecha}");
            _logger.LogInformation($"✅ Usando turno alternativo: ID={turno.TurnoProduccionID}, Número={turno.TurnoNumero}, Fecha={turno.Fecha}");
        }
        else
        {
            Console.WriteLine($"✅✅✅ Turno ENCONTRADO: ID={turno.TurnoProduccionID}, Número={turno.TurnoNumero} ✅✅✅");
            _logger.LogInformation($"✅ Turno encontrado: ID={turno.TurnoProduccionID}, Número={turno.TurnoNumero}, Fecha={turno.Fecha}");
        }

        // Usar el TurnoProduccionID del turno encontrado
        int turnoIdFinal = turno.TurnoProduccionID;
        Console.WriteLine($"🎯 TurnoID FINAL que se usará: {turnoIdFinal}");

        // 2. Buscar material
        Material? material = null;

        if (dto.MaterialID > 0)
        {
            material = await _context.Materiales
                .FirstOrDefaultAsync(m => m.MaterialID == dto.MaterialID && m.Activo);
        }

        if (material == null)
        {
            material = await _context.Materiales
                .FirstOrDefaultAsync(m => m.Activo);
        }

        if (material == null)
        {
            material = new Material
            {
                Nombre = dto.MaterialNombre ?? "Cemento Portland",
                Codigo = "CP-001",
                Activo = true,
                PesoBolsa = 50
            };
            _context.Materiales.Add(material);
            await _context.SaveChangesAsync();
        }

        Console.WriteLine($"✅ Material: ID={material.MaterialID}, Nombre={material.Nombre}");

        // 3. Buscar último lote del turno
        var ultimoLote = await _context.LotesProduccion
            .Where(l => l.TurnoID == turnoIdFinal)
            .OrderByDescending(l => l.FechaHoraFin)
            .FirstOrDefaultAsync();

        var fechaInicio = ultimoLote?.FechaHoraFin ?? turno.FechaHoraInicio;
        DateTime? fechaFin = DateTime.Now;

        // 4. Contar lotes para generar número consecutivo
        var conteoLotes = await _context.LotesProduccion
            .CountAsync(l => l.TurnoID == turnoIdFinal);

        // 5. Calcular horas marcha
        decimal? horasMarcha = null;
        if (fechaFin.HasValue)
        {
            horasMarcha = (decimal)(fechaFin.Value - fechaInicio).TotalHours;
        }

        // 6. Crear registro de producción automático
        if (horasMarcha.HasValue)
        {
            var produccion = new ProduccionMaterial
            {
                TurnoProduccionID = turnoIdFinal,
                MaterialID = material.MaterialID,
                BolsasElaboradas = dto.CantidadBolsas,
                BolsasRotas = dto.BolsasRotas,
                HorasMarcha = horasMarcha.Value
            };
            _context.ProduccionMaterial.Add(produccion);
            Console.WriteLine($"✅ ProduccionMaterial agregado");
        }

        // 7. Crear el lote
        var lote = new LoteProduccion
        {
            TurnoID = turnoIdFinal,
            FechaHoraInicio = fechaInicio,
            FechaHoraFin = fechaFin ?? DateTime.Now,
            CantidadBolsas = dto.CantidadBolsas,
            NumeroLote = dto.NumeroLote ?? GenerarNumeroLote(turno, conteoLotes + 1),
            TipoRegistro = dto.TipoRegistro ?? "Manual",
            Observaciones = dto.Observaciones ?? string.Empty,
            MaterialID = material.MaterialID
        };

        Console.WriteLine($"💾💾💾 GUARDANDO LOTE 💾💾💾");
        Console.WriteLine($"   TurnoID: {lote.TurnoID}");
        Console.WriteLine($"   MaterialID: {lote.MaterialID}");
        Console.WriteLine($"   Bolsas: {lote.CantidadBolsas}");
        Console.WriteLine($"   NumeroLote: {lote.NumeroLote}");

        _context.LotesProduccion.Add(lote);
        await _context.SaveChangesAsync();
        
        Console.WriteLine($"✅✅✅ LOTE {lote.LoteID} CREADO EXITOSAMENTE ✅✅✅");
        Console.WriteLine("═══════════════════════════════════════════════════");

        return Ok(new
        {
            success = true,
            message = dto.TurnoID != turnoIdFinal 
                ? $"Lote creado exitosamente. Nota: Se usó el turno {turnoIdFinal} en lugar del {dto.TurnoID} solicitado."
                : "Lote creado exitosamente",
            data = new
            {
                lote.LoteID,
                lote.TurnoID,
                lote.NumeroLote,
                lote.FechaHoraInicio,
                lote.FechaHoraFin,
                lote.CantidadBolsas,
                MaterialID = material.MaterialID,
                MaterialNombre = material.Nombre
            }
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌❌❌ EXCEPCIÓN EN CREAR LOTE ❌❌❌");
        Console.WriteLine($"Mensaje: {ex.Message}");
        Console.WriteLine($"InnerException: {ex.InnerException?.Message}");
        Console.WriteLine("═══════════════════════════════════════════════════");
        
        _logger.LogError(ex, "❌ Error al crear lote");
        return StatusCode(500, new 
        { 
            success = false, 
            message = $"Error interno: {ex.Message}",
            innerException = ex.InnerException?.Message
        });
    }
}

    private string GenerarNumeroLote(TurnoProduccion turno, int consecutivo)
    {
        var fecha = turno.Fecha.ToString("yyyyMMdd");
        return $"T{turno.TurnoNumero}-{fecha}-{consecutivo:D3}";
    }

    [HttpPost("trazabilidad")]
    public async Task<ActionResult<ResultadoTrazabilidadDto>> ConsultarTrazabilidad(ConsultaTrazabilidadDto consulta)
    {
        try
        {
            var tolerancia = consulta.ToleranciaMinutos ?? 5;
            var fechaMin = consulta.FechaHoraImpresa.AddMinutes(-tolerancia);
            var fechaMax = consulta.FechaHoraImpresa.AddMinutes(tolerancia);

            var lote = await _context.LotesProduccion
                .Include(l => l.Material)
                .Where(l => l.FechaHoraInicio <= fechaMax && l.FechaHoraFin >= fechaMin)
                .Select(l => new LoteProduccionDto
                {
                    LoteID = l.LoteID,
                    TurnoID = l.TurnoID, // ← CORREGIDO
                    FechaHoraInicio = l.FechaHoraInicio,
                    FechaHoraFin = l.FechaHoraFin,
                    CantidadBolsas = l.CantidadBolsas,
                    NumeroLote = l.NumeroLote,
                    TipoRegistro = l.TipoRegistro,
                    Observaciones = l.Observaciones,
                    MaterialID = l.MaterialID,
                    MaterialNombre = l.Material != null ? l.Material.Nombre : "Sin material"
                })
                .FirstOrDefaultAsync();

            if (lote == null)
            {
                return Ok(new ResultadoTrazabilidadDto
                {
                    Encontrado = false,
                    TurnoDescripcion = string.Empty,
                    PersonalTurno = new List<string>(),
                    MaquinaUtilizada = string.Empty,
                    MateriaPrimaLote = string.Empty
                });
            }

            // Obtener información adicional del turno
            var turno = await _context.TurnosProduccion
                .Include(t => t.PersonalTurno)
                .ThenInclude(pt => pt.Personal)
                .FirstOrDefaultAsync(t => t.TurnoProduccionID == lote.TurnoID); // ← CAMBIADO

            var personal = turno?.PersonalTurno
                .Select(pt => pt.Personal?.Nombre ?? "Desconocido")
                .ToList() ?? new List<string>();

            return Ok(new ResultadoTrazabilidadDto
            {
                Encontrado = true,
                Lote = lote,
                TurnoDescripcion = turno != null ?
                    $"Turno {turno.TurnoNumero} - {turno.Fecha:dd/MM/yyyy}" :
                    "Turno no encontrado",
                PersonalTurno = personal,
                MaquinaUtilizada = "Máquina principal", // Ajusta según tu sistema
                MateriaPrimaLote = lote.MaterialNombre
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en consulta de trazabilidad");
            return StatusCode(500, new
            {
                success = false,
                message = $"Error: {ex.Message}"
            });
        }
    }

    [HttpGet("{loteId}")]
    public async Task<ActionResult<LoteProduccionDto>> GetLotePorId(int loteId)
    {
        try
        {
            var lote = await _context.LotesProduccion
                .Include(l => l.Material)
                .Where(l => l.LoteID == loteId)
                .Select(l => new LoteProduccionDto
                {
                    LoteID = l.LoteID,
                    TurnoID = l.TurnoID, // ← CORREGIDO
                    FechaHoraInicio = l.FechaHoraInicio,
                    FechaHoraFin = l.FechaHoraFin,
                    CantidadBolsas = l.CantidadBolsas,
                    NumeroLote = l.NumeroLote,
                    TipoRegistro = l.TipoRegistro,
                    Observaciones = l.Observaciones,
                    MaterialID = l.MaterialID,
                    MaterialNombre = l.Material != null ? l.Material.Nombre : "Sin material"
                })
                .FirstOrDefaultAsync();

            if (lote == null)
                return NotFound(new { success = false, message = "Lote no encontrado" });

            return Ok(new { success = true, data = lote });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al obtener lote {loteId}");
            return StatusCode(500, new { success = false, message = $"Error: {ex.Message}" });
        }
    }
}