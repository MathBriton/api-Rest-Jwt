using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinhaAPI.Data;
using MinhaAPI.Models;
using MinhaAPI.Services;
using System.Security.Claims;
using BCrypt.Net;

namespace MinhaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _context;
    private readonly ITokenService _tokenService;

    public AuthController(IConfiguration configuration, AppDbContext context, ITokenService tokenService)
    {
        _configuration = configuration;
        _context = context;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
        {
            return BadRequest(new { message = "Username já está em uso" });
        }

        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            return BadRequest(new { message = "Email já está em uso" });
        }

        var validRoles = new[] { "Admin", "User", "Manager" };
        if (!validRoles.Contains(request.Role))
        {
            request.Role = "User";
        }

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Role,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Usuário registrado com sucesso", userId = user.Id, role = user.Role });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Usuário ou senha inválidos" });
        }

        // Gerar tokens
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
        
        // Salvar refresh token no banco
        var savedRefreshToken = await _tokenService.SaveRefreshTokenAsync(user.Id, refreshToken);

        var jwtSettings = _configuration.GetSection("JwtSettings");
        var accessTokenExpiration = DateTime.UtcNow.AddMinutes(
            int.Parse(jwtSettings["AccessTokenExpirationMinutes"]));

        return Ok(new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiration = accessTokenExpiration,
            RefreshTokenExpiration = savedRefreshToken.ExpiresAt,
            Username = user.Username,
            Role = user.Role
        });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var refreshToken = await _tokenService.GetRefreshTokenAsync(request.RefreshToken);

        if (refreshToken == null || !refreshToken.IsActive)
        {
            return Unauthorized(new { message = "Refresh token inválido ou expirado" });
        }

        // Revogar o refresh token antigo
        await _tokenService.RevokeRefreshTokenAsync(request.RefreshToken);

        // Gerar novos tokens
        var user = refreshToken.User;
        var newAccessToken = _tokenService.GenerateAccessToken(user);
        var newRefreshToken = _tokenService.GenerateRefreshToken();
        
        // Salvar novo refresh token
        var savedRefreshToken = await _tokenService.SaveRefreshTokenAsync(user.Id, newRefreshToken);

        var jwtSettings = _configuration.GetSection("JwtSettings");
        var accessTokenExpiration = DateTime.UtcNow.AddMinutes(
            int.Parse(jwtSettings["AccessTokenExpirationMinutes"]));

        return Ok(new LoginResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            AccessTokenExpiration = accessTokenExpiration,
            RefreshTokenExpiration = savedRefreshToken.ExpiresAt,
            Username = user.Username,
            Role = user.Role
        });
    }

    [Authorize]
    [HttpPost("revoke")]
    public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenRequest request)
    {
        await _tokenService.RevokeRefreshTokenAsync(request.RefreshToken);
        return Ok(new { message = "Token revogado com sucesso" });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        await _tokenService.RevokeAllUserRefreshTokensAsync(userId);
        return Ok(new { message = "Logout realizado com sucesso" });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        var user = await _context.Users
            .Where(u => u.Username == username)
            .Select(u => new
            {
                u.Id,
                u.Username,
                u.Email,
                u.Role,
                u.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (user == null)
        {
            return NotFound(new { message = "Usuário não encontrado" });
        }

        return Ok(user);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _context.Users
            .Select(u => new
            {
                u.Id,
                u.Username,
                u.Email,
                u.Role,
                u.CreatedAt
            })
            .ToListAsync();

        return Ok(users);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPut("users/{id}/role")]
    public async Task<IActionResult> UpdateUserRole(int id, [FromBody] UpdateRoleRequest request)
    {
        var user = await _context.Users.FindAsync(id);
        
        if (user == null)
        {
            return NotFound(new { message = "Usuário não encontrado" });
        }

        var validRoles = new[] { "Admin", "User", "Manager" };
        if (!validRoles.Contains(request.Role))
        {
            return BadRequest(new { message = "Role inválida. Use: Admin, User ou Manager" });
        }

        user.Role = request.Role;
        user.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();

        return Ok(new { message = "Role atualizada com sucesso", userId = user.Id, newRole = user.Role });
    }
}

public class UpdateRoleRequest
{
    public string Role { get; set; }
}