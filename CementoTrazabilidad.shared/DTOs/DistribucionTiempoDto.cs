namespace CementoTrazabilidad.Shared.DTOs;

public class DistribucionTiempoDto
{
    public string Actividad { get; set; } = string.Empty;
    public double Minutos { get; set; }
    public double Horas { get; set; }
    public decimal Porcentaje { get; set; }
    public string Color { get; set; } = string.Empty;
}