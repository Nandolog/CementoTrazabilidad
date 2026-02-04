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
    public class TurnosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TurnosController> _logger;

        public TurnosController(ApplicationDbContext context, ILogger<TurnosController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ✅ CORREGIDO: Crear turno con estado "Programado"
        [HttpPost]
        [Authorize(Roles = "Administrador,Supervisor")]
        public async Task<ActionResult<TurnoDto>> Create([FromBody] CreateTurnoDto dto)
        {
            try
            {
                _logger.LogInformation($"📥 Creando turno: Fecha={dto.Fecha}, Turno={dto.TurnoNumero}");

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // ✅ CORRECCIÓN: Usar TurnosProduccion (plural) como está en DbContext
                var existe = await _context.TurnosProduccion
                    .AnyAsync(t => t.Fecha == dto.Fecha && t.TurnoNumero == dto.TurnoNumero);

                if (existe)
                {
                    _logger.LogWarning($"❌ Turno duplicado: Turno {dto.TurnoNumero} para {dto.Fecha}");
                    return BadRequest(new
                    {
                        success = false,
                        message = $"Ya existe un turno {dto.TurnoNumero} para la fecha {dto.Fecha:yyyy-MM-dd}"
                    });
                }

                // ✅ CORRECTO: Crear con estado "Programado"
                var turno = new TurnoProduccion
                {
                    Fecha = dto.Fecha,
                    TurnoNumero = dto.TurnoNumero,
                    Estado = "Programado",
                    FechaHoraInicio = default(DateTime),
                    FechaHoraFin = null
                };

                // ✅ CORRECCIÓN: Usar TurnosProduccion (plural)
                _context.TurnosProduccion.Add(turno);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Turno creado: ID={turno.TurnoProduccionID}, Estado={turno.Estado}");

                // Asignar personal si se proporciona
                if (dto.PersonalIds != null && dto.PersonalIds.Any())
                {
                    _logger.LogInformation($"   Asignando {dto.PersonalIds.Count} personal...");
                    foreach (var personalId in dto.PersonalIds)
                    {
                        var personalExiste = await _context.Personal.AnyAsync(p => p.PersonalID == personalId);
                        if (!personalExiste)
                        {
                            _logger.LogWarning($"   Personal ID {personalId} no encontrado, omitiendo...");
                            continue;
                        }

                        var personalTurno = new PersonalTurno
                        {
                            TurnoProduccionID = turno.TurnoProduccionID,
                            PersonalID = personalId,
                            RolTurno = "Operario"
                        };
                        _context.PersonalTurno.Add(personalTurno);
                    }
                    await _context.SaveChangesAsync();
                }

                var turnoDto = new TurnoDto
                {
                    TurnoProduccionID = turno.TurnoProduccionID,
                    Fecha = turno.Fecha,
                    TurnoNumero = turno.TurnoNumero,
                    Estado = turno.Estado,
                    FechaHoraInicio = turno.FechaHoraInicio,
                    FechaHoraFin = turno.FechaHoraFin
                };

                return CreatedAtAction(nameof(GetById),
                    new { id = turno.TurnoProduccionID },
                    new { success = true, data = turnoDto, message = "Turno creado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR al crear turno");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error interno del servidor",
                    error = ex.Message
                });
            }
        }

        // ✅ CORREGIDO: Iniciar turno - De "Programado" a "En Proceso"
        [HttpPut("{id}/iniciar")]
        [Authorize(Roles = "Administrador,Supervisor,JefeTurno")]
        public async Task<IActionResult> IniciarTurno(int id)
        {
            try
            {
                _logger.LogInformation($"▶️ Iniciando turno {id}");

                var turno = await _context.TurnosProduccion.FindAsync(id);
                if (turno == null)
                {
                    _logger.LogWarning($"❌ Turno {id} no encontrado");
                    return NotFound(new { success = false, message = $"Turno con ID {id} no encontrado" });
                }

                if (turno.Estado != "Programado")
                {
                    _logger.LogWarning($"❌ Turno {id} no puede iniciarse. Estado actual: {turno.Estado}");
                    return BadRequest(new
                    {
                        success = false,
                        message = $"El turno no puede iniciarse. Estado actual: {turno.Estado}",
                        estadosPermitidos = new[] { "Programado" }
                    });
                }

                // ✅ Asignar hora de inicio REAL
                turno.Estado = "En Proceso";
                turno.FechaHoraInicio = DateTime.Now;
                
                // ✅ Calcular hora de fin esperada según el turno
                turno.FechaHoraFin = CalcularHoraFinEsperada(turno.Fecha, turno.TurnoNumero);

                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Turno {id} iniciado. Real: {turno.FechaHoraInicio:HH:mm}, Esperado: {turno.FechaHoraFin:HH:mm}");
                
                return Ok(new
                {
                    success = true,
                    message = $"✅ Turno {turno.TurnoNumero} iniciado correctamente",
                    data = new
                    {
                        turnoId = turno.TurnoProduccionID,
                        turnoNumero = turno.TurnoNumero,
                        estado = turno.Estado,
                        fechaHoraInicio = turno.FechaHoraInicio,
                        fechaHoraFinEsperada = turno.FechaHoraFin,
                        horario = ObtenerHorarioTexto(turno.TurnoNumero)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ ERROR al iniciar turno {id}");
                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
            }
        }

        // ✅ CORREGIDO: Finalizar turno - De "En Proceso" a "Finalizado"
        [HttpPut("{id}/finalizar")]
        [Authorize(Roles = "Administrador,Supervisor,JefeTurno")]
        public async Task<IActionResult> FinalizarTurno(int id, [FromBody] FinalizarTurnoDto? dto = null)
        {
            try
            {
                _logger.LogInformation($"⏹️ Finalizando turno {id}");

                var turno = await _context.TurnosProduccion.FindAsync(id);
                if (turno == null)
                {
                    _logger.LogWarning($"❌ Turno {id} no encontrado");
                    return NotFound(new { success = false, message = $"Turno con ID {id} no encontrado" });
                }

                // ✅ VALIDACIÓN CORRECTA: Solo se puede finalizar si está "En Proceso"
                if (turno.Estado != "En Proceso")
                {
                    _logger.LogWarning($"❌ Turno {id} no puede finalizarse. Estado actual: {turno.Estado}");
                    return BadRequest(new
                    {
                        success = false,
                        message = $"El turno no puede finalizarse. Estado actual: {turno.Estado}",
                        estadosPermitidos = new[] { "En Proceso" }
                    });
                }

                // Cambiar estado y registrar hora de fin
                turno.Estado = "Finalizado";
                turno.FechaHoraFin = dto?.FechaHoraFin ?? DateTime.Now;
                if (!string.IsNullOrEmpty(dto?.Observaciones))
                {
                    turno.Observaciones = dto.Observaciones;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Turno {id} finalizado: {turno.FechaHoraFin:HH:mm}");
                return Ok(new
                {
                    success = true,
                    message = "Turno finalizado exitosamente",
                    data = new
                    {
                        turnoId = turno.TurnoProduccionID,
                        estado = turno.Estado,
                        fechaHoraFin = turno.FechaHoraFin
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ ERROR al finalizar turno {id}");
                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
            }
        }

        // ✅ CORREGIDO: Asignar personal
        [HttpPost("{id}/asignar-personal")]
        [Authorize(Roles = "Administrador,Supervisor,JefeTurno")]
        public async Task<IActionResult> AsignarPersonal(int id, [FromBody] AsignarPersonalDto dto)
        {
            try
            {
                _logger.LogInformation($"👥 Asignando personal {dto.PersonalId} al turno {id}");

                var turno = await _context.TurnosProduccion.FindAsync(id);
                if (turno == null)
                {
                    _logger.LogWarning($"❌ Turno {id} no encontrado");
                    return NotFound(new { success = false, message = $"Turno con ID {id} no encontrado" });
                }

                // ✅ VALIDACIÓN CORRECTA: Solo se puede asignar personal si el turno está "Programado" o "En Proceso"
                if (turno.Estado == "Finalizado")
                {
                    _logger.LogWarning($"❌ No se puede asignar personal a un turno finalizado");
                    return BadRequest(new
                    {
                        success = false,
                        message = "No se puede asignar personal a un turno finalizado",
                        estadoActual = turno.Estado
                    });
                }

                var personal = await _context.Personal.FindAsync(dto.PersonalId);
                if (personal == null)
                {
                    _logger.LogWarning($"❌ Personal {dto.PersonalId} no encontrado");
                    return NotFound(new { success = false, message = $"Personal con ID {dto.PersonalId} no encontrado" });
                }

                // Verificar si ya está asignado
                var yaAsignado = await _context.PersonalTurno
                    .AnyAsync(pt => pt.TurnoProduccionID == id && pt.PersonalID == dto.PersonalId);

                if (yaAsignado)
                {
                    _logger.LogWarning($"❌ Personal {dto.PersonalId} ya está asignado al turno {id}");
                    return BadRequest(new { success = false, message = "El personal ya está asignado a este turno" });
                }

                // Crear asignación
                var personalTurno = new PersonalTurno
                {
                    TurnoProduccionID = id,
                    PersonalID = dto.PersonalId,
                    RolTurno = dto.RolTurno ?? "Operario"
                };

                _context.PersonalTurno.Add(personalTurno);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Personal {dto.PersonalId} asignado al turno {id} (Asignación ID: {personalTurno.PersonalTurnoID})");
                return Ok(new
                {
                    success = true,
                    message = "Personal asignado exitosamente",
                    data = new
                    {
                        asignacionId = personalTurno.PersonalTurnoID,
                        personalNombre = personal.Nombre,
                        rolTurno = personalTurno.RolTurno
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ ERROR al asignar personal al turno {id}");
                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
            }
        }

        // ✅ Método para obtener personal del turno
        [HttpGet("{id}/personal")]
        public async Task<ActionResult<IEnumerable<PersonalTurnoDto>>> GetPersonalTurno(int id)
        {
            try
            {
                _logger.LogInformation($"🔍 Obteniendo personal para turno {id}");

                var personal = await _context.PersonalTurno
                    .Include(pt => pt.Personal)
                    .Where(pt => pt.TurnoProduccionID == id)
                    .ToListAsync();

                var personalDtos = personal.Select(pt => new PersonalTurnoDto
                {
                    PersonalTurnoID = pt.PersonalTurnoID,
                    TurnoProduccionID = pt.TurnoProduccionID,
                    PersonalID = pt.PersonalID,
                    RolTurno = pt.RolTurno ?? "Operario",
                    PersonalNombre = pt.Personal?.Nombre ?? "No disponible",
                    PersonalLegajo = pt.Personal?.Legajo ?? "N/A",
                    RolPersonal = pt.Personal?.Rol ?? "N/A",
                    Activo = pt.Personal?.Activo ?? false
                }).ToList();

                _logger.LogInformation($"✅ Personal obtenido: {personalDtos.Count} personas");
                return Ok(new { success = true, data = personalDtos, count = personalDtos.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ ERROR en GetPersonalTurno para turno {id}");
                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
            }
        }

        // ✅ Método para obtener todos los turnos con filtros
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TurnoDto>>> GetAll(
            [FromQuery] DateOnly? fecha = null,
            [FromQuery] int? turnoNumero = null,
            [FromQuery] string? estado = null)
        {
            try
            {
                _logger.LogInformation($"🔍 Obteniendo turnos (Fecha={fecha}, Turno={turnoNumero}, Estado={estado})");

                var query = _context.TurnosProduccion.AsQueryable();

                if (fecha.HasValue)
                    query = query.Where(t => t.Fecha == fecha.Value);

                if (turnoNumero.HasValue)
                    query = query.Where(t => t.TurnoNumero == turnoNumero.Value);

                if (!string.IsNullOrEmpty(estado))
                    query = query.Where(t => t.Estado == estado);

                var turnos = await query
                    .OrderByDescending(t => t.Fecha)
                    .ThenBy(t => t.TurnoNumero)
                    .ToListAsync();

                var turnosDto = turnos.Select(t => new TurnoDto
                {
                    TurnoProduccionID = t.TurnoProduccionID,
                    Fecha = t.Fecha,
                    TurnoNumero = t.TurnoNumero,
                    Estado = t.Estado,
                    FechaHoraInicio = t.FechaHoraInicio,
                    FechaHoraFin = t.FechaHoraFin
                }).ToList();

                _logger.LogInformation($"✅ Turnos obtenidos: {turnosDto.Count}");
                return Ok(new { success = true, data = turnosDto, count = turnosDto.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR al obtener turnos");
                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
            }
        }

        // ✅ Método para obtener un turno por ID
        [HttpGet("{id}")]
        public async Task<ActionResult<TurnoDto>> GetById(int id)
        {
            try
            {
                _logger.LogInformation($"🔍 Obteniendo turno {id}");

                var turno = await _context.TurnosProduccion.FindAsync(id);
                if (turno == null)
                {
                    _logger.LogWarning($"❌ Turno {id} no encontrado");
                    return NotFound(new { success = false, message = $"Turno con ID {id} no encontrado" });
                }

                var turnoDto = new TurnoDto
                {
                    TurnoProduccionID = turno.TurnoProduccionID,
                    Fecha = turno.Fecha,
                    TurnoNumero = turno.TurnoNumero,
                    Estado = turno.Estado,
                    FechaHoraInicio = turno.FechaHoraInicio,
                    FechaHoraFin = turno.FechaHoraFin
                };

                _logger.LogInformation($"✅ Turno {id} obtenido (Estado: {turno.Estado})");
                return Ok(new { success = true, data = turnoDto });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ ERROR al obtener turno {id}");
                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
            }
        }

        // ✅ Método para obtener resumen
        [HttpGet("{id}/resumen")]
        public async Task<ActionResult<TurnoResumenDto>> GetResumen(int id)
        {
            try
            {
                var turno = await _context.TurnosProduccion
                    .Include(t => t.Producciones)
                    .Include(t => t.Paradas)
                    .Include(t => t.PersonalTurno)
                    .FirstOrDefaultAsync(t => t.TurnoProduccionID == id);

                if (turno == null)
                    return NotFound(new { success = false, message = $"Turno con ID {id} no encontrado" });

                var resumen = new TurnoResumenDto
                {
                    TurnoProduccionID = turno.TurnoProduccionID,
                    Fecha = turno.Fecha,
                    TurnoNumero = turno.TurnoNumero,
                    Estado = turno.Estado,
                    FechaHoraInicio = turno.FechaHoraInicio,
                    FechaHoraFin = turno.FechaHoraFin,
                    CantidadPersonal = turno.PersonalTurno?.Count ?? 0,
                    TotalParadas = turno.Paradas?.Count ?? 0
                };

                return Ok(new { success = true, data = resumen });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ ERROR al obtener resumen del turno {id}");
                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
            }
        }

        // ✅ NUEVO: Registrar producción
        [HttpPost("{id}/produccion")]
        [Authorize(Roles = "Administrador,Supervisor,JefeTurno,Operario")]
        public async Task<IActionResult> RegistrarProduccion(int id, [FromBody] CreateProduccionDto dto)
        {
            try
            {
                _logger.LogInformation($"📦 Registrando producción para turno {id}");

                var turno = await _context.TurnosProduccion.FindAsync(id);
                if (turno == null)
                {
                    _logger.LogWarning($"❌ Turno {id} no encontrado");
                    return NotFound(new { success = false, message = $"Turno con ID {id} no encontrado" });
                }

                // Validar que el turno esté en proceso
                if (turno.Estado != "En Proceso")
                {
                    _logger.LogWarning($"❌ El turno {id} no está en proceso. Estado: {turno.Estado}");
                    return BadRequest(new
                    {
                        success = false,
                        message = "Solo se puede registrar producción en turnos que están en proceso",
                        estadoActual = turno.Estado
                    });
                }

                // Validar que el material existe
                var material = await _context.Materiales.FindAsync(dto.MaterialID);
                if (material == null)
                {
                    _logger.LogWarning($"❌ Material {dto.MaterialID} no encontrado");
                    return NotFound(new { success = false, message = $"Material con ID {dto.MaterialID} no encontrado" });
                }

                // Calcular toneladas (asumiendo peso por bolsa del material)
                decimal toneladas = (dto.BolsasElaboradas * material.PesoBolsa) / 1000m;

                var produccion = new ProduccionMaterial
                {
                    TurnoProduccionID = id,
                    MaterialID = dto.MaterialID,
                    BolsasElaboradas = dto.BolsasElaboradas,
                    BolsasRotas = dto.BolsasRotas,
                    HorasMarcha = dto.HorasMarcha,
                    
                    
                };

                _context.ProduccionMaterial.Add(produccion);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Producción registrada: ID={produccion.ProduccionMaterialID}, Bolsas={dto.BolsasElaboradas}");

                return Ok(new
                {
                    success = true,
                    message = "Producción registrada exitosamente",
                    data = new
                    {
                        produccionId = produccion.ProduccionMaterialID,
                        bolsasElaboradas = produccion.BolsasElaboradas,
                        toneladas = toneladas // <-- Usar la variable local 'toneladas' en vez de 'produccion.Toneladas'
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ ERROR al registrar producción para turno {id}");
                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
            }
        }

        // ✅ NUEVO: Registrar parada
        [HttpPost("{id}/paradas")]
        [Authorize(Roles = "Administrador,Supervisor,JefeTurno,Operario")]
        public async Task<IActionResult> RegistrarParada(int id, [FromBody] CreateParadaDto dto)
        {
            try
            {
                _logger.LogInformation($"⏸️ Registrando parada para turno {id}");
                _logger.LogInformation($"   Datos recibidos: Tipo={dto.Tipo}, Descripción={dto.Descripcion}");

                var turnoExiste = await _context.TurnosProduccion.AnyAsync(t => t.TurnoProduccionID == id);
                if (!turnoExiste)
                {
                    _logger.LogWarning($"❌ Turno {id} no encontrado");
                    return NotFound(new { success = false, message = $"Turno con ID {id} no encontrado" });
                }

                var turno = await _context.TurnosProduccion
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.TurnoProduccionID == id);

                if (turno!.Estado != "En Proceso")
                {
                    _logger.LogWarning($"❌ El turno {id} no está en proceso. Estado: {turno.Estado}");
                    return BadRequest(new
                    {
                        success = false,
                        message = "Solo se puede registrar paradas en turnos que están en proceso",
                        estadoActual = turno.Estado
                    });
                }

                int? duracionMinutos = null;
                if (dto.FechaHoraFin.HasValue)
                {
                    duracionMinutos = (int)(dto.FechaHoraFin.Value - dto.FechaHoraInicio).TotalMinutes;
                }

                _logger.LogInformation($"   Creando nueva entidad Parada...");
                
                // ✅ CORRECCIÓN: Mapear SOLO las columnas que existen en la base de datos
                var parada = new Parada
                {
                    TurnoProduccionID = id,
                    TipoParada = dto.Tipo ?? "No especificado",
                    Descripcion = dto.Descripcion ?? "Sin descripción",
                    FechaHoraInicio = dto.FechaHoraInicio,
                    FechaHoraFin = dto.FechaHoraFin
                    // ✅ NO asignar: Motivo, Estado, AccionesCorrectivas (no existen en BD)
                };

                _logger.LogInformation($"   Parada creada en memoria:");
                _logger.LogInformation($"     - TurnoProduccionID: {parada.TurnoProduccionID}");
                _logger.LogInformation($"     - TipoParada: {parada.TipoParada}");
                _logger.LogInformation($"     - Descripcion: {parada.Descripcion}");
                _logger.LogInformation($"     - FechaHoraInicio: {parada.FechaHoraInicio}");
                _logger.LogInformation($"     - FechaHoraFin: {parada.FechaHoraFin?.ToString() ?? "null"}");

                _context.Paradas.Add(parada);
                
                _logger.LogInformation($"   Guardando cambios en la base de datos...");
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Parada registrada: ID={parada.ParadaID}, Tipo={dto.Tipo}, Duración={duracionMinutos ?? 0} min");

                return Ok(new
                {
                    success = true,
                    message = "Parada registrada exitosamente",
                    data = new
                    {
                        paradaId = parada.ParadaID,
                        tipo = parada.TipoParada,
                        descripcion = parada.Descripcion,
                        duracionMinutos = duracionMinutos,
                        estado = parada.Estado,
                        impactoProductivo = parada.ImpactoProductivo
                    }
                });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, $"❌ ERROR DE BASE DE DATOS al registrar parada para turno {id}");
                _logger.LogError($"   Inner Exception: {dbEx.InnerException?.Message}");
                return StatusCode(500, new 
                { 
                    success = false, 
                    message = "Error al guardar en la base de datos", 
                    error = dbEx.InnerException?.Message ?? dbEx.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ ERROR al registrar parada para turno {id}");
                return StatusCode(500, new { success = false, message = "Error interno del servidor", error = ex.Message });
            }
        }

        // ✅ Método para obtener paradas de un turno
        [HttpGet("{id}/paradas")]
        public async Task<ActionResult<IEnumerable<ParadaDto>>> GetParadasTurno(int id)
        {
            try
            {
                _logger.LogInformation($"🔍 Obteniendo paradas para turno {id}");

                var paradas = await _context.Paradas
                    .Where(p => p.TurnoProduccionID == id)
                    .OrderByDescending(p => p.FechaHoraInicio)
                    .ToListAsync();

                // ✅ CORRECCIÓN: Mapear correctamente las propiedades de la entidad Parada
                var paradasDto = paradas.Select(p => new ParadaDto
                {
                    ParadaID = p.ParadaID,
                    TurnoProduccionID = p.TurnoProduccionID,
                    Tipo = p.TipoParada,  // ✅ Mapear TipoParada a Tipo
                    Descripcion = p.Descripcion,
                    FechaHoraInicio = p.FechaHoraInicio,
                    FechaHoraFin = p.FechaHoraFin,
                    DuracionMinutos = p.FechaHoraFin.HasValue ? 
                        (int)(p.FechaHoraFin.Value - p.FechaHoraInicio).TotalMinutes : 
                        null,
                    AccionesCorrectivas = null  // ✅ La entidad Parada no tiene esta propiedad
                }).ToList();

                _logger.LogInformation($"✅ Paradas obtenidas: {paradasDto.Count}");
                return Ok(new { success = true, data = paradasDto, count = paradasDto.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ ERROR al obtener paradas del turno {id}");
                _logger.LogError($"   Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { success = false, message = "Error interno del servidor", error = ex.Message });
            }
        }

        // ========== AGREGAR ESTE ENDPOINT AL CONTROLLER DE TURNOS ==========

        [HttpGet("activo")]
        [AllowAnonymous] // ✅ AGREGAR ESTA LÍNEA para permitir acceso sin autenticación
        public async Task<ActionResult<TurnoProduccionDto>> GetTurnoActivo()
        {
            try
            {
                _logger.LogInformation("🔄 API - Buscando turno activo");
                Console.WriteLine("🔄 API - Buscando turno activo");
                
                // Buscar el turno en estado "En Proceso"
                var turnoActivo = await _context.TurnosProduccion
                    .Where(t => t.Estado == "En Proceso")
                    .OrderByDescending(t => t.FechaHoraInicio)
                    .FirstOrDefaultAsync();

                if (turnoActivo == null)
                {
                    _logger.LogInformation("ℹ️ No hay turno activo");
                    Console.WriteLine("ℹ️ No hay turno activo");
                    return NotFound(new { success = false, message = "No hay turno activo" });
                }

                var dto = new TurnoProduccionDto
                {
                    TurnoProduccionID = turnoActivo.TurnoProduccionID,
                    Fecha = turnoActivo.Fecha,
                    TurnoNumero = turnoActivo.TurnoNumero,
                    Estado = turnoActivo.Estado,
                    FechaHoraInicio = turnoActivo.FechaHoraInicio,
                    FechaHoraFin = turnoActivo.FechaHoraFin,
                    TotalBolsasElaboradas = 0,
                    TotalBolsasRotas = 0,
                    TotalToneladas = 0,
                    Producciones = new List<ProduccionMaterialDto>(),
                    Paradas = new List<ParadaDto>()
                };

                _logger.LogInformation($"✅ Turno activo encontrado: ID {dto.TurnoProduccionID}");
                Console.WriteLine($"✅ Turno activo encontrado: ID {dto.TurnoProduccionID}");
                
                return Ok(new { success = true, data = dto });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener turno activo");
                Console.WriteLine($"❌ Error al obtener turno activo: {ex.Message}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // ✅ Métodos auxiliares al final de la clase (antes del cierre de la clase)
        private DateTime? CalcularHoraFinEsperada(DateOnly fecha, int turnoNumero)
        {
            var fechaBase = fecha.ToDateTime(TimeOnly.MinValue);
            
            return turnoNumero switch
            {
                1 => fechaBase.AddHours(14).AddMinutes(30),  // 14:30 mismo día
                2 => fechaBase.AddHours(22).AddMinutes(30),  // 22:30 mismo día
                3 => fechaBase.AddDays(1).AddHours(6),        // 06:00 día siguiente
                _ => null
            };
        }

        private string ObtenerHorarioTexto(int turnoNumero)
        {
            return turnoNumero switch
            {
                1 => "06:00 - 14:30",
                2 => "14:30 - 22:30",
                3 => "22:30 - 06:00",
                _ => "N/A"
            };
        }
    }

    // DTO para finalizar turno
    public class FinalizarTurnoDto
    {
        public DateTime? FechaHoraFin { get; set; }
        public string? Observaciones { get; set; }
    }
}