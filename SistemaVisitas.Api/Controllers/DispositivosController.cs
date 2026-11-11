using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVisitas.Api.Extensions;
using SistemaVisitas.Api.Models;
using System.Text.Json;
using SistemaVisitas.Shared.DTOs;

namespace SistemaVisitas.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DispositivosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DispositivosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =======================================================
        // GET: api/Dispositivos (Trae todos los equipos)
        // =======================================================
        [HttpGet]
        [Authorize] // Permite entrar a cualquiera que tenga token (la lógica interna ya filtra lo que pueden ver)
        public async Task<ActionResult<IEnumerable<DispositivoDto>>> GetDispositivos()
        {
            var query = _context.Dispositivos
                .Include(d => d.TipoDispositivo)
                .AsQueryable();

            // 2. Seguridad por Roles (Filtra las listas de las Empresas)
            if (User.IsInRole("Empresa"))
            {
                var empresaId = User.GetEmpresaId();
                if (empresaId != null)
                {
                    query = query.Where(d => d.EmpresaId == empresaId.Value);
                }
            }

            // 3. MAPEO COMPLETO HACIA EL DTO
            var equipos = await query
                .Select(d => new DispositivoDto
                {
                    Id = d.Id,
                    Marca = d.Marca,
                    CodigoActivo = d.CodigoActivo,
                    Modelo = d.Modelo,
                    Ubicacion = d.Ubicacion,
                    Estado = d.Estado,
                    Nombre = d.Marca + " " + d.Modelo,
                    NumeroSerie = d.NumeroSerie,
                    EmpresaId = d.EmpresaId,
                    TipoDispositivoId = d.TipoDispositivoId,
                    DetallesTecnicos = d.DetallesTecnicos,
                    // NUEVO: Agregamos el nombre del tipo para que la tabla en el frontend pueda mostrarlo
                    Tipo = d.TipoDispositivo != null ? d.TipoDispositivo.Nombre : "Sin clasificar"
                })
                .ToListAsync();

            return Ok(equipos);
        }


        [HttpGet("PorEmpresa/{empresaId}")]
        public async Task<ActionResult<IEnumerable<DispositivoDto>>> GetDispositivosPorEmpresa(int empresaId)
        {
            var equipos = await _context.Dispositivos
                .Where(d => d.EmpresaId == empresaId)
                .Select(d => new DispositivoDto
                {
                    Id = d.Id,
                    // Si Marca o Modelo son nulos, mostramos un texto por defecto para evitar el guion vacío
                    Nombre = (!string.IsNullOrEmpty(d.Marca) || !string.IsNullOrEmpty(d.Modelo))
                             ? (d.Marca + " " + d.Modelo).Trim()
                             : "Equipo sin nombre (" + d.Id + ")"
                })
                .ToListAsync();

            return Ok(equipos);
        }

        // GET: api/Dispositivos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Dispositivo>> GetDispositivo(int id)
        {
            var dispositivo = await _context.Dispositivos.FindAsync(id);
            if (dispositivo == null) return NotFound();

            if (User.IsInRole("Empresa") && dispositivo.EmpresaId != User.GetEmpresaId()) return Forbid();

            return dispositivo;
        }

        // POST: api/Dispositivos
        [HttpPost]
        [Authorize(Roles = "Admin, Trabajador")]
        public async Task<ActionResult<Dispositivo>> PostDispositivo(Dispositivo dispositivo)
        {
            if (User.IsInRole("Empresa"))
            {
                var empresaId = User.GetEmpresaId();
                if (empresaId == null) return Forbid();
                dispositivo.EmpresaId = empresaId.Value;
            }

            // --- NUEVO: Motor de Validación Dinámica ---
            var (esValido, mensajeError) = await ValidarDetallesContraEsquema(dispositivo.TipoDispositivoId, dispositivo.DetallesTecnicos);
            if (!esValido)
            {
                return BadRequest(new { mensaje = mensajeError });
            }

            _context.Dispositivos.Add(dispositivo);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetDispositivo", new { id = dispositivo.Id }, dispositivo);
        }

        // PUT: api/Dispositivos/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin, Trabajador")]
        public async Task<IActionResult> PutDispositivo(int id, Dispositivo dispositivo)
        {
            if (id != dispositivo.Id) return BadRequest();

            if (User.IsInRole("Empresa") && dispositivo.EmpresaId != User.GetEmpresaId()) return Forbid();

            // --- NUEVO: Motor de Validación Dinámica ---
            var (esValido, mensajeError) = await ValidarDetallesContraEsquema(dispositivo.TipoDispositivoId, dispositivo.DetallesTecnicos);
            if (!esValido)
            {
                return BadRequest(new { mensaje = mensajeError });
            }

            _context.Entry(dispositivo).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DispositivoExists(id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        // DELETE: api/Dispositivos/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin, Trabajador")]
        public async Task<IActionResult> DeleteDispositivo(int id)
        {
            var dispositivo = await _context.Dispositivos.FindAsync(id);
            if (dispositivo == null) return NotFound();

            if (User.IsInRole("Empresa") && dispositivo.EmpresaId != User.GetEmpresaId()) return Forbid();

            _context.Dispositivos.Remove(dispositivo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DispositivoExists(int id)
        {
            return _context.Dispositivos.Any(e => e.Id == id);
        }

        // ========================================================================
        // MÉTODO CENTRAL DE VALIDACIÓN DINÁMICA
        // ========================================================================
        private async Task<(bool EsValido, string MensajeError)> ValidarDetallesContraEsquema(int tipoDispositivoId, string detallesTecnicosJson)
        {
            if (string.IsNullOrWhiteSpace(detallesTecnicosJson))
                return (false, "Los detalles técnicos no pueden estar vacíos.");

            var tipo = await _context.TiposDispositivos.FindAsync(tipoDispositivoId);
            if (tipo == null)
                return (false, "El Tipo de Dispositivo asignado no existe.");

            try
            {
                var esquema = JsonDocument.Parse(tipo.EsquemaMolde).RootElement;
                var detalles = JsonDocument.Parse(detallesTecnicosJson).RootElement;

                // Recorremos las reglas del molde (ej: "Procesador", "RAM")
                foreach (var campo in esquema.EnumerateArray())
                {
                    var nombreCampo = campo.GetProperty("nombreCampo").GetString();
                    var requerido = campo.GetProperty("requerido").GetBoolean();

                    // Si el molde dice que es requerido, verificamos que venga en el JSON del cliente
                    if (requerido && !detalles.TryGetProperty(nombreCampo!, out _))
                    {
                        return (false, $"Validación fallida: Falta el campo obligatorio '{nombreCampo}'.");
                    }
                }
                return (true, string.Empty); // Todo perfecto
            }
            catch (JsonException)
            {
                return (false, "El formato JSON de los detalles técnicos o del esquema es inválido.");
            }
        }
    }
}