namespace CementoTrazabilidad.Core.Entidades
{
    public class ControlPesoDetalle
    {
        public int ControlPesoDetalleID { get; set; }
        public int ControlPesoID { get; set; }
        public ControlPeso ControlPeso { get; set; }

        public int NumeroBoquilla { get; set; }
        public decimal PesoControlador { get; set; }
        public decimal PesoBalanza { get; set; }
        public decimal Diferencia { get; set; }
        public decimal AjusteAplicado { get; set; }
        public bool DentroDeTolerancia { get; set; }
        public string Observacion { get; set; }
    }
}