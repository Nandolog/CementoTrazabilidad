using CementoTrazabilidad.Shared.DTOs;
using Microsoft.EntityFrameworkCore;
using CementoTrazabilidad.Core.Entidades;
using CementoTrazabilidad.Infrastructure.Data;
using Microsoft.Extensions.Logging;
namespace CementoTrazabilidad.Infrastructure.Services
{
    public class ControlPesoService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ControlPesoService> _logger;

        public ControlPesoService(ApplicationDbContext context, ILogger<ControlPesoService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ControlPeso> RegistrarControlPeso(CrearControlPesoDto dto, string usuario)
        {
            _logger.LogInformation($"⚖️ Registrando control de peso para turno {dto.TurnoProduccionID}");

            // 1. Obtener configuración de tolerancia del material
            var configTolerancia = await _context.ConfiguracionesToleranciaPeso
                .FirstOrDefaultAsync(c => c.MaterialID == dto.MaterialID && c.Activo);

            if (configTolerancia == null)
                throw new Exception($"No hay configuración de tolerancia para el material {dto.MaterialID}");

            // 2. Calcular número de control en el turno
            var controlesPrevios = await _context.ControlesPeso
                .Where(c => c.TurnoProduccionID == dto.TurnoProduccionID)
                .CountAsync();

            // 3. Calcular promedios y estadísticas
            var pesosControlador = dto.Detalles.Select(d => d.PesoControlador).ToList();
            var pesosBalanza = dto.Detalles.Select(d => d.PesoBalanza).ToList();
            var diferencias = dto.Detalles.Select(d => d.PesoBalanza - d.PesoControlador).ToList();

            var promedioControlador = pesosControlador.Average();
            var promedioBalanza = pesosBalanza.Average();
            var diferenciaPromedio = diferencias.Average();
            var desviacionEstandar = CalcularDesviacionEstandar(pesosBalanza);

            // 4. Determinar estado del control
            var pesosFueraTolerancia = dto.Detalles
                .Count(d => d.PesoBalanza < configTolerancia.ToleranciaMinima ||
                            d.PesoBalanza > configTolerancia.ToleranciaMaxima);

            string estadoControl;
            bool dentroDeTolerancia;

            if (pesosFueraTolerancia == 0)
            {
                estadoControl = "OK";
                dentroDeTolerancia = true;
            }
            else if (pesosFueraTolerancia <= 2)
            {
                estadoControl = "FUERA_TOLERANCIA";
                dentroDeTolerancia = false;
            }
            else
            {
                estadoControl = "CRITICO";
                dentroDeTolerancia = false;
            }

            // 5. Crear cabecera del control
            var controlPeso = new ControlPeso
            {
                TurnoProduccionID = dto.TurnoProduccionID,
                FechaHora = DateTime.Now,
                NumeroControl = controlesPrevios + 1,
                OperadorResponsable = dto.OperadorResponsable,
                Observaciones = dto.Observacion,
                PesoObjetivo = configTolerancia.PesoObjetivo,
                PesoPromedioControlador = Math.Round(promedioControlador, 3),
                PesoPromedioBalanza = Math.Round(promedioBalanza, 3),
                DiferenciaPromedio = Math.Round(diferenciaPromedio, 3),
                DesviacionEstandar = Math.Round(desviacionEstandar, 3),
                DentroDeTolerancia = dentroDeTolerancia,
                EstadoControl = estadoControl,
                FechaCreacion = DateTime.Now
            };

            _context.ControlesPeso.Add(controlPeso);
            await _context.SaveChangesAsync();

            // 6. Crear detalles por boquilla
            foreach (var detalleDto in dto.Detalles)
            {
                var diferencia = detalleDto.PesoBalanza - detalleDto.PesoControlador;
                var dentroTol = detalleDto.PesoBalanza >= configTolerancia.ToleranciaMinima &&
                                detalleDto.PesoBalanza <= configTolerancia.ToleranciaMaxima;

                var detalle = new ControlPesoDetalle
                {
                    ControlPesoID = controlPeso.ControlPesoID,
                    NumeroBoquilla = detalleDto.NumeroBoquilla,
                    PesoControlador = detalleDto.PesoControlador,
                    PesoBalanza = detalleDto.PesoBalanza,
                    Diferencia = Math.Round(diferencia, 3),
                    AjusteAplicado = detalleDto.AjusteAplicado,
                    DentroDeTolerancia = dentroTol,
                    Observacion = detalleDto.Observacion
                };

                _context.ControlesPesoDetalle.Add(detalle);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation($"✅ Control de peso #{controlPeso.NumeroControl} registrado. " +
                                   $"Estado: {estadoControl}, " +
                                   $"Desviación: {desviacionEstandar:F3} kg");

            return controlPeso;
        }

        private decimal CalcularDesviacionEstandar(List<decimal> valores)
        {
            if (valores.Count <= 1) return 0;

            var promedio = valores.Average();
            var sumaCuadrados = valores.Sum(v => (v - promedio) * (v - promedio));
            var varianza = sumaCuadrados / (valores.Count - 1);

            return (decimal)Math.Sqrt((double)varianza);
        }
    }
}