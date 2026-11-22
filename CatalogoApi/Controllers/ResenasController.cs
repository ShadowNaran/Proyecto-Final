using CatalogoApi.Data;
using CatalogoApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogoApi.Controllers;

[ApiController]
[Route("api/resenas")]
[Authorize]
public class ResenasController : ControllerBase
{
    private readonly AppDbContext _context;

    public ResenasController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var resenas = await _context.Resenas
            .Where(r => r.Activo)
            .ToListAsync();
        return Ok(resenas);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var resena = await _context.Resenas
            .FirstOrDefaultAsync(r => r.Id == id && r.Activo);

        if (resena == null)
        {
            return NotFound(new { mensaje = $"Reseña con ID {id} no encontrada o no se encuentra activa." });
        }

        return Ok(resena);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Resena resena)
    {
        if (string.IsNullOrWhiteSpace(resena.Autor) ||
            string.IsNullOrWhiteSpace(resena.TipoContenido) ||
            string.IsNullOrWhiteSpace(resena.TituloObra))
        {
            return BadRequest(new { mensaje = "Autor, tipo_contenido y titulo_obra son obligatorios." });
        }

        if (resena.Puntuacion < 1 || resena.Puntuacion > 5)
        {
            return BadRequest(new { mensaje = "La puntuación debe estar entre 1 y 5." });
        }

        resena.Activo = true;
        resena.CreadoEn = DateTime.UtcNow;
        resena.EliminadoEn = null;

        _context.Resenas.Add(resena);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = resena.Id }, resena);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Resena resena)
    {
        if (id != resena.Id)
        {
            return BadRequest(new { mensaje = "El ID de la ruta no coincide con el ID del cuerpo." });
        }

        if (string.IsNullOrWhiteSpace(resena.Autor) ||
            string.IsNullOrWhiteSpace(resena.TipoContenido) ||
            string.IsNullOrWhiteSpace(resena.TituloObra))
        {
            return BadRequest(new { mensaje = "Autor, tipo_contenido y titulo_obra son obligatorios." });
        }

        if (resena.Puntuacion < 1 || resena.Puntuacion > 5)
        {
            return BadRequest(new { mensaje = "La puntuación debe estar entre 1 y 5." });
        }

        var existing = await _context.Resenas.FindAsync(id);
        if (existing == null || !existing.Activo)
        {
            return NotFound(new { mensaje = $"Reseña con ID {id} no existe o no se encuentra activa." });
        }

        existing.Autor = resena.Autor;
        existing.TipoContenido = resena.TipoContenido;
        existing.TituloObra = resena.TituloObra;
        existing.Puntuacion = resena.Puntuacion;
        existing.Comentario = resena.Comentario;

        await _context.SaveChangesAsync();

        return NoContent();
    }
}
