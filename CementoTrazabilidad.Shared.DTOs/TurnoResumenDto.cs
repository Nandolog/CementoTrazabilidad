public class TurnoResumenDto
{
    public int TurnoProduccionID { get; set; }
    public DateOnly Fecha { get; set; }
    public int TurnoNumero { get; set; }
    public string Estado { get; set; }
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
    public int CantidadParadas { get; set; } // ← Añadido para corregir el error CS0117
}