namespace CatalogoApi.Dtos;

/// <summary>
/// DTO principal del reporte autorizado de talento y producción de Emanuel Rafael Kilibarda Rocha.
/// Combina y cruza directores, actores, productoras y plataformas activas.
/// </summary>
public class ReporteTalentoEmanuelDto
{
    public string Autor { get; set; } = "Emanuel Rafael Kilibarda Rocha";
    public List<string> Tablas { get; set; } = new() { "directores", "actores", "productoras", "plataformas" };
    public string Proposito { get; set; } = string.Empty;
    public DateTime GeneradoEn { get; set; }
    public ReporteTalentoResumenDto Resumen { get; set; } = new();
    public List<ReporteRegionDesgloseDto> DesglosePorPais { get; set; } = new();
    public List<ReportePlataformaResumenDto> PlataformasDistribucion { get; set; } = new();
}

/// <summary>
/// Métricas globales del talento y ecosistema de producción y distribución audiovisual.
/// </summary>
public class ReporteTalentoResumenDto
{
    public int TotalDirectores { get; set; }
    public int TotalActores { get; set; }
    public int TotalTalento { get; set; }
    public int TotalProductoras { get; set; }
    public int TotalPlataformas { get; set; }
    public int TotalPaisesRepresentados { get; set; }
    public decimal PrecioPromedioSuscripcion { get; set; }
}

/// <summary>
/// Desglose combinado de directores, actores y productoras agrupados por país o nacionalidad.
/// </summary>
public class ReporteRegionDesgloseDto
{
    public string Pais { get; set; } = string.Empty;
    public int CantidadDirectores { get; set; }
    public int CantidadActores { get; set; }
    public int TotalTalento { get; set; }
    public int CantidadProductoras { get; set; }
    public List<string> Directores { get; set; } = new();
    public List<string> Actores { get; set; } = new();
    public List<string> Productoras { get; set; } = new();
}

/// <summary>
/// Resumen de plataformas de distribución activas y su costo.
/// </summary>
public class ReportePlataformaResumenDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? SitioWeb { get; set; }
    public decimal? PrecioMensual { get; set; }
    public string? PaisDisponible { get; set; }
}
