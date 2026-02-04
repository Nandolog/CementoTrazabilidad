public class EventoCarga
{
    public int EventoCargaID { get; set; }
    public int TurnoProduccionID { get; set; }
    public string ZonaCarga { get; set; }
    public string TipoEvento { get; set; }
    public DateTime FechaHora { get; set; }
    public string? Observaciones { get; set; }
    public int? CantidadBolsas { get; set; } // ← Agrega esta propiedad para que coincida con el DTO
    public virtual TurnoProduccion Turno { get; set; }
}