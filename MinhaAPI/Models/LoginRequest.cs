using System.ComponentModel.DataAnnotations;

namespace MinhaAPI.Models;

public class LoginRequest
{
    [Required(ErrorMessage = "Username é obrigatório")]
    public string Username { get; set; }
    [Required(ErrorMessage="Password é obrigatório")]
    public string Password { get; set; }
}