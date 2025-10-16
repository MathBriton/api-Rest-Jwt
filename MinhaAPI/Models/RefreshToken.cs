using System.ComponentModel.DataAnnotations;

namespace MinhaAPI.Models;

public class RefreshToken
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string Token { get; set; }
    [Required]
    public int UserId  { get; set; }
    public User User { get; set; }
    [Required]
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRevoked { get; set; } = false;
    public DateTime? RevokedAt  { get; set; }
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;
}