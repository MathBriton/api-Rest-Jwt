using System.ComponentModel.DataAnnotations;

namespace MinhaAPI.Models;

public class RefreshTokenRequest
{
    [Required(ErrorMessage = "Refresh token é obrigatório")]
    public string RefreshToken { get; set; }
}