using CatalogoApi.Data;
using CatalogoApi.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogoApi.Controllers;

[ApiController]
[Route("api/reportes")]
public class ReportesController : ControllerBase
{
    private readonly AppDbContext _context;

    public ReportesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("catalogo-abraham")]
    [Authorize(Policy = "ReporteAbrahamPolicy")]
    public async Task<ActionResult<ReporteCatalogoAbrahamDto>> GetReporteCatalogoAbraham()
    {
        var categorias = await _context.Categorias
            .AsNoTracking()
            .Where(c => c.Activo)
            .OrderBy(c => c.Nombre)
            .Select(c => new { c.Id, c.Nombre })
            .ToListAsync();

        var peliculas = await _context.Peliculas
            .AsNoTracking()
            .Where(p => p.Activo)
            .OrderBy(p => p.Titulo)
            .Select(p => new { p.Id, p.Titulo, p.DuracionMin, p.CategoriaId })
            .ToListAsync();

        var series = await _context.Series
            .AsNoTracking()
            .Where(s => s.Activo)
            .OrderBy(s => s.Titulo)
            .Select(s => new { s.Id, s.Titulo, s.CategoriaId })
            .ToListAsync();

        var temporadas = await _context.Temporadas
            .AsNoTracking()
            .Where(t => t.Activo && t.Serie.Activo)
            .Select(t => new { t.Id, t.SerieId, t.CantidadEpisodios })
            .ToListAsync();

        var temporadasPorSerie = temporadas
            .GroupBy(t => t.SerieId)
            .ToDictionary(
                g => g.Key,
                g => new
                {
                    TotalTemporadas = g.Count(),
                    TotalEpisodios = g.Sum(t => t.CantidadEpisodios)
                });

        var categoriasActivasIds = categorias.Select(c => c.Id).ToHashSet();

        var desglose = new List<ReporteCategoriaDesgloseDto>();

        foreach (var categoria in categorias)
        {
            var peliculasCat = peliculas
                .Where(p => p.CategoriaId == categoria.Id)
                .ToList();

            var seriesCat = series
                .Where(s => s.CategoriaId == categoria.Id)
                .ToList();

            var totalTemporadasCat = seriesCat.Sum(s =>
                temporadasPorSerie.TryGetValue(s.Id, out var stats) ? stats.TotalTemporadas : 0);

            var totalEpisodiosCat = seriesCat.Sum(s =>
                temporadasPorSerie.TryGetValue(s.Id, out var stats) ? stats.TotalEpisodios : 0);

            var titulos = peliculasCat
                .Select(p => $"{p.Titulo} (Película)")
                .Concat(seriesCat.Select(s => $"{s.Titulo} (Serie)"))
                .ToList();

            desglose.Add(new ReporteCategoriaDesgloseDto
            {
                CategoriaId = categoria.Id,
                NombreCategoria = categoria.Nombre,
                CantidadPeliculas = peliculasCat.Count,
                CantidadSeries = seriesCat.Count,
                TotalTemporadas = totalTemporadasCat,
                TotalEpisodios = totalEpisodiosCat,
                Titulos = titulos
            });
        }

        var peliculasSinCategoria = peliculas
            .Where(p => !p.CategoriaId.HasValue || !categoriasActivasIds.Contains(p.CategoriaId.Value))
            .ToList();

        var seriesSinCategoria = series
            .Where(s => !s.CategoriaId.HasValue || !categoriasActivasIds.Contains(s.CategoriaId.Value))
            .ToList();

        if (peliculasSinCategoria.Count > 0 || seriesSinCategoria.Count > 0)
        {
            desglose.Add(new ReporteCategoriaDesgloseDto
            {
                CategoriaId = null,
                NombreCategoria = "Sin categoría",
                CantidadPeliculas = peliculasSinCategoria.Count,
                CantidadSeries = seriesSinCategoria.Count,
                TotalTemporadas = seriesSinCategoria.Sum(s =>
                    temporadasPorSerie.TryGetValue(s.Id, out var stats) ? stats.TotalTemporadas : 0),
                TotalEpisodios = seriesSinCategoria.Sum(s =>
                    temporadasPorSerie.TryGetValue(s.Id, out var stats) ? stats.TotalEpisodios : 0),
                Titulos = peliculasSinCategoria
                    .Select(p => $"{p.Titulo} (Película)")
                    .Concat(seriesSinCategoria.Select(s => $"{s.Titulo} (Serie)"))
                    .ToList()
            });
        }

        var reporte = new ReporteCatalogoAbrahamDto
        {
            Autor = "Abraham Valeriano",
            Tablas = new List<string> { "categorias", "peliculas", "series", "temporadas" },
            Proposito = "Consolidado del catálogo audiovisual activo por categoría combinando películas, series, temporadas y episodios.",
            GeneradoEn = DateTime.UtcNow,
            Resumen = new ReporteResumenGeneralDto
            {
                TotalCategorias = categorias.Count,
                TotalPeliculas = peliculas.Count,
                TotalSeries = series.Count,
                TotalTemporadas = temporadas.Count,
                TotalEpisodios = temporadas.Sum(t => t.CantidadEpisodios),
                DuracionPromedioPeliculasMin = peliculas.Count > 0
                    ? Math.Round(peliculas.Average(p => p.DuracionMin), 2)
                    : 0
            },
            DesglosePorCategoria = desglose
        };

        return Ok(reporte);
    }

    /// <summary>
    /// Reporte autorizado de comunidad (Marco Antonio Arias Giron).
    /// Combina resenas, listas_reproduccion, favoritos y premios activos.
    /// </summary>
    [HttpGet("comunidad-marco")]
    [Authorize(Policy = "ReporteMarcoPolicy")]
    public async Task<ActionResult<ReporteComunidadMarcoDto>> GetReporteComunidadMarco()
    {
        var resenas = await _context.Resenas
            .AsNoTracking()
            .Where(r => r.Activo)
            .Select(r => new { r.TituloObra, r.Puntuacion })
            .ToListAsync();

        var favoritos = await _context.Favoritos
            .AsNoTracking()
            .Where(f => f.Activo)
            .Select(f => new { f.TituloObra })
            .ToListAsync();

        var premios = await _context.Premios
            .AsNoTracking()
            .Where(p => p.Activo)
            .OrderByDescending(p => p.Anio)
            .Select(p => new { p.ObraGanadora, p.Nombre, p.CategoriaPremio, p.Anio })
            .ToListAsync();

        var listas = await _context.ListasReproduccion
            .AsNoTracking()
            .Where(l => l.Activo)
            .OrderBy(l => l.Nombre)
            .Select(l => new ReporteListaResumenDto
            {
                Id = l.Id,
                Nombre = l.Nombre,
                Propietario = l.Propietario,
                EsPublica = l.EsPublica,
                Descripcion = l.Descripcion
            })
            .ToListAsync();

        var resenasPorObra = resenas
            .GroupBy(r => r.TituloObra.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => new
                {
                    DisplayTitle = g.First().TituloObra.Trim(),
                    Cantidad = g.Count(),
                    Promedio = Math.Round(g.Average(x => x.Puntuacion), 2)
                },
                StringComparer.OrdinalIgnoreCase);

        var favoritosPorObra = favoritos
            .GroupBy(f => f.TituloObra.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => new
                {
                    DisplayTitle = g.First().TituloObra.Trim(),
                    Cantidad = g.Count()
                },
                StringComparer.OrdinalIgnoreCase);

        var premiosPorObra = premios
            .GroupBy(p => p.ObraGanadora.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => new
                {
                    DisplayTitle = g.First().ObraGanadora.Trim(),
                    Cantidad = g.Count(),
                    Lista = g.Select(p => $"{p.Nombre} - {p.CategoriaPremio} ({p.Anio})").ToList()
                },
                StringComparer.OrdinalIgnoreCase);

        var todasLasObras = resenasPorObra.Keys
            .Concat(favoritosPorObra.Keys)
            .Concat(premiosPorObra.Keys)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(t => t, StringComparer.OrdinalIgnoreCase);

        var desglosePorObra = new List<ReporteObraComunidadDto>();
        foreach (var clave in todasLasObras)
        {
            resenasPorObra.TryGetValue(clave, out var rInfo);
            favoritosPorObra.TryGetValue(clave, out var fInfo);
            premiosPorObra.TryGetValue(clave, out var pInfo);

            var tituloDisplay = rInfo?.DisplayTitle ?? fInfo?.DisplayTitle ?? pInfo?.DisplayTitle ?? clave;

            desglosePorObra.Add(new ReporteObraComunidadDto
            {
                TituloObra = tituloDisplay,
                CantidadResenas = rInfo?.Cantidad ?? 0,
                PromedioPuntuacion = rInfo?.Promedio ?? 0,
                CantidadFavoritos = fInfo?.Cantidad ?? 0,
                CantidadPremios = pInfo?.Cantidad ?? 0,
                Premios = pInfo?.Lista ?? new List<string>()
            });
        }

        var reporte = new ReporteComunidadMarcoDto
        {
            Autor = "Marco Antonio Arias Giron",
            Tablas = new List<string> { "resenas", "listas_reproduccion", "favoritos", "premios" },
            Proposito = "Consolidado de actividad de la comunidad combinando reseñas, favoritos, premios ganados y listas de reproducción activas por obra.",
            GeneradoEn = DateTime.UtcNow,
            Resumen = new ReporteComunidadResumenDto
            {
                TotalResenas = resenas.Count,
                PromedioPuntuacionResenas = resenas.Count > 0
                    ? Math.Round(resenas.Average(r => r.Puntuacion), 2)
                    : 0,
                TotalFavoritos = favoritos.Count,
                TotalPremios = premios.Count,
                TotalListasReproduccion = listas.Count,
                ListasPublicas = listas.Count(l => l.EsPublica)
            },
            DesglosePorObra = desglosePorObra,
            ListasReproduccion = listas
        };

        return Ok(reporte);
    }

    /// <summary>
    /// Reporte autorizado de talento y producción (Emanuel Rafael Kilibarda Rocha).
    /// Combina directores, actores, productoras y plataformas de distribución activas por región geográfica.
    /// </summary>
    [HttpGet("talento-emanuel")]
    [Authorize(Policy = "ReporteEmanuelPolicy")]
    public async Task<ActionResult<ReporteTalentoEmanuelDto>> GetReporteTalentoEmanuel()
    {
        var directores = await _context.Directores
            .AsNoTracking()
            .Where(d => d.Activo)
            .OrderBy(d => d.Nombre)
            .Select(d => new { d.Id, d.Nombre, Nacionalidad = d.Nacionalidad ?? string.Empty })
            .ToListAsync();

        var actores = await _context.Actores
            .AsNoTracking()
            .Where(a => a.Activo)
            .OrderBy(a => a.Nombre)
            .Select(a => new { a.Id, a.Nombre, Nacionalidad = a.Nacionalidad ?? string.Empty })
            .ToListAsync();

        var productoras = await _context.Productoras
            .AsNoTracking()
            .Where(p => p.Activo)
            .OrderBy(p => p.Nombre)
            .Select(p => new { p.Id, p.Nombre, Pais = p.Pais ?? string.Empty, p.AnioFundacion, p.SitioWeb })
            .ToListAsync();

        var plataformas = await _context.Plataformas
            .AsNoTracking()
            .Where(p => p.Activo)
            .OrderBy(p => p.Nombre)
            .Select(p => new ReportePlataformaResumenDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                SitioWeb = p.SitioWeb,
                PrecioMensual = p.PrecioMensual,
                PaisDisponible = p.PaisDisponible
            })
            .ToListAsync();

        static string NormalizarPais(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor)) return "Sin especificar";
            var trim = valor.Trim();
            return string.IsNullOrEmpty(trim) ? "Sin especificar" : trim;
        }

        var directoresPorPais = directores
            .GroupBy(d => NormalizarPais(d.Nacionalidad), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => new
                {
                    Pais = string.IsNullOrWhiteSpace(g.First().Nacionalidad) ? g.Key : g.First().Nacionalidad.Trim(),
                    Directores = g.Select(d => d.Nombre).ToList()
                },
                StringComparer.OrdinalIgnoreCase);

        var actoresPorPais = actores
            .GroupBy(a => NormalizarPais(a.Nacionalidad), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => new
                {
                    Pais = string.IsNullOrWhiteSpace(g.First().Nacionalidad) ? g.Key : g.First().Nacionalidad.Trim(),
                    Actores = g.Select(a => a.Nombre).ToList()
                },
                StringComparer.OrdinalIgnoreCase);

        var productorasPorPais = productoras
            .GroupBy(p => NormalizarPais(p.Pais), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => new
                {
                    Pais = string.IsNullOrWhiteSpace(g.First().Pais) ? g.Key : g.First().Pais.Trim(),
                    Productoras = g.Select(p => p.Nombre).ToList()
                },
                StringComparer.OrdinalIgnoreCase);

        var todosLosPaises = directoresPorPais.Keys
            .Concat(actoresPorPais.Keys)
            .Concat(productorasPorPais.Keys)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(p => p.Equals("Sin especificar", StringComparison.OrdinalIgnoreCase) ? 1 : 0)
            .ThenBy(p => p, StringComparer.OrdinalIgnoreCase);

        var desglosePorPais = new List<ReporteRegionDesgloseDto>();
        foreach (var clave in todosLosPaises)
        {
            directoresPorPais.TryGetValue(clave, out var dInfo);
            actoresPorPais.TryGetValue(clave, out var aInfo);
            productorasPorPais.TryGetValue(clave, out var pInfo);

            var directoresList = dInfo?.Directores ?? new List<string>();
            var actoresList = aInfo?.Actores ?? new List<string>();
            var productorasList = pInfo?.Productoras ?? new List<string>();

            desglosePorPais.Add(new ReporteRegionDesgloseDto
            {
                Pais = clave,
                CantidadDirectores = directoresList.Count,
                CantidadActores = actoresList.Count,
                TotalTalento = directoresList.Count + actoresList.Count,
                CantidadProductoras = productorasList.Count,
                Directores = directoresList,
                Actores = actoresList,
                Productoras = productorasList
            });
        }

        var precios = plataformas
            .Where(p => p.PrecioMensual.HasValue)
            .Select(p => p.PrecioMensual!.Value)
            .ToList();

        var precioPromedio = precios.Count > 0
            ? Math.Round(precios.Average(), 2)
            : 0m;

        var totalPaises = todosLosPaises.Count(p => !p.Equals("Sin especificar", StringComparison.OrdinalIgnoreCase));

        var reporte = new ReporteTalentoEmanuelDto
        {
            Autor = "Emanuel Rafael Kilibarda Rocha",
            Tablas = new List<string> { "directores", "actores", "productoras", "plataformas" },
            Proposito = "Consolidado de talento y producción cinematográfica cruzando directores, actores y casas productoras por región geográfica, junto con los canales de distribución y streaming activos.",
            GeneradoEn = DateTime.UtcNow,
            Resumen = new ReporteTalentoResumenDto
            {
                TotalDirectores = directores.Count,
                TotalActores = actores.Count,
                TotalTalento = directores.Count + actores.Count,
                TotalProductoras = productoras.Count,
                TotalPlataformas = plataformas.Count,
                TotalPaisesRepresentados = totalPaises,
                PrecioPromedioSuscripcion = precioPromedio
            },
            DesglosePorPais = desglosePorPais,
            PlataformasDistribucion = plataformas
        };

        return Ok(reporte);
    }
}
