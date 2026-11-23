using CatalogoApi.Data;
using CatalogoApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogoApi.Controllers;

[ApiController]
[Route("api/premios")]
[Authorize]
public class PremiosController : ControllerBase
{
    private readonly AppDbContext _context;

    public PremiosController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var premios = await _context.Premios
            .Where(p => p.Activo)
            .ToListAsync();
        return Ok(premios);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var premio = await _context.Premios
            .FirstOrDefaultAsync(p => p.Id == id && p.Activo);

        if (premio == null)
        {
            return NotFound(new { mensaje = $"Premio con ID {id} no encontrado o no se encuentra activo." });
        }

        return Ok(premio);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Premio premio)
    {
        if (string.IsNullOrWhiteSpace(premio.Nombre) ||
            string.IsNullOrWhiteSpace(premio.CategoriaPremio) ||
            string.IsNullOrWhiteSpace(premio.ObraGanadora))
        {
            return BadRequest(new { mensaje = "nombre, categoria_premio y obra_ganadora son obligatorios." });
        }

        premio.Activo = true;
        premio.CreadoEn = DateTime.UtcNow;
        premio.EliminadoEn = null;

        _context.Premios.Add(premio);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = premio.Id }, premio);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Premio premio)
    {
        if (id != premio.Id)
        {
            return BadRequest(new { mensaje = "El ID de la ruta no coincide con el ID del cuerpo." });
        }

        if (string.IsNullOrWhiteSpace(premio.Nombre) ||
            string.IsNullOrWhiteSpace(premio.CategoriaPremio) ||
            string.IsNullOrWhiteSpace(premio.ObraGanadora))
        {
            return BadRequest(new { mensaje = "nombre, categoria_premio y obra_ganadora son obligatorios." });
        }

        var existing = await _context.Premios.FindAsync(id);
        if (existing == null || !existing.Activo)
        {
            return NotFound(new { mensaje = $"Premio con ID {id} no existe o no se encuentra activo." });
        }

        existing.Nombre = premio.Nombre;
        existing.CategoriaPremio = premio.CategoriaPremio;
        existing.Anio = premio.Anio;
        existing.ObraGanadora = premio.ObraGanadora;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var premio = await _context.Premios.FindAsync(id);
        if (premio == null || !premio.Activo)
        {
            return NotFound(new { mensaje = $"Premio con ID {id} no existe o ya fue eliminado." });
        }

        premio.Activo = false;
        premio.EliminadoEn = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
