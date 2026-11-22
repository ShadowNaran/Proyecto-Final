using CatalogoApi.Data;
using CatalogoApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogoApi.Controllers;

[ApiController]
[Route("api/actores")]
[Authorize]
public class ActoresController : ControllerBase
{
    private readonly AppDbContext _context;

    public ActoresController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var actores = await _context.Actores.ToListAsync();
        return Ok(actores);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var actor = await _context.Actores.FindAsync(id);
        if (actor == null)
        {
            return NotFound(new { mensaje = $"Actor con ID {id} no encontrado." });
        }
        return Ok(actor);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Actor actor)
    {
        if (string.IsNullOrWhiteSpace(actor.Nombre))
        {
            return BadRequest(new { mensaje = "El nombre del actor es obligatorio." });
        }

        actor.CreadoEn = DateTime.UtcNow;
        _context.Actores.Add(actor);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = actor.Id }, actor);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Actor actor)
    {
        if (id != actor.Id)
        {
            return BadRequest(new { mensaje = "El ID de la ruta no coincide con el ID del cuerpo." });
        }

        if (string.IsNullOrWhiteSpace(actor.Nombre))
        {
            return BadRequest(new { mensaje = "El nombre del actor es obligatorio." });
        }

        var existing = await _context.Actores.FindAsync(id);
        if (existing == null)
        {
            return NotFound(new { mensaje = $"Actor con ID {id} no encontrado." });
        }

        existing.Nombre = actor.Nombre;
        existing.Nacionalidad = actor.Nacionalidad;
        existing.FechaNacimiento = actor.FechaNacimiento;
        existing.Biografia = actor.Biografia;
        existing.Activo = actor.Activo;
        if (actor.EliminadoEn != null)
        {
            existing.EliminadoEn = actor.EliminadoEn;
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var actor = await _context.Actores.FindAsync(id);
        if (actor == null)
        {
            return NotFound(new { mensaje = $"Actor con ID {id} no encontrado." });
        }

        _context.Actores.Remove(actor);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
