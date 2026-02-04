using System.ComponentModel.DataAnnotations;

namespace CementoTrazabilidad.Shared.DTOs;

// DTO para lotes de producción con rango temporal
public class LoteProduccionDto
{
    public int LoteID { get; set; }
    public int TurnoID { get; set; } 
    public DateTime FechaHoraInicio { get; set; }
    public DateTime? FechaHoraFin { get; set; } // Cambiado a nullable
    public int CantidadBolsas { get; set; }
    public required string NumeroLote { get; set; }
    public string? TipoRegistro { get; set; }
    public string? Observaciones { get; set; }
    public int MaterialID { get; set; }
    public required string MaterialNombre { get; set; }
}

public class CreateLoteProduccionDto
{
    [Required]
    public int TurnoID { get; set; }

    [Required]
    [Range(1, 100000, ErrorMessage = "La cantidad debe estar entre 1 y 100000")]
    public int CantidadBolsas { get; set; }

    public string? TipoRegistro { get; set; }
    public string? Observaciones { get; set; }

    public double HorasMarcha { get; set; } 
    public int BolsasRotas { get; set; }   
    public int? LoteID { get; set; }        
    public string? NumeroLote { get; set; } 
    public int MaterialID { get; set; }     
    public string? MaterialNombre { get; set; } 
}

// DTO para consultar trazabilidad por timestamp
public class ConsultaTrazabilidadDto
{
    [Required]
    public DateTime FechaHoraImpresa { get; set; } // Lo que está impreso en la bolsa
    
    public int? ToleranciaMinutos { get; set; } = 5; // Margen de error
}

public class ResultadoTrazabilidadDto
{
    public bool Encontrado { get; set; }
    public LoteProduccionDto? Lote { get; set; }
    public string TurnoDescripcion { get; set; } = string.Empty;
    public List<string> PersonalTurno { get; set; } = new();
    public string MaquinaUtilizada { get; set; } = string.Empty;
    public string MateriaPrimaLote { get; set; } = string.Empty;
}