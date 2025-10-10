using System.ComponentModel.DataAnnotations;

namespace MinhaAPI.Models;

public class User
{
    [Key]
    public int Id { get; set; }
    [Required]
    [MaxLength(100)]
    public string Username { get; set; }
    [Required]
    [MaxLength(100)]
    public string Email { get; set; }
    [Required]
    public string PasswordHash { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
}