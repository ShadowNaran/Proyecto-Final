using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CatalogoApi.Models;

[Table("productoras")]
public class Productora
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Column("pais")]
    public string? Pais { get; set; }

    [Column("anio_fundacion")]
    public int? AnioFundacion { get; set; }

    [Column("sitio_web")]
    public string? SitioWeb { get; set; }

    [Column("activo")]
    public bool Activo { get; set; } = true;

    [Column("creado_en")]
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

    [Column("eliminado_en")]
    public DateTime? EliminadoEn { get; set; }
}
