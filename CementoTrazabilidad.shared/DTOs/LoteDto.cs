namespace CementoTrazabilidad.Shared.DTOs
{
    public class LoteDto
    {
        public int LoteId { get; set; }
        public string? Descripcion { get; set; }
        // Agrega otras propiedades según sea necesario
        public string? PersonalNombre { get; set; }
        public int? PersonalID { get; set; }
        
    }
}