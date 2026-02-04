using System.ComponentModel.DataAnnotations;

namespace CementoTrazabilidad.Shared.DTOs;

public class ProduccionMaterialDto
{
    public int ProduccionMaterialID { get; set; }
    public int TurnoProduccionID { get; set; }
    public int MaterialID { get; set; }
    public string MaterialCodigo { get; set; } = string.Empty;
    public int BolsasElaboradas { get; set; }
    public int BolsasRotas { get; set; }
    public decimal HorasMarcha { get; set; }
    public decimal Toneladas { get; set; }
    public DateTime? HoraRegistro { get; set; }

    public string MaterialNombre { get; set; } = string.Empty;

}

public class CreateProduccionDto
{
    [Required]
    public int TurnoProduccionID { get; set; }

    [Required]
    public int MaterialID { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public int BolsasElaboradas { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public int BolsasRotas { get; set; }

    [Required]
    [Range(0, 24)]
    public decimal HorasMarcha { get; set; }
}

public class ParadaDto
{
    public int ParadaID { get; set; }
    public int TurnoProduccionID { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Motivo { get; set; } = string.Empty; // ✅ AGREGADO
    public string Descripcion { get; set; } = string.Empty;
    public DateTime FechaHoraInicio { get; set; }
    public DateTime? FechaHoraFin { get; set; }
    public int? DuracionMinutos { get; set; }
    public string? AccionesCorrectivas { get; set; }
    public string? ImpactoProductivo { get; set; } // ✅ AGREGADO
}

public class CreateParadaDto
{
    [Required(ErrorMessage = "El turno es requerido")]
    public int TurnoProduccionID { get; set; }

    [Required(ErrorMessage = "El tipo de parada es requerido")]
    public string Tipo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El motivo es requerido")]
    [MaxLength(200)]
    public string Motivo { get; set; } = string.Empty; // ✅ AGREGADO

    [Required(ErrorMessage = "La descripción es requerida")]
    [MaxLength(500)]
    public string Descripcion { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha y hora de inicio es requerida")]
    public DateTime FechaHoraInicio { get; set; } = DateTime.Now;

    public DateTime? FechaHoraFin { get; set; }

    [MaxLength(500)]
    public string? AccionesCorrectivas { get; set; }
}

// ✅ NUEVO DTO para turno activo con información de producción
public class TurnoProduccionDto
{
    public int TurnoProduccionID { get; set; }
    public DateOnly Fecha { get; set; }
    public int TurnoNumero { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaHoraInicio { get; set; }
    public DateTime? FechaHoraFin { get; set; }
    public List<ProduccionMaterialDto> Producciones { get; set; } = new();
    public List<ParadaDto> Paradas { get; set; } = new();
    public int TotalBolsasElaboradas { get; set; }
    public int TotalBolsasRotas { get; set; }
    public decimal TotalToneladas { get; set; }
}

