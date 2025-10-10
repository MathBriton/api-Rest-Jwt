using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MinhaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{

    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        //Validação simples ( em produção, use banco de dados e hash de senha)
        if (request.Username == "admin" && request.Password == "123456")
        {
            var token = GenerateJwtToken(request.Username);
            var expiration = DateTime.UtcNow.AddMinutes(
                int.Parse(_configuration["JwtSettings:ExpirationMinutes"]));

            return Ok(new LoginResponse
            {
                Token = token,
                Expiration = expiration
            });
        }

        return Unauthorized(new { message = "Usuário ou senha inválidos" });
    }

    [Authorize]
    [HttpGet("protected")]
    public IActionResult Protected()
    {
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        return Ok(new { message = $"Olá, {username}! Você está autenticado." });
    }

    private string GenerateJwtToken(string username)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"];
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["ExpirationMinutes"])),
            SigningCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}