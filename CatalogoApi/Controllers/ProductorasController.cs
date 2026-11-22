using CatalogoApi.Data;
using CatalogoApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogoApi.Controllers;

[ApiController]
[Route("api/productoras")]
[Authorize]
public class ProductorasController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductorasController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var productoras = await _context.Productoras.ToListAsync();
        return Ok(productoras);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var productora = await _context.Productoras.FindAsync(id);
        if (productora == null)
        {
            return NotFound(new { mensaje = $"Productora con ID {id} no encontrada." });
        }
        return Ok(productora);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Productora productora)
    {
        if (string.IsNullOrWhiteSpace(productora.Nombre))
        {
            return BadRequest(new { mensaje = "El nombre de la productora es obligatorio." });
        }

        var existeNombre = await _context.Productoras
            .AnyAsync(p => p.Nombre.ToLower() == productora.Nombre.Trim().ToLower());
        if (existeNombre)
        {
            return Conflict(new { mensaje = $"Ya existe una productora con el nombre '{productora.Nombre}'." });
        }

        productora.CreadoEn = DateTime.UtcNow;
        _context.Productoras.Add(productora);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            if (await _context.Productoras.AnyAsync(p => p.Nombre.ToLower() == productora.Nombre.Trim().ToLower()))
            {
                return Conflict(new { mensaje = $"Ya existe una productora con el nombre '{productora.Nombre}'." });
            }
            throw;
        }

        return CreatedAtAction(nameof(GetById), new { id = productora.Id }, productora);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Productora productora)
    {
        if (id != productora.Id)
        {
            return BadRequest(new { mensaje = "El ID de la ruta no coincide con el ID del cuerpo." });
        }

        if (string.IsNullOrWhiteSpace(productora.Nombre))
        {
            return BadRequest(new { mensaje = "El nombre de la productora es obligatorio." });
        }

        var existing = await _context.Productoras.FindAsync(id);
        if (existing == null)
        {
            return NotFound(new { mensaje = $"Productora con ID {id} no encontrada." });
        }

        var existeNombre = await _context.Productoras
            .AnyAsync(p => p.Id != id && p.Nombre.ToLower() == productora.Nombre.Trim().ToLower());
        if (existeNombre)
        {
            return Conflict(new { mensaje = $"Ya existe otra productora con el nombre '{productora.Nombre}'." });
        }

        existing.Nombre = productora.Nombre;
        existing.Pais = productora.Pais;
        existing.AnioFundacion = productora.AnioFundacion;
        existing.SitioWeb = productora.SitioWeb;
        existing.Activo = productora.Activo;
        if (productora.EliminadoEn != null)
        {
            existing.EliminadoEn = productora.EliminadoEn;
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            if (await _context.Productoras.AnyAsync(p => p.Id != id && p.Nombre.ToLower() == productora.Nombre.Trim().ToLower()))
            {
                return Conflict(new { mensaje = $"Ya existe otra productora con el nombre '{productora.Nombre}'." });
            }
            throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var productora = await _context.Productoras.FindAsync(id);
        if (productora == null)
        {
            return NotFound(new { mensaje = $"Productora con ID {id} no encontrada." });
        }

        _context.Productoras.Remove(productora);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
