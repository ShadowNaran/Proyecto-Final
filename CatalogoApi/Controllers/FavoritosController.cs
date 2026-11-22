using CatalogoApi.Data;
using CatalogoApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogoApi.Controllers;

[ApiController]
[Route("api/favoritos")]
[Authorize]
public class FavoritosController : ControllerBase
{
    private readonly AppDbContext _context;

    public FavoritosController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var favoritos = await _context.Favoritos
            .Where(f => f.Activo)
            .ToListAsync();
        return Ok(favoritos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var favorito = await _context.Favoritos
            .FirstOrDefaultAsync(f => f.Id == id && f.Activo);

        if (favorito == null)
        {
            return NotFound(new { mensaje = $"Favorito con ID {id} no encontrado o no se encuentra activo." });
        }

        return Ok(favorito);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Favorito favorito)
    {
        if (string.IsNullOrWhiteSpace(favorito.UsuarioEmail) ||
            string.IsNullOrWhiteSpace(favorito.TipoContenido) ||
            string.IsNullOrWhiteSpace(favorito.TituloObra))
        {
            return BadRequest(new { mensaje = "usuario_email, tipo_contenido y titulo_obra son obligatorios." });
        }

        favorito.Activo = true;
        favorito.CreadoEn = DateTime.UtcNow;
        favorito.EliminadoEn = null;

        _context.Favoritos.Add(favorito);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = favorito.Id }, favorito);
    }
}
