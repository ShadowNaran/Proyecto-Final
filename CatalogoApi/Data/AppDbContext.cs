using CatalogoApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CatalogoApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Director> Directores { get; set; }
    public DbSet<Actor> Actores { get; set; }
    public DbSet<Productora> Productoras { get; set; }
    public DbSet<Plataforma> Plataformas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Productora>()
            .HasIndex(p => p.Nombre)
            .IsUnique();

        modelBuilder.Entity<Plataforma>()
            .HasIndex(p => p.Nombre)
            .IsUnique();
    }
}
