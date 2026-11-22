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
}
