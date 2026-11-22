using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CatalogoApi.Models;

[Table("resenas")]
public class Resena
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("autor")]
    public string Autor { get; set; } = string.Empty;

    [Required]
    [Column("tipo_contenido")]
    public string TipoContenido { get; set; } = string.Empty;

    [Required]
    [Column("titulo_obra")]
    public string TituloObra { get; set; } = string.Empty;

    [Range(1, 5)]
    [Column("puntuacion")]
    public int Puntuacion { get; set; }

    [Column("comentario")]
    public string? Comentario { get; set; }

    [Column("activo")]
    public bool Activo { get; set; } = true;

    [Column("creado_en")]
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

    [Column("eliminado_en")]
    public DateTime? EliminadoEn { get; set; }
}
