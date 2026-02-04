using System.ComponentModel.DataAnnotations;

namespace CementoTrazabilidad.Shared.DTOs;

public class DespachoDTO
{
    public int DespachoID { get; set; }
    public int? TurnoProduccionID { get; set; }
    public int? MaterialID { get; set; }
    public string Modalidad { get; set; } = string.Empty;
    public string Destino { get; set; } = string.Empty;
    public int? Bolsas { get; set; }
    public decimal? Toneladas { get; set; }
    public int? Camiones { get; set; }
    public DateTime? FechaHoraDespacho { get; set; }

    // Información adicional para mostrar
    public int? TurnoNumero { get; set; }
    public DateTime? TurnoFecha { get; set; }
    public string? MaterialDescripcion { get; set; }
}

public class CreateDespachoDTO
{
    public int? TurnoProduccionID { get; set; }
    public int? MaterialID { get; set; }

    [Required(ErrorMessage = "La modalidad es requerida")]
    [RegularExpression("^(GRANEL|PALETIZADO|ANDEN)$",
        ErrorMessage = "La modalidad debe ser GRANEL, PALETIZADO o ANDEN")]
    public string Modalidad { get; set; } = string.Empty;

    [Required(ErrorMessage = "El destino es requerido")]
    [MaxLength(50)]
    public string Destino { get; set; } = string.Empty;

    public int? Bolsas { get; set; }
    public decimal? Toneladas { get; set; }
    public int? Camiones { get; set; }
    public DateTime? FechaHoraDespacho { get; set; } = DateTime.Now;
}

public class UpdateDespachoDTO
{
    public int? TurnoProduccionID { get; set; }
    public int? MaterialID { get; set; }

    [Required(ErrorMessage = "La modalidad es requerida")]
    [RegularExpression("^(GRANEL|PALETIZADO|ANDEN)$",
        ErrorMessage = "La modalidad debe ser GRANEL, PALETIZADO o ANDEN")]
    public string Modalidad { get; set; } = string.Empty;

    [Required(ErrorMessage = "El destino es requerido")]
    [MaxLength(50)]
    public string Destino { get; set; } = string.Empty;

    public int? Bolsas { get; set; }
    public decimal? Toneladas { get; set; }
    public int? Camiones { get; set; }
    public DateTime? FechaHoraDespacho { get; set; }
}