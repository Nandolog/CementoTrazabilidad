using System.ComponentModel.DataAnnotations;

namespace CementoTrazabilidad.Shared.DTOs;

public class MaterialDto
{
    public int MaterialID { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal PesoPorBolsa { get; set; }
    public decimal DensidadKGm3 { get; set; } // ← AGREGAR
    public bool Activo { get; set; }
}

public class CreateMaterialDto
{
    [Required]
    [MaxLength(10)]
    public string Codigo { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Descripcion { get; set; } = string.Empty;

    [Required]
    [Range(25, 100)]
    public decimal PesoPorBolsa { get; set; } = 50.0m;

    [Required]
    [Range(1000, 2000)]
    public decimal DensidadKGm3 { get; set; } = 1500.0m; // ← AGREGAR
}

public class UpdateMaterialDto
{
    [Required]
    [MaxLength(100)]
    public string Descripcion { get; set; } = string.Empty;

    [Required]
    [Range(25, 100)]
    public decimal PesoPorBolsa { get; set; }

    [Required]
    [Range(1000, 2000)]
    public decimal DensidadKGm3 { get; set; } // ← AGREGAR

    public bool Activo { get; set; }
}