using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CatalogoApi.Models;

[Table("favoritos")]
public class Favorito
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("usuario_email")]
    public string UsuarioEmail { get; set; } = string.Empty;

    [Required]
    [Column("tipo_contenido")]
    public string TipoContenido { get; set; } = string.Empty;

    [Required]
    [Column("titulo_obra")]
    public string TituloObra { get; set; } = string.Empty;

    [Column("nota_personal")]
    public string? NotaPersonal { get; set; }

    [Column("activo")]
    public bool Activo { get; set; } = true;

    [Column("creado_en")]
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

    [Column("eliminado_en")]
    public DateTime? EliminadoEn { get; set; }
}
