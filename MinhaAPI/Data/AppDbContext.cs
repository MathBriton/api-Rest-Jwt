using Microsoft.EntityFrameworkCore;
using MinhaAPI.Models;

namespace MinhaAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User indexes
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Product indexes
        modelBuilder.Entity<Product>()
            .HasIndex(p => p.Name);

        modelBuilder.Entity<Product>()
            .HasIndex(p => p.Category);

        modelBuilder.Entity<Product>()
            .HasIndex(p => p.IsActive);

        // Decimal precision
        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasPrecision(18, 2);
    }
}