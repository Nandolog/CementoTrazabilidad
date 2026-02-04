namespace CementoTrazabilidad.Shared.DTOs
{
    public class ConsumoBolsasDTO
    {
        public int ConsumoBolsasID { get; set; }
        public int ProveedorBolsaID { get; set; }
        public string? ProveedorNombre { get; set; }
        public int TurnoProduccionID { get; set; }
        public int? ProduccionMaterialID { get; set; }
        public string? MaterialDescripcion { get; set; }
        public int CantidadBolsas { get; set; }
        public int BolsasDefectuosas { get; set; }
        public DateTime FechaConsumo { get; set; }
        public string? LoteBolsa { get; set; }
        public string? TipoCemento { get; set; }
        public string? Observaciones { get; set; }
    }

    public class ConsumoBolsasCreateDTO
    {
        public int ProveedorBolsaID { get; set; }
        public int TurnoProduccionID { get; set; }
        public int? ProduccionMaterialID { get; set; }
        public int CantidadBolsas { get; set; }
        public int BolsasDefectuosas { get; set; }
        public string? LoteBolsa { get; set; }
        public string? TipoCemento { get; set; }
        public string? Observaciones { get; set; }
    }

    public class ProveedorBolsaDTO
    {
        public int ProveedorBolsaID { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Activo { get; set; }
    }
}