using System.ComponentModel.DataAnnotations;

namespace MinhaAPI.Models;

public class RegisterRequest
{
    [Required(ErrorMessage = "Username é obrigatório")]
    [MinLength(3, ErrorMessage = "Username deve ter no mínimo 3 caracteres")]
    public string Username { get; set; }

    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Password é obrigatório")]
    [MinLength(6, ErrorMessage = "Password deve ter no mínimo 6 caracteres")]
    public string Password { get; set; }

    public string Role { get; set; } = "User";
}