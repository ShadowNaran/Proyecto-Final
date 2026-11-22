using CatalogoApi.Data;
using CatalogoApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogoApi.Controllers;

[ApiController]
[Route("api/listas-reproduccion")]
[Authorize]
public class ListasReproduccionController : ControllerBase
{
    private readonly AppDbContext _context;

    public ListasReproduccionController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var listas = await _context.ListasReproduccion
            .Where(l => l.Activo)
            .ToListAsync();
        return Ok(listas);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var lista = await _context.ListasReproduccion
            .FirstOrDefaultAsync(l => l.Id == id && l.Activo);

        if (lista == null)
        {
            return NotFound(new { mensaje = $"Lista de reproducción con ID {id} no encontrada o no se encuentra activa." });
        }

        return Ok(lista);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ListaReproduccion lista)
    {
        if (string.IsNullOrWhiteSpace(lista.Nombre) ||
            string.IsNullOrWhiteSpace(lista.Propietario))
        {
            return BadRequest(new { mensaje = "El nombre y el propietario son obligatorios." });
        }

        lista.Activo = true;
        lista.CreadoEn = DateTime.UtcNow;
        lista.EliminadoEn = null;

        _context.ListasReproduccion.Add(lista);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = lista.Id }, lista);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ListaReproduccion lista)
    {
        if (id != lista.Id)
        {
            return BadRequest(new { mensaje = "El ID de la ruta no coincide con el ID del cuerpo." });
        }

        if (string.IsNullOrWhiteSpace(lista.Nombre) ||
            string.IsNullOrWhiteSpace(lista.Propietario))
        {
            return BadRequest(new { mensaje = "El nombre y el propietario son obligatorios." });
        }

        var existing = await _context.ListasReproduccion.FindAsync(id);
        if (existing == null || !existing.Activo)
        {
            return NotFound(new { mensaje = $"Lista de reproducción con ID {id} no existe o no se encuentra activa." });
        }

        existing.Nombre = lista.Nombre;
        existing.Descripcion = lista.Descripcion;
        existing.EsPublica = lista.EsPublica;
        existing.Propietario = lista.Propietario;

        await _context.SaveChangesAsync();

        return NoContent();
    }
}
