using System.ComponentModel.DataAnnotations;

namespace MinhaAPI.Models;

public class ProductCreateRequest
{
    [Required(ErrorMessage = "Nome é obrigatório")]
    [MaxLength(200)]
    public string Name { get; set; }
    
    [MaxLength(1000)]
    public string Description { get; set; }

    [Required(ErrorMessage = "Preço é obrigatório")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Preço deve ser maior que zero")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Estoque não pode ser negativo")]
    public int Stock { get; set; } = 0;
    
    [MaxLength(100)]
    public string Category { get; set; }

    public bool IsActive { get; set; } = true;
}