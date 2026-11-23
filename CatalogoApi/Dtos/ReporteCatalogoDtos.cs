namespace CatalogoApi.Dtos;

public class ReporteCatalogoAbrahamDto
{
    public string Autor { get; set; } = "Abraham Valeriano";
    public List<string> Tablas { get; set; } = new() { "categorias", "peliculas", "series", "temporadas" };
    public string Proposito { get; set; } = string.Empty;
    public DateTime GeneradoEn { get; set; }
    public ReporteResumenGeneralDto Resumen { get; set; } = new();
    public List<ReporteCategoriaDesgloseDto> DesglosePorCategoria { get; set; } = new();
}

public class ReporteResumenGeneralDto
{
    public int TotalCategorias { get; set; }
    public int TotalPeliculas { get; set; }
    public int TotalSeries { get; set; }
    public int TotalTemporadas { get; set; }
    public int TotalEpisodios { get; set; }
    public double DuracionPromedioPeliculasMin { get; set; }
}

public class ReporteCategoriaDesgloseDto
{
    public int? CategoriaId { get; set; }
    public string NombreCategoria { get; set; } = string.Empty;
    public int CantidadPeliculas { get; set; }
    public int CantidadSeries { get; set; }
    public int TotalTemporadas { get; set; }
    public int TotalEpisodios { get; set; }
    public List<string> Titulos { get; set; } = new();
}
