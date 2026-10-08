namespace CementoTrazabilidad.Shared.DTOs
{
    public class EstadisticaBoquillaDto
    {
        public int Boquilla { get; set; }
        public decimal PesoPromedioBalanza { get; set; }
        public decimal PesoPromedioControlador { get; set; }
        public decimal DiferenciaPromedio { get; set; }
        public decimal DesviacionEstandar { get; set; }
        public int CantidadControles { get; set; }
        public int ControlesFueraTolerancia { get; set; }
        public decimal AjustePromedio { get; set; }
    }

    public class DashboardControlPesoDto
    {
        public bool TieneControles { get; set; }
        public string? Mensaje { get; set; }
        public int TotalControles { get; set; }
        public int ControlesOK { get; set; }
        public int ControlesFueraTolerancia { get; set; }
        public int ControlesCriticos { get; set; }
        public UltimoControlDto? UltimoControl { get; set; }
        public List<TendenciaControlDto>? Tendencia { get; set; }
        public List<BoquillaResumenDto>? Boquillas { get; set; }
        public decimal DesviacionPromedioTurno { get; set; }
        public decimal PorcentajeAceptacion { get; set; }
    }

    public class UltimoControlDto
    {
        public int Numero { get; set; }
        public DateTime FechaHora { get; set; }
        public decimal PesoPromedioBalanza { get; set; }
        public decimal DiferenciaPromedio { get; set; }
        public decimal DesviacionEstandar { get; set; }
        public string Estado { get; set; } = "";
    }

    public class TendenciaControlDto
    {
        public int Numero { get; set; }
        public decimal PesoPromedio { get; set; }
        public decimal Diferencia { get; set; }
        public decimal Desviacion { get; set; }
        public string Estado { get; set; } = "";
    }

    public class BoquillaResumenDto
    {
        public int Boquilla { get; set; }
        public decimal PesoPromedio { get; set; }
        public decimal DiferenciaPromedio { get; set; }
        public int ControlesFueraTolerancia { get; set; }
    }
}