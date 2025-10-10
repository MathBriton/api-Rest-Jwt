using System.ComponentModel.DataAnnotations;

namespace MinhaAPI.Models;

public class Product
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Nome é obrigatório")]
    [MaxLength(200)]
    public string Name { get; set; }
    
    [MaxLength(1000)]
    public string Description { get; set; }

    [Required(ErrorMessage = "Preço é obgiatório")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Preço deve ser maior que zero")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Estoque não pode ser negativo")]
    public int Stock { get; set; }
    
    [MaxLength(100)]
    public string Category { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}