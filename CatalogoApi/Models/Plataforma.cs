using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CatalogoApi.Models;

[Table("plataformas")]
public class Plataforma
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Column("sitio_web")]
    public string? SitioWeb { get; set; }

    [Column("precio_mensual")]
    public decimal? PrecioMensual { get; set; }

    [Column("pais_disponible")]
    public string? PaisDisponible { get; set; }

    [Column("activo")]
    public bool Activo { get; set; } = true;

    [Column("creado_en")]
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

    [Column("eliminado_en")]
    public DateTime? EliminadoEn { get; set; }
}
