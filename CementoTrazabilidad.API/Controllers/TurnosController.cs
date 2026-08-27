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

        // ============================================
        // 📋 MÉTODOS PRINCIPALES
        // ============================================

        [HttpPost]
        [Authorize(Roles = "Administrador,Supervisor")]
        public async Task<ActionResult<TurnoDto>> Create([FromBody] CreateTurnoDto dto)
        {
            try
            {
                _logger.LogInformation($"📥 Creando turno: Fecha={dto.Fecha}, Turno={dto.TurnoNumero}");

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

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

                var turno = new TurnoProduccion
                {
                    Fecha = dto.Fecha,
                    TurnoNumero = dto.TurnoNumero,
                    Estado = "Programado",
                    FechaHoraInicio = default(DateTime),
                    FechaHoraFin = null
                };

                _context.TurnosProduccion.Add(turno);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Turno creado: ID={turno.TurnoProduccionID}, Estado={turno.Estado}");

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

        // ✅ CORREGIDO: Iniciar turno - NO asigna FechaHoraFin
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

                // ✅ VALIDACIÓN DE HORARIO
                var ahora = DateTime.Now;
                var horaActual = ahora.TimeOfDay;
                var fechaActual = DateOnly.FromDateTime(ahora);
                var diaSemana = ahora.DayOfWeek;
                bool esDomingo = diaSemana == DayOfWeek.Sunday;

                var horario = ObtenerHorarioTurno(turno.TurnoNumero);

                var horaInicioMinima = horario.HoraInicio.Add(TimeSpan.FromMinutes(-30));
                var horaInicioMaxima = horario.HoraInicio.Add(TimeSpan.FromMinutes(200));
                bool horarioValido = horaActual >= horaInicioMinima && horaActual <= horaInicioMaxima;

                if (esDomingo)
                {
                    bool hayProduccionDomingo = await HayProduccionProgramada(fechaActual);

                    if (!hayProduccionDomingo)
                    {
                        if (turno.TurnoNumero == 3 && horaActual >= new TimeSpan(22, 0, 0))
                        {
                            _logger.LogInformation($"✅ Domingo: Turno 3 permitido (mantenimiento finalizado)");
                            horarioValido = true;
                        }
                        else
                        {
                            _logger.LogWarning($"⛔ Domingo sin producción programada. Turno {turno.TurnoNumero} no permitido.");
                            return BadRequest(new
                            {
                                success = false,
                                message = $"Los domingos solo se permite el Turno 3 a partir de las 22:30 (mantenimiento). " +
                                         $"Si necesita producción, programe una excepción en la configuración.",
                                esDomingo = true,
                                turnoPermitido = 3,
                                horaMinima = "22:30"
                            });
                        }
                    }
                }

                if (!horarioValido && !esDomingo)
                {
                    bool overrideManual = await VerificarOverrideManual(turno.TurnoNumero, fechaActual);

                    if (!overrideManual)
                    {
                        _logger.LogWarning($"⛔ Turno {turno.TurnoNumero} fuera de horario. Hora actual: {horaActual}");
                        return BadRequest(new
                        {
                            success = false,
                            message = $"El turno {turno.TurnoNumero} solo puede iniciarse entre " +
                                     $"{horaInicioMinima:hh\\:mm} y {horaInicioMaxima:hh\\:mm}. " +
                                     $"Hora actual: {horaActual:hh\\:mm}",
                            horarioPermitido = new
                            {
                                desde = horaInicioMinima.ToString(@"hh\:mm"),
                                hasta = horaInicioMaxima.ToString(@"hh\:mm")
                            },
                            horaActual = horaActual.ToString(@"hh\:mm")
                        });
                    }
                    else
                    {
                        _logger.LogInformation($"✅ Turno {turno.TurnoNumero} iniciado con OVERRIDE manual");
                    }
                }

                // ✅ INICIAR TURNO - CORREGIDO
                turno.Estado = "En Proceso";
                turno.FechaHoraInicio = DateTime.Now;
                turno.FechaHoraFin = null;  // ✅ NO asignar fin esperado

                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Turno {id} iniciado. Real: {turno.FechaHoraInicio:HH:mm}");

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

        [HttpPost("override")]
        [Authorize(Roles = "Administrador,Supervisor")]
        public async Task<IActionResult> CrearOverride([FromBody] CrearOverrideDto dto)
        {
            try
            {
                _logger.LogInformation($"📝 Creando override para Turno {dto.TurnoNumero} - {dto.Fecha}");

                var usuario = User.Identity?.Name ?? "Sistema";

                var existeOverride = await _context.Set<ConfiguracionTurno>()
                    .AnyAsync(c => c.TurnoNumero == dto.TurnoNumero && c.Fecha == dto.Fecha);

                if (existeOverride)
                {
                    var configExistente = await _context.Set<ConfiguracionTurno>()
                        .FirstAsync(c => c.TurnoNumero == dto.TurnoNumero && c.Fecha == dto.Fecha);

                    configExistente.OverrideActivo = true;
                    configExistente.Motivo = dto.Motivo;
                    configExistente.UsuarioModifico = usuario;
                    configExistente.FechaModificacion = DateTime.Now;

                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"✅ Override actualizado por {usuario}");

                    return Ok(new
                    {
                        success = true,
                        message = $"Override actualizado para Turno {dto.TurnoNumero} - {dto.Fecha}",
                        data = configExistente
                    });
                }

                var configuracion = new ConfiguracionTurno
                {
                    TurnoNumero = dto.TurnoNumero,
                    Fecha = dto.Fecha,
                    OverrideActivo = true,
                    Motivo = dto.Motivo,
                    UsuarioModifico = usuario,
                    FechaModificacion = DateTime.Now
                };

                _context.Set<ConfiguracionTurno>().Add(configuracion);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Override creado por {usuario}");

                return Ok(new
                {
                    success = true,
                    message = $"Override creado para Turno {dto.TurnoNumero} - {dto.Fecha}",
                    data = configuracion
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al crear override");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("programar-produccion")]
        [Authorize(Roles = "Administrador,Supervisor")]
        public async Task<IActionResult> ProgramarProduccion([FromBody] ProgramarProduccionDto dto)
        {
            try
            {
                _logger.LogInformation($"📅 Programando producción para {dto.Fecha}");

                var usuario = User.Identity?.Name ?? "Sistema";

                var existeProgramacion = await _context.Set<ProgramacionProduccion>()
                    .AnyAsync(p => p.Fecha == dto.Fecha);

                if (existeProgramacion)
                {
                    var programacionExistente = await _context.Set<ProgramacionProduccion>()
                        .FirstAsync(p => p.Fecha == dto.Fecha);

                    programacionExistente.Activa = dto.Activa;
                    programacionExistente.Motivo = dto.Motivo;

                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"✅ Programación actualizada para {dto.Fecha}");

                    return Ok(new
                    {
                        success = true,
                        message = $"Programación actualizada para {dto.Fecha}",
                        data = programacionExistente
                    });
                }

                var programacion = new ProgramacionProduccion
                {
                    Fecha = dto.Fecha,
                    Activa = dto.Activa,
                    Motivo = dto.Motivo,
                    FechaCreacion = DateTime.Now
                };

                _context.Set<ProgramacionProduccion>().Add(programacion);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Programación creada para {dto.Fecha}");

                return Ok(new
                {
                    success = true,
                    message = $"Programación creada para {dto.Fecha}",
                    data = programacion
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al programar producción");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("configuracion-horarios")]
        [AllowAnonymous]
        public IActionResult GetConfiguracionHorarios()
        {
            try
            {
                var horarios = new List<HorarioTurnoDto>
                {
                    new() { TurnoNumero = 1, HoraInicio = new TimeSpan(6, 0, 0), HoraFin = new TimeSpan(14, 30, 0) },
                    new() { TurnoNumero = 2, HoraInicio = new TimeSpan(14, 30, 0), HoraFin = new TimeSpan(22, 30, 0) },
                    new() { TurnoNumero = 3, HoraInicio = new TimeSpan(22, 30, 0), HoraFin = new TimeSpan(6, 0, 0) }
                };

                return Ok(new
                {
                    success = true,
                    data = horarios,
                    margenMinutos = 30,
                    reglasDomingo = new
                    {
                        turnoPermitido = 3,
                        horaMinima = "22:30",
                        descripcion = "Los domingos solo se permite el Turno 3 a partir de las 22:30, " +
                                     "a menos que se haya programado producción especial."
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener configuración de horarios");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // ============================================
        // 📋 MÉTODOS GET
        // ============================================

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

        [HttpGet("activo")]
        [AllowAnonymous]
        public async Task<ActionResult<TurnoProduccionDto>> GetTurnoActivo()
        {
            try
            {
                _logger.LogInformation("🔄 API - Buscando turno activo");

                var turnoActivo = await _context.TurnosProduccion
                    .Where(t => t.Estado == "En Proceso")
                    .OrderByDescending(t => t.FechaHoraInicio)
                    .FirstOrDefaultAsync();

                if (turnoActivo == null)
                {
                    _logger.LogInformation("ℹ️ No hay turno activo");
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
                return Ok(new { success = true, data = dto });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener turno activo");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

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

        // ============================================
        // 📋 MÉTODOS DE PERSONAL
        // ============================================

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

                var yaAsignado = await _context.PersonalTurno
                    .AnyAsync(pt => pt.TurnoProduccionID == id && pt.PersonalID == dto.PersonalId);

                if (yaAsignado)
                {
                    _logger.LogWarning($"❌ Personal {dto.PersonalId} ya está asignado al turno {id}");
                    return BadRequest(new { success = false, message = "El personal ya está asignado a este turno" });
                }

                var personalTurno = new PersonalTurno
                {
                    TurnoProduccionID = id,
                    PersonalID = dto.PersonalId,
                    RolTurno = dto.RolTurno ?? "Operario"
                };

                _context.PersonalTurno.Add(personalTurno);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Personal {dto.PersonalId} asignado al turno {id}");
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

        // ============================================
        // 📋 MÉTODOS DE PRODUCCIÓN
        // ============================================

        [HttpPost("produccion")]
        [Authorize(Roles = "Administrador,Supervisor,JefeTurno,Operario")]
        public async Task<IActionResult> RegistrarProduccion([FromBody] CreateProduccionDto dto)
        {
            try
            {
                _logger.LogInformation($"📦 Registrando producción para turno {dto.TurnoProduccionID}");

                var turno = await _context.TurnosProduccion.FindAsync(dto.TurnoProduccionID);
                if (turno == null)
                {
                    return NotFound(new { success = false, message = $"Turno con ID {dto.TurnoProduccionID} no encontrado" });
                }

                if (turno.Estado != "En Proceso")
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Solo se puede registrar producción en turnos que están en proceso",
                        estadoActual = turno.Estado
                    });
                }

                var material = await _context.Materiales.FindAsync(dto.MaterialID);
                if (material == null)
                {
                    return NotFound(new { success = false, message = $"Material con ID {dto.MaterialID} no encontrado" });
                }

                var produccion = new ProduccionMaterial
                {
                    TurnoProduccionID = dto.TurnoProduccionID,
                    MaterialID = dto.MaterialID,
                    BolsasElaboradas = dto.BolsasElaboradas,
                    BolsasRotas = dto.BolsasRotas,
                    HorasMarcha = dto.HorasMarcha,  // ✅ TimeSpan
                    Observaciones = dto.Observaciones  // ✅ AGREGADO
                };

                _context.ProduccionMaterial.Add(produccion);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Producción registrada: Rotas={dto.BolsasRotas}");

                return Ok(new
                {
                    success = true,
                    message = "Producción registrada exitosamente",
                    data = new
                    {
                        produccionId = produccion.ProduccionMaterialID,
                        bolsasRotas = produccion.BolsasRotas
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ ERROR al registrar producción");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // ============================================
        // 📋 MÉTODOS DE PARADAS
        // ============================================

        [HttpPost("{id}/paradas")]
        [Authorize(Roles = "Administrador,Supervisor,JefeTurno,Operario")]
        public async Task<IActionResult> RegistrarParada(int id, [FromBody] CreateParadaDto dto)
        {
            try
            {
                _logger.LogInformation($"⏸️ Registrando parada para turno {id}");

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

                var parada = new Parada
                {
                    TurnoProduccionID = id,
                    TipoParada = dto.Tipo ?? "No especificado",
                    Descripcion = dto.Descripcion ?? "Sin descripción",
                    FechaHoraInicio = dto.FechaHoraInicio,
                    FechaHoraFin = dto.FechaHoraFin
                };

                _context.Paradas.Add(parada);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Parada registrada: ID={parada.ParadaID}");

                return Ok(new
                {
                    success = true,
                    message = "Parada registrada exitosamente",
                    data = new
                    {
                        paradaId = parada.ParadaID,
                        tipo = parada.TipoParada,
                        descripcion = parada.Descripcion
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ ERROR al registrar parada para turno {id}");
                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
            }
        }

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

                var paradasDto = paradas.Select(p => new ParadaDto
                {
                    ParadaID = p.ParadaID,
                    TurnoProduccionID = p.TurnoProduccionID,
                    Tipo = p.TipoParada,
                    Descripcion = p.Descripcion,
                    FechaHoraInicio = p.FechaHoraInicio,
                    FechaHoraFin = p.FechaHoraFin,
                    DuracionMinutos = p.FechaHoraFin.HasValue ?
                        (int)(p.FechaHoraFin.Value - p.FechaHoraInicio).TotalMinutes :
                        null,
                    AccionesCorrectivas = null
                }).ToList();

                _logger.LogInformation($"✅ Paradas obtenidas: {paradasDto.Count}");
                return Ok(new { success = true, data = paradasDto, count = paradasDto.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ ERROR al obtener paradas del turno {id}");
                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
            }
        }

        // ============================================
        // 📋 MÉTODOS AUXILIARES
        // ============================================

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

        private HorarioTurnoDto ObtenerHorarioTurno(int turnoNumero)
        {
            return turnoNumero switch
            {
                1 => new HorarioTurnoDto { TurnoNumero = 1, HoraInicio = new TimeSpan(6, 0, 0), HoraFin = new TimeSpan(14, 30, 0) },
                2 => new HorarioTurnoDto { TurnoNumero = 2, HoraInicio = new TimeSpan(14, 30, 0), HoraFin = new TimeSpan(22, 30, 0) },
                3 => new HorarioTurnoDto { TurnoNumero = 3, HoraInicio = new TimeSpan(22, 30, 0), HoraFin = new TimeSpan(6, 0, 0) },
                _ => new HorarioTurnoDto { TurnoNumero = turnoNumero, HoraInicio = new TimeSpan(6, 0, 0), HoraFin = new TimeSpan(14, 30, 0) }
            };
        }

        private async Task<bool> HayProduccionProgramada(DateOnly fecha)
        {
            try
            {
                var programacion = await _context.Set<ProgramacionProduccion>()
                    .FirstOrDefaultAsync(p => p.Fecha == fecha && p.Activa);

                return programacion != null;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> VerificarOverrideManual(int turnoNumero, DateOnly fecha)
        {
            try
            {
                var configuracion = await _context.Set<ConfiguracionTurno>()
                    .FirstOrDefaultAsync(c => c.TurnoNumero == turnoNumero &&
                                              c.Fecha == fecha &&
                                              c.OverrideActivo);

                return configuracion != null;
            }
            catch
            {
                return false;
            }
        }
    }

    // ============================================
    // 📋 DTOs
    // ============================================

    public class HorarioTurnoDto
    {
        public int TurnoNumero { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
    }

    public class CrearOverrideDto
    {
        public int TurnoNumero { get; set; }
        public DateOnly Fecha { get; set; }
        public string Motivo { get; set; }
    }

    public class ProgramarProduccionDto
    {
        public DateOnly Fecha { get; set; }
        public bool Activa { get; set; }
        public string Motivo { get; set; }
    }

    public class FinalizarTurnoDto
    {
        public DateTime? FechaHoraFin { get; set; }
        public string? Observaciones { get; set; }
    }
}

// ============================================
// 📋 ENTIDADES PARA LAS NUEVAS TABLAS
// ============================================

namespace CementoTrazabilidad.Core.Entidades
{
    public class ConfiguracionTurno
    {
        public int ConfiguracionTurnoID { get; set; }
        public int TurnoNumero { get; set; }
        public DateOnly Fecha { get; set; }
        public bool OverrideActivo { get; set; }
        public string Motivo { get; set; }
        public string UsuarioModifico { get; set; }
        public DateTime FechaModificacion { get; set; }
    }

    public class ProgramacionProduccion
    {
        public int ProgramacionProduccionID { get; set; }
        public DateOnly Fecha { get; set; }
        public bool Activa { get; set; }
        public string Motivo { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}