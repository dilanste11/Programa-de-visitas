using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVisitas.Api.Models;
using System.Text.Json;

namespace SistemaVisitas.Api.Controllers
{
    [Authorize(Roles = "Admin")] // Aislamiento: Solo los administradores pueden crear moldes
    [Route("api/[controller]")]
    [ApiController]
    public class TiposDispositivosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TiposDispositivosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/TiposDispositivos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TiposDispositivo>>> GetTiposDispositivos()
        {
            return await _context.TiposDispositivos.ToListAsync();
        }

        // GET: api/TiposDispositivos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TiposDispositivo>> GetTiposDispositivo(int id)
        {
            var TiposDispositivo = await _context.TiposDispositivos.FindAsync(id);

            if (TiposDispositivo == null)
            {
                return NotFound();
            }

            return TiposDispositivo;
        }

        // POST: api/TiposDispositivos
        [HttpPost]
        public async Task<ActionResult<TiposDispositivo>> PostTiposDispositivo(TiposDispositivo TiposDispositivo)
        {
            // Validar que el esquema cumpla con ser un JSON Array válido
            if (!ValidarEsquemaJSON(TiposDispositivo.EsquemaMolde))
            {
                return BadRequest("El EsquemaMolde debe ser un arreglo JSON válido (ej: [{...}, {...}]).");
            }

            _context.TiposDispositivos.Add(TiposDispositivo);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTiposDispositivo", new { id = TiposDispositivo.Id }, TiposDispositivo);
        }

        // PUT: api/TiposDispositivos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTiposDispositivo(int id, TiposDispositivo TiposDispositivo)
        {
            if (id != TiposDispositivo.Id)
            {
                return BadRequest();
            }

            if (!ValidarEsquemaJSON(TiposDispositivo.EsquemaMolde))
            {
                return BadRequest("El EsquemaMolde debe ser un arreglo JSON válido.");
            }

            _context.Entry(TiposDispositivo).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TiposDispositivoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/TiposDispositivos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTiposDispositivo(int id)
        {
            var TiposDispositivo = await _context.TiposDispositivos.FindAsync(id);
            if (TiposDispositivo == null)
            {
                return NotFound();
            }

            _context.TiposDispositivos.Remove(TiposDispositivo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TiposDispositivoExists(int id)
        {
            return _context.TiposDispositivos.Any(e => e.Id == id);
        }

        // Método auxiliar para garantizar la integridad del campo dinámico
        private bool ValidarEsquemaJSON(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return false;

            try
            {
                using var doc = JsonDocument.Parse(json);
                // Exigimos que el elemento raíz sea un Array, ya que será una lista de campos
                return doc.RootElement.ValueKind == JsonValueKind.Array;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}