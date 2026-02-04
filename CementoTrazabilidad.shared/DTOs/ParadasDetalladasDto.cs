namespace CementoTrazabilidad.Shared.DTOs;

public class ParadasDetalladasDto
{
    public string TipoParada { get; set; } = string.Empty;
    public double TotalMinutos { get; set; }
    public double TotalHoras { get; set; }
    public int CantidadParadas { get; set; }
    public List<ParadaIndividualDto> Paradas { get; set; } = new();
}

public class ParadaIndividualDto
{
    public int ParadaID { get; set; }
    public DateTime Inicio { get; set; }
    public DateTime? Fin { get; set; }
    public double Minutos { get; set; }
    public string Motivo { get; set; } = string.Empty;
}