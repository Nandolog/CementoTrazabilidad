using System.ComponentModel.DataAnnotations;

namespace CementoTrazabilidad.Shared.DTOs;

public class TurnoDto
{
    public int TurnoProduccionID { get; set; }
    public DateOnly Fecha { get; set; }
    public int TurnoNumero { get; set; }
    public string Estado { get; set; } = "Programado";
    public DateTime FechaHoraInicio { get; set; }
    public DateTime? FechaHoraFin { get; set; }
    public  string? Observaciones { get; set; }
}

public class CreateTurnoDto
{
    [Required(ErrorMessage = "La fecha es requerida")]
    public DateOnly Fecha { get; set; }

    [Required(ErrorMessage = "El número de turno es requerido")]
    [Range(1, 4, ErrorMessage = "El turno debe estar entre 1 y 4")]
    public int TurnoNumero { get; set; }

    public TimeOnly? HoraInicio { get; set; }

    // ✅ Personal a asignar (opcional) - ¡ESTA YA LA TIENES!
    public List<int>? PersonalIds { get; set; } = new();

    // ✅ ESTAS SON LAS QUE FALTAN:
    public Dictionary<int, string>? RolesPersonal { get; set; } = new();
    public string? Observaciones { get; set; }
    public bool IniciarAutomaticamente { get; set; } = false;
}

public class TurnoDetalleDto : TurnoDto
{
    public int TotalBolsas { get; set; }
    public decimal TotalToneladas { get; set; }
    public int TotalBolsasRotas { get; set; }
    public decimal PorcentajeRotura { get; set; }
    public List<PersonalDto> Personal { get; set; } = new();
    public List<ProduccionMaterialDto> Produccion { get; set; } = new();
    public List<ParadaDto> Paradas { get; set; } = new();
}
    
    
public class TurnoResumenDto
{
    public int TurnoProduccionID { get; set; }
    public DateOnly Fecha { get; set; }
    public int TurnoNumero { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaHoraInicio { get; set; }
    public DateTime? FechaHoraFin { get; set; }
    public int TotalBolsas { get; set; }
    public decimal TotalToneladas { get; set; }
    public int TotalBolsasRotas { get; set; }
    public decimal PorcentajeRotura { get; set; }
    public int TotalParadas { get; set; }
    public decimal TotalHorasParadas { get; set; }
    public int CantidadPersonal { get; set; }
    public decimal Eficiencia { get; set; }
}

// DTOs adicionales útiles

public class IniciarTurnoDto
{
    public DateTime? FechaHoraInicio { get; set; }
}

public class FinalizarTurnoDto
{
    public DateTime? FechaHoraFin { get; set; }
    public string? Observaciones { get; set; }
}

public class UpdateTurnoDto
{
    [Range(1, 4)]
    public int? TurnoNumero { get; set; }

    public DateOnly? Fecha { get; set; }

    public DateTime? FechaHoraInicio { get; set; }

    [StringLength(500)]
    public string? Observaciones { get; set; }
}

public class FiltroTurnosDto
{
    public DateOnly? FechaDesde { get; set; }
    public DateOnly? FechaHasta { get; set; }
    public int? TurnoNumero { get; set; }
    public string? Estado { get; set; }
    public int? PersonalId { get; set; }
    public int Pagina { get; set; } = 1;
    public int TamanoPagina { get; set; } = 20;
}

public class EstadoTurnoDto
{
    public int TurnoProduccionID { get; set; }
    public string Estado { get; set; } = string.Empty;
    public bool TieneParadaActiva { get; set; }
    public DateTime? InicioParadaActiva { get; set; }
    public string? MotivoParadaActiva { get; set; }
}