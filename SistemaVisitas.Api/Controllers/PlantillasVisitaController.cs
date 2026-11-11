using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVisitas.Api.Models; 
using SistemaVisitas.Shared.DTOs;

namespace SistemaVisitas.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PlantillasVisitaController : ControllerBase
    {
        private readonly ApplicationDbContext _context; 

        public PlantillasVisitaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/PlantillasVisita
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PlantillaVisitaDto>>> GetPlantillas()
        {
            var plantillas = await _context.PlantillasVisita
                .Select(p => new PlantillaVisitaDto
                {
                    Id = p.Id,
                    Codigo = p.Codigo,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    Activa = p.Activa ?? true
                })
                .ToListAsync();

            return Ok(plantillas);
        }

        // GET: api/PlantillasVisita/5 (Trae la plantilla con TODAS sus secciones e ítems ordenados)
        [HttpGet("{id}")]
        public async Task<ActionResult<PlantillaVisitaDto>> GetPlantilla(int id)
        {
            var plantilla = await _context.PlantillasVisita
                .Include(p => p.PlantillaDetalles)
                    .ThenInclude(d => d.PlantillaDetalleItems)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (plantilla == null) return NotFound();

            var dto = new PlantillaVisitaDto
            {
                Id = plantilla.Id,
                Codigo = plantilla.Codigo,
                Nombre = plantilla.Nombre,
                Descripcion = plantilla.Descripcion,
                Activa = plantilla.Activa ?? true,
                Detalles = plantilla.PlantillaDetalles
                    .OrderBy(d => d.Orden) // Respetamos el orden de las secciones
                    .Select(d => new PlantillaDetalleDto
                    {
                        Id = d.Id,
                        TituloSeccion = d.TituloSeccion,
                        Orden = d.Orden,
                        Activo = d.Activo ?? true,
                        Items = d.PlantillaDetalleItems
                            .OrderBy(i => i.Orden) // Respetamos el orden de las preguntas
                            .Select(i => new PlantillaDetalleItemDto
                            {
                                Id = i.Id,
                                NombreItem = i.NombreItem,
                                TipoCampo = i.TipoCampo,
                                EsObligatorio = i.EsObligatorio ?? true,
                                Orden = i.Orden,
                                Activo = i.Activo ?? true
                            }).ToList()
                    }).ToList()
            };

            return Ok(dto);
        }

        // POST: api/PlantillasVisita (Creación con Guardado Consistente)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> CrearPlantilla(PlantillaVisitaDto dto)
        {
            // Iniciamos la Transacción para garantizar que se guarde TODO o NADA
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var nuevaPlantilla = new PlantillasVisitum
                {
                    Codigo = dto.Codigo,
                    Nombre = dto.Nombre,
                    Descripcion = dto.Descripcion,
                    Activa = dto.Activa,
                    PlantillaDetalles = dto.Detalles.Select(d => new PlantillaDetalle
                    {
                        TituloSeccion = d.TituloSeccion,
                        Orden = d.Orden,
                        Activo = d.Activo,
                        PlantillaDetalleItems = d.Items.Select(i => new PlantillaDetalleItem
                        {
                            NombreItem = i.NombreItem,
                            TipoCampo = i.TipoCampo,
                            EsObligatorio = i.EsObligatorio,
                            Orden = i.Orden,
                            Activo = i.Activo
                        }).ToList()
                    }).ToList()
                };

                _context.PlantillasVisita.Add(nuevaPlantilla);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync(); // Confirmamos el guardado exitoso
                return Ok();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(); // Reversamos todo si hubo un fallo
                return BadRequest($"Error al guardar de forma consistente: {ex.Message}");
            }
        }

        // PUT: api/PlantillasVisita/5 (Actualización)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ActualizarPlantilla(int id, PlantillaVisitaDto dto)
        {
            if (id != dto.Id) return BadRequest("El ID no coincide.");

            // 🛡️ EL ESCUDO: Verificamos si la plantilla ya fue usada en alguna visita
            bool estaEnUso = await _context.HojasVisita.AnyAsync(h => h.PlantillaVisitaId == id);

            if (estaEnUso)
            {
                return BadRequest("No puedes editar esta plantilla porque ya ha sido utilizada en actas de visita. Para proteger el historial, por favor inactiva esta plantilla y crea una nueva.");
            }
            if (id != dto.Id) return BadRequest();

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var plantillaDb = await _context.PlantillasVisita
                    .Include(p => p.PlantillaDetalles)
                        .ThenInclude(d => d.PlantillaDetalleItems)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (plantillaDb == null) return NotFound();

                // 1. Actualizamos la cabecera
                plantillaDb.Codigo = dto.Codigo;
                plantillaDb.Nombre = dto.Nombre;
                plantillaDb.Descripcion = dto.Descripcion;
                plantillaDb.Activa = dto.Activa;

                // 2. La técnica más segura para constructores visuales: 
                // Eliminamos los detalles viejos e insertamos los nuevos que vienen armados del Frontend
                foreach (var detalle in plantillaDb.PlantillaDetalles)
                {
                    _context.RemoveRange(detalle.PlantillaDetalleItems);
                }

                // Luego borramos los "hijos" (las secciones)
                _context.RemoveRange(plantillaDb.PlantillaDetalles);

                var nuevosDetalles = dto.Detalles.Select(d => new PlantillaDetalle
                {
                    PlantillaVisitaId = id,
                    TituloSeccion = d.TituloSeccion,
                    Orden = d.Orden,
                    Activo = d.Activo,
                    PlantillaDetalleItems = d.Items.Select(i => new PlantillaDetalleItem
                    {
                        NombreItem = i.NombreItem,
                        TipoCampo = i.TipoCampo,
                        EsObligatorio = i.EsObligatorio,
                        Orden = i.Orden,
                        Activo = i.Activo
                    }).ToList()
                }).ToList();

                _context.PlantillaDetalles.AddRange(nuevosDetalles);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest($"Error en actualización consistente: {ex.Message}");
            }
        }
    }
}