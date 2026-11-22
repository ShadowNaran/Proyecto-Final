using CatalogoApi.Data;
using CatalogoApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogoApi.Controllers;

[ApiController]
[Route("api/directores")]
[Authorize]
public class DirectoresController : ControllerBase
{
    private readonly AppDbContext _context;

    public DirectoresController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var directores = await _context.Directores
            .Where(d => d.Activo)
            .ToListAsync();
        return Ok(directores);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var director = await _context.Directores
            .FirstOrDefaultAsync(d => d.Id == id && d.Activo);

        if (director == null)
        {
            return NotFound(new { mensaje = $"Director con ID {id} no encontrado." });
        }
        return Ok(director);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Director director)
    {
        if (string.IsNullOrWhiteSpace(director.Nombre))
        {
            return BadRequest(new { mensaje = "El nombre del director es obligatorio." });
        }

        director.CreadoEn = DateTime.UtcNow;
        _context.Directores.Add(director);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = director.Id }, director);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Director director)
    {
        if (id != director.Id)
        {
            return BadRequest(new { mensaje = "El ID de la ruta no coincide con el ID del cuerpo." });
        }

        if (string.IsNullOrWhiteSpace(director.Nombre))
        {
            return BadRequest(new { mensaje = "El nombre del director es obligatorio." });
        }

        var existing = await _context.Directores.FindAsync(id);
        if (existing == null)
        {
            return NotFound(new { mensaje = $"Director con ID {id} no encontrado." });
        }

        existing.Nombre = director.Nombre;
        existing.Nacionalidad = director.Nacionalidad;
        existing.FechaNacimiento = director.FechaNacimiento;
        existing.Biografia = director.Biografia;
        existing.Activo = director.Activo;
        if (director.EliminadoEn != null)
        {
            existing.EliminadoEn = director.EliminadoEn;
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var director = await _context.Directores.FindAsync(id);
        if (director == null)
        {
            return NotFound(new { mensaje = $"Director con ID {id} no encontrado." });
        }

        _context.Directores.Remove(director);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
