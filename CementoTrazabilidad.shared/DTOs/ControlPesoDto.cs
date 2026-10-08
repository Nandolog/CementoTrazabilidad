using System;
using System.Collections.Generic;

namespace CementoTrazabilidad.Shared.DTOs
{
    public class ControlPesoDto
    {
        public int ControlPesoID { get; set; }
        public int TurnoProduccionID { get; set; }
        public DateTime FechaHora { get; set; }
        public int NumeroControl { get; set; }
        public string OperadorResponsable { get; set; }
        public string Observacion { get; set; }

        public decimal PesoObjetivo { get; set; }
        public decimal PesoPromedioControlador { get; set; }
        public decimal PesoPromedioBalanza { get; set; }
        public decimal DiferenciaPromedio { get; set; }
        public decimal DesviacionEstandar { get; set; }

        public bool DentroDeTolerancia { get; set; }
        public string EstadoControl { get; set; }

        public List<ControlPesoDetalleDto> Detalles { get; set; }
    }
}