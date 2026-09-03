using CementoTrazabilidad.Shared.DTOs;
using System.ComponentModel.DataAnnotations;

namespace CementoTrazabilidad.Shared.DTOs
{
    // DTO para lotes de producción con rango temporal
    public class LoteProduccionDto
    {
        public int LoteProduccionID { get; set; }

        // Propiedades adicionales para compatibilidad con el controlador
        public int LoteID { get; set; }  // Para compatibilidad
        public int TurnoID { get; set; }  // Para compatibilidad

        public int TurnoProduccionID { get; set; }
        public string NumeroLote { get; set; } = string.Empty;
        public DateTime FechaHoraInicio { get; set; }
        public DateTime? FechaHoraFin { get; set; }
        public int CantidadBolsas { get; set; }
        public string? Observaciones { get; set; }

        public string TipoRegistro { get; set; } = "Manual";
        public string ZonaCarga { get; set; } = "Paletizado";
        public int BolsasAnden { get; set; }
        public int BolsasPaletizado { get; set; }

        // Propiedades adicionales
        public int MaterialID { get; set; }
        public string? MaterialNombre { get; set; }
        public int BolsasRotas { get; set; }
        public decimal HorasMarcha { get; set; }

        public TurnoProduccionDto? Turno { get; set; }
        public int? PersonalID { get; set; }
        public string? PersonalNombre { get; set; }
    }
}

public class CreateLoteProduccionDto
{
    [Required(ErrorMessage = "El Turno es requerido")]
    public int TurnoID { get; set; }

    [Required(ErrorMessage = "La cantidad de bolsas es requerida")]
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
    public int CantidadBolsas { get; set; }

    public string? Observaciones { get; set; }

    // Propiedades de distribución
    [Required(ErrorMessage = "La zona de carga es requerida")]
    public string ZonaCarga { get; set; } = "Paletizado";

    [Range(0, int.MaxValue, ErrorMessage = "La cantidad de bolsas a anden debe ser mayor o igual a 0")]
    public int BolsasAnden { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "La cantidad de bolsas a paletizado debe ser mayor o igual a 0")]
    public int BolsasPaletizado { get; set; }

    // Propiedades adicionales (opcionales)
    public int? MaterialID { get; set; }
    public string? MaterialNombre { get; set; }
    public int? BolsasRotas { get; set; }
    public decimal? HorasMarcha { get; set; }

    // Para compatibilidad (no enviar si no es necesario)
    public int? LoteID { get; set; }
    public string? NumeroLote { get; set; }
    public string? TipoRegistro { get; set; } = "Manual";
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

