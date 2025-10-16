using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MinhaAPI.Data;
using MinhaAPI.Models;
using MinhaAPI.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configuração do SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registrar serviços
builder.Services.AddScoped<ITokenService, TokenService>();

// Configuração do JWT
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero // Remove delay padrão de 5 minutos
    };
});

// Configuração de Policies de Autorização
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("UserOrAdmin", policy => policy.RequireRole("User", "Admin"));
    options.AddPolicy("ManagerOnly", policy => policy.RequireRole("Manager"));
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Criar banco de dados e seed inicial
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
    
    // Seed de dados inicial
    if (!dbContext.Users.Any())
    {
        var adminUser = new User
        {
            Username = "admin",
            Email = "admin@minhaapi.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            Role = "Admin",
            CreatedAt = DateTime.UtcNow
        };
        
        var regularUser = new User
        {
            Username = "user",
            Email = "user@minhaapi.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("user123"),
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };
        
        dbContext.Users.AddRange(adminUser, regularUser);
        dbContext.SaveChanges();
        
        Console.WriteLine("✅ Usuários padrão criados:");
        Console.WriteLine("   Admin - username: admin, password: admin123");
        Console.WriteLine("   User  - username: user, password: user123");
    }
    
    // Seed de produtos
    if (!dbContext.Products.Any())
    {
        var products = new[]
        {
            new Product
            {
                Name = "Notebook Dell",
                Description = "Notebook Dell Inspiron 15, 8GB RAM, 256GB SSD",
                Price = 3500.00m,
                Stock = 15,
                Category = "Eletrônicos",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Name = "Mouse Logitech",
                Description = "Mouse sem fio Logitech MX Master 3",
                Price = 350.00m,
                Stock = 50,
                Category = "Periféricos",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Name = "Teclado Mecânico",
                Description = "Teclado mecânico RGB, switches blue",
                Price = 450.00m,
                Stock = 30,
                Category = "Periféricos",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Name = "Monitor LG 27",
                Description = "Monitor LG 27 polegadas, Full HD, IPS",
                Price = 1200.00m,
                Stock = 20,
                Category = "Eletrônicos",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Name = "Webcam Logitech C920",
                Description = "Webcam Full HD 1080p",
                Price = 550.00m,
                Stock = 0,
                Category = "Periféricos",
                IsActive = false,
                CreatedAt = DateTime.UtcNow
            }
        };
        
        dbContext.Products.AddRange(products);
        dbContext.SaveChanges();
        
        Console.WriteLine("✅ Produtos de exemplo criados!");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();