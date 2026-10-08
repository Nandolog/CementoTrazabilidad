using System;
using System.Collections.Generic;

namespace CementoTrazabilidad.Shared.DTOs
{
    public class CrearControlPesoDto
    {
        public int TurnoProduccionID { get; set; }
        public int MaterialID { get; set; }
        public string OperadorResponsable { get; set; }
        public string Observacion { get; set; }
        public List<ControlPesoDetalleDto> Detalles { get; set; }
    }

    public class ControlPesoDetalleDto
    {
        public int NumeroBoquilla { get; set; }
        public decimal PesoControlador { get; set; }
        public decimal PesoBalanza { get; set; }
        public decimal Diferencia => PesoBalanza - PesoControlador;  // ✅ AGREGAR
        public decimal AjusteAplicado { get; set; }
        public string Observacion { get; set; }
    }
}