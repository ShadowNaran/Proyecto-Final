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

    public DbSet<Categoria> Categorias { get; set; }
    public DbSet<Pelicula> Peliculas { get; set; }
    public DbSet<Serie> Series { get; set; }
    public DbSet<Temporada> Temporadas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Productora>()
            .HasIndex(p => p.Nombre)
            .IsUnique();

        modelBuilder.Entity<Plataforma>()
            .HasIndex(p => p.Nombre)
            .IsUnique();
        
        modelBuilder.Entity<Categoria>()
            .HasIndex(c => c.Nombre)
            .IsUnique();

        modelBuilder.Entity<Pelicula>()
            .HasOne(p => p.Categoria)
            .WithMany(c => c.Peliculas)
            .HasForeignKey(p => p.CategoriaId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Serie>()
            .HasOne(s => s.Categoria)
            .WithMany(c => c.Series)
            .HasForeignKey(s => s.CategoriaId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Temporada>()
            .HasOne(t => t.Serie)
            .WithMany(s => s.Temporadas)
            .HasForeignKey(t => t.SerieId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
