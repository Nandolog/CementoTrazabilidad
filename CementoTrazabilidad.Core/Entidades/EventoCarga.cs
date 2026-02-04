using System.ComponentModel.DataAnnotations;

namespace CementoTrazabilidad.Core.Entidades;

public class EventoCarga
{
    public int EventoCargaID { get; set; }
    
    public int TurnoProduccionID { get; set; }
    
    [Required, MaxLength(20)]
    public string ZonaCarga { get; set; } = string.Empty; // "Anden" o "Paletizado"
    
    [Required, MaxLength(20)]
    public string TipoEvento { get; set; } = string.Empty; // "Inicio" o "Fin"
    
    public DateTime FechaHora { get; set; } = DateTime.Now;
    
    [MaxLength(200)]
    public string? Observaciones { get; set; }
    public int? CantidadBolsas { get; set; }


    // Navegación
    public virtual TurnoProduccion Turno { get; set; } = null!;
}