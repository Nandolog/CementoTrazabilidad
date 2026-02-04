using System.ComponentModel.DataAnnotations;

namespace CementoTrazabilidad.Shared.DTOs;

public class EventoCargaDto
{
    public int EventoCargaID { get; set; }
    public int TurnoProduccionID { get; set; }
    public string ZonaCarga { get; set; } = string.Empty;
    public string TipoEvento { get; set; } = string.Empty;
    public DateTime FechaHora { get; set; }
    public string? Observaciones { get; set; }
    public int? CantidadBolsas { get; set; }
}

public class RegistrarEventoCargaDto
{
    [Required]
    public int TurnoProduccionID { get; set; }
    
    [Required]
    public string ZonaCarga { get; set; } = string.Empty;
    
    [Required]
    public string TipoEvento { get; set; } = string.Empty;
    
    public string? Observaciones { get; set; }
}

public class ResumenCargaDto    
{
    public string ZonaCarga { get; set; } = string.Empty;
    public DateTime? HoraInicio { get; set; }
    public DateTime? HoraFin { get; set; }
    public TimeSpan? TiempoTotal { get; set; }
    public string Estado { get; set; } = string.Empty;
}

public class EventoCargaHistorialDto
{
    public int EventoCargaID { get; set; }
    public string ZonaCarga { get; set; } = string.Empty;
    public string TipoEvento { get; set; } = string.Empty;
    public DateTime FechaHora { get; set; }
    public string? Observaciones { get; set; }
    
    // ✅ NUEVA PROPIEDAD: Tipo de carga calculado
    public string TipoCarga => ZonaCarga == "Anden" ? "Granel" : "Paletizado";
}
