using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CementoTrazabilidad.Core.Entidades;

public class LoteProduccion
{
    [Key]
    public int LoteID { get; set; }
    
    [ForeignKey("Turno")]
    public int TurnoID { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string NumeroLote { get; set; } = string.Empty;
    
    public DateTime FechaHoraInicio { get; set; }
    public DateTime? FechaHoraFin { get; set; }
    
    [Range(1, 100000)]
    public int CantidadBolsas { get; set; }
    
    [ForeignKey("Material")]
    public int MaterialID { get; set; }
    
    [Required]
    [MaxLength(20)]
    public string TipoRegistro { get; set; } = "Manual";
    
    [MaxLength(500)]
    public string? Observaciones { get; set; }

    // Navigation properties
    public virtual TurnoProduccion? Turno { get; set; }
    public virtual Material? Material { get; set; }
}