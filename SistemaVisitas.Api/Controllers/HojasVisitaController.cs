using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVisitas.Api.Models; 
using SistemaVisitas.Api.Services;
using SistemaVisitas.Shared.DTOs;
using System.Security.Claims;

namespace SistemaVisitas.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Permite el acceso a los técnicos (Trabajadores)
    public class HojasVisitaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly GeneradorPdfService _generadorPdfService;
        private readonly IEmailService _emailService;
        // Inyectamos los servicios en el constructor
        public HojasVisitaController(
            ApplicationDbContext context,
            GeneradorPdfService generadorPdfService,
            IEmailService emailService)
        {
            _context = context;
            _generadorPdfService = generadorPdfService;
            _emailService = emailService;
        }

        [HttpPost("Ejecutar")]
        public async Task<ActionResult> EjecutarVisita(EjecucionVisitaDto dto)
        {
            // ====================================================================
            // 0. FORZAR ID DEL USUARIO DESDE EL TOKEN (Blindaje de Seguridad)
            // ====================================================================
            var claimId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                          ?? User.FindFirst("id")?.Value
                          ?? User.FindFirst("Id")?.Value;

            if (int.TryParse(claimId, out int idRealToken))
            {
                // Sin importar qué ID envió el navegador, le asignamos obligatoriamente 
                // el ID real de la persona que firmó el JWT.
                dto.TrabajadorId = idRealToken;
            }
            else
            {
                return Unauthorized("Token inválido: No se pudo identificar al trabajador.");
            }
            // ====================================================================
            // 1. VALIDACIONES DE SEGURIDAD (Aislamiento por Empresa / Tenant)
            // ====================================================================

            // Verificar que la empresa exista
            var empresaExiste = await _context.Empresas.AnyAsync(e => e.Id == dto.EmpresaId);
            if (!empresaExiste) return BadRequest("La empresa cliente no existe.");

            // HITO 4: Validar que los dispositivos realmente le pertenezcan a esa Empresa
            if (dto.EquiposRevisados.Any())
            {
                var idsDispositivos = dto.EquiposRevisados.Select(d => d.DispositivoId).ToList();
                var equiposValidos = await _context.Dispositivos
                    .Where(d => idsDispositivos.Contains(d.Id) && d.EmpresaId == dto.EmpresaId)
                    .CountAsync();

                if (equiposValidos != idsDispositivos.Count)
                {
                    return BadRequest("Alerta de Seguridad: Estás intentando reportar equipos que no pertenecen a este cliente.");
                }
            }

            // Validar que mandó la firma
            if (string.IsNullOrWhiteSpace(dto.FirmaClienteBase64))
            {
                return BadRequest("La firma del cliente es obligatoria para cerrar la visita.");
            }

            // ====================================================================
            // 2. TRANSACCIÓN DE BASE DE DATOS (Guardado Consistente Todo o Nada)
            // ====================================================================
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // A. Auto-generar un consecutivo elegante (Ej: VIS-20261105-001)
                var fecha = DateTime.Now;
                var totalVisitas = await _context.HojasVisita.CountAsync();
                var consecutivo = $"VIS-{fecha:yyyyMMdd}-{(totalVisitas + 1):D3}";

                // B. Crear la Cabecera (HojasVisitum)
                var nuevaVisita = new HojasVisitum
                {
                    Consecutivo = consecutivo,
                    EmpresaId = dto.EmpresaId,
                    TrabajadorId = dto.TrabajadorId, // El ID del técnico logueado
                    PlantillaVisitaId = dto.PlantillaVisitaId,
                    FechaInicio = dto.FechaInicio,
                    FechaFin = dto.FechaFin ?? DateTime.Now,
                    ObservacionesGenerales = dto.ObservacionesGenerales,
                    NombreQuienRecibe = dto.NombreQuienRecibe,
                    CargoQuienRecibe = dto.CargoQuienRecibe,
                    DocumentoQuienRecibe = dto.DocumentoQuienRecibe,
                    FirmaClienteBase64 = dto.FirmaClienteBase64, // <-- La firma viaja como texto largo
                    Estado = dto.Estado,
                    CorreoEnviado = false
                };

                _context.HojasVisita.Add(nuevaVisita);
                await _context.SaveChangesAsync(); // Se guarda para que la BD nos devuelva el ID de la visita

                // C. Guardar los Equipos Revisados (VisitaDispositivo)
                if (dto.EquiposRevisados.Any())
                {
                    var relacionEquipos = dto.EquiposRevisados.Select(eq => new VisitaDispositivo
                    {
                        HojaVisitaId = nuevaVisita.Id,
                        DispositivoId = eq.DispositivoId,
                        ObservacionEquipo = eq.ObservacionEquipo
                    }).ToList();

                    _context.VisitaDispositivos.AddRange(relacionEquipos);
                }

                // D. Guardar las Respuestas de la encuesta (HojaVisitaRespuesta)
                if (dto.Respuestas.Any())
                {
                    var respuestasEntity = dto.Respuestas.Select(r => new HojaVisitaRespuesta
                    {
                        HojaVisitaId = nuevaVisita.Id,
                        PlantillaDetalleItemId = r.PlantillaDetalleItemId,

                        // Mapeo directo a las columnas de tu base de datos
                        ValorRespuesta = r.ValorRespuesta,
                        ObservacionItem = r.ObservacionItem
                    }).ToList();

                    // Asegúrate de usar el nombre plural o singular según cómo lo autocompletó tu ApplicationDbContext
                    _context.HojaVisitaRespuestas.AddRange(respuestasEntity);
                }

                // E. Guardado Final de los hijos
                await _context.SaveChangesAsync();

                // F. Confirmar la transacción (LA VISITA SE GUARDA AQUÍ)
                await transaction.CommitAsync();

                // === NUEVO: AISLAMOS EL PDF Y EL CORREO ===
                try
                {
                    var actaDtoParaPdf = await ObtenerDetalle(nuevaVisita.Id);
                    ActaVisitaDto acta = actaDtoParaPdf.Value;

                    if (acta == null && actaDtoParaPdf.Result is OkObjectResult okResult)
                    {
                        acta = okResult.Value as ActaVisitaDto;
                    }

                    if (acta != null)
                    {
                        var pdfBytes = _generadorPdfService.GenerarActaPdf(acta);

                        string correoDestino = dto.CorreoEnviado;
                        string asunto = $"Acta de Visita Técnica {acta.Consecutivo} - Tecnoware Solutions";
                        string cuerpo = $@"
                        <h3>Hola, {acta.NombreQuienRecibe}</h3>
                        <p>Adjuntamos el acta de servicio correspondiente a nuestra visita técnica en sus instalaciones.</p>
                        <p>Gracias por confiar en <strong>Tecnoware Solutions</strong>.</p>";

                        await _emailService.EnviarCorreoAsync(correoDestino, asunto, cuerpo, pdfBytes, $"Acta_{acta.Consecutivo}.pdf");

                        // Si el envío fue exitoso, actualizas el estado en la base de datos
                        nuevaVisita.CorreoEnviado = true;
                        await _context.SaveChangesAsync();
                    }

                    return Ok(new { consecutivo = nuevaVisita.Consecutivo, mensaje = "Visita finalizada y acta enviada por correo exitosamente." });
                }
                catch (Exception exEmail)
                {
                    // Si el correo falla (ej. credenciales inválidas), la visita ya está a salvo en la BD.
                    // Devolvemos Ok pero informamos que el correo no salió.
                    return Ok(new { consecutivo = nuevaVisita.Consecutivo, mensaje = $"Visita guardada correctamente, pero el correo falló: {exEmail.Message}" });
                }
            }
            catch (Exception ex)
            {
                // Este catch ahora solo atrapará errores ANTES del CommitAsync (ej. errores de SQL)
                // Validamos si la transacción sigue viva antes de hacer rollback
                try
                {
                    await transaction.RollbackAsync();
                }
                catch
                {
                    // Si falla (porque ya se había cerrado o se cayó la red), simplemente lo ignoramos
                }

                return StatusCode(500, $"Error crítico. No se guardó ningún dato: {ex.Message}");
            }
        }
        [HttpGet]
        [Authorize] // Bloquea el acceso a usuarios sin token
        public async Task<ActionResult<IEnumerable<VisitaResumenDto>>> ObtenerHistorial()
        {
            var query = _context.HojasVisita
                .Include(h => h.Empresa)
                .Include(h => h.PlantillaVisita)
                .Include(h => h.Trabajador)
                .AsQueryable();

            // 1. Filtro para "Cliente": Solo ve las actas de su empresa
            if (User.IsInRole("Empresa"))
            {
                var empresaIdClaim = User.FindFirst("EmpresaId")?.Value;

                if (int.TryParse(empresaIdClaim, out int empresaId))
                {
                    query = query.Where(h => h.EmpresaId == empresaId);
                }
                else
                {
                    return Forbid("El token no contiene una empresa válida.");
                }
            }
            // 2. Filtro para "Trabajador": Solo ve las actas donde él fue el técnico
            else if (User.IsInRole("Trabajador"))
            {
                // Se asume que guardas el Id del usuario en el Claim NameIdentifier al hacer login
                var trabajadorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (int.TryParse(trabajadorIdClaim, out int trabajadorId))
                {
                    query = query.Where(h => h.TrabajadorId == trabajadorId);
                }
            }
            // Si el rol es "Admin", no entra a ningún 'if' y la consulta se va limpia (trae todas)

            var historial = await query
                .OrderByDescending(h => h.FechaInicio)
                .Select(h => new VisitaResumenDto
                {
                    Id = h.Id,
                    Consecutivo = h.Consecutivo,
                    EmpresaNombre = h.Empresa.Nombre,
                    PlantillaNombre = h.PlantillaVisita.Nombre,
                    TecnicoNombre = h.Trabajador.NombreCompleto,
                    FechaInicio = h.FechaInicio,
                    Estado = h.Estado ?? "Finalizada"
                })
                .ToListAsync();

            return Ok(historial);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<ActaVisitaDto>> ObtenerDetalle(int id)
        {
            // 1. Buscamos la visita con todas sus relaciones ("Hijos" y "Nietos")
            var visita = await _context.HojasVisita
                .Include(h => h.Empresa)
                .Include(h => h.Trabajador)
                .Include(h => h.PlantillaVisita)
                .Include(h => h.VisitaDispositivos)
                    .ThenInclude(vd => vd.Dispositivo)
                // Asegúrate que tu modelo HojasVisita tenga una propiedad llamada HojaVisitaRespuestas
                .Include(h => h.HojaVisitaRespuesta)
                    .ThenInclude(r => r.PlantillaDetalleItem)
                        .ThenInclude(i => i.PlantillaDetalle)
                .FirstOrDefaultAsync(h => h.Id == id);

            if (visita == null) return NotFound("Visita no encontrada.");

            // 2. Seguridad: Si es cliente, verificamos que esta visita sea de su empresa
            if (User.IsInRole("Cliente"))
            {
                var empresaIdClaim = User.FindFirst("EmpresaId")?.Value;
                if (empresaIdClaim == null || visita.EmpresaId.ToString() != empresaIdClaim)
                {
                    return Forbid();
                }
            }

            // 3. Mapeamos al DTO plano para el frontend
            var acta = new ActaVisitaDto
            {
                Id = visita.Id,
                Consecutivo = visita.Consecutivo,
                EmpresaNombre = visita.Empresa?.Nombre ?? "Desconocida",
                TecnicoNombre = visita.Trabajador?.NombreCompleto ?? "Desconocido",
                PlantillaNombre = visita.PlantillaVisita?.Nombre ?? "N/A",
                FechaInicio = visita.FechaInicio,
                FechaFin = visita.FechaFin ?? visita.FechaInicio,
                ObservacionesGenerales = visita.ObservacionesGenerales,
                NombreQuienRecibe = visita.NombreQuienRecibe,
                CargoQuienRecibe = visita.CargoQuienRecibe,
                DocumentoQuienRecibe = visita.DocumentoQuienRecibe,
                FirmaClienteBase64 = visita.FirmaClienteBase64,
                Estado = visita.Estado ?? "Finalizada",

                // Extraemos solo los nombres de los equipos
                Equipos = visita.VisitaDispositivos
                    .Select(vd => vd.Dispositivo?.Marca+" "+vd.Dispositivo?.Modelo ?? "Equipo sin nombre")
                    .ToList(),

                // Armamos las respuestas ordenadas
                Respuestas = visita.HojaVisitaRespuesta
                    .OrderBy(r => r.PlantillaDetalleItem?.PlantillaDetalle?.Orden)
                    .ThenBy(r => r.PlantillaDetalleItem?.Orden)
                    .Select(r => new ActaRespuestaDto
                    {
                        Seccion = r.PlantillaDetalleItem?.PlantillaDetalle?.TituloSeccion ?? "General",
                        Pregunta = r.PlantillaDetalleItem?.NombreItem ?? "Pregunta",
                        TipoCampo = r.PlantillaDetalleItem?.TipoCampo ?? "Texto",
                        Respuesta = r.ValorRespuesta == "True" ? "Sí / Cumple" :
                                    r.ValorRespuesta == "False" ? "No / No Cumple" :
                                    (r.ValorRespuesta ?? "Sin respuesta"),
                        Observacion = r.ObservacionItem
                    }).ToList()
            };

            return Ok(acta);
        }

        [HttpPost("Asignar")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> AsignarVisita([FromBody] CrearAsignacionDto dto)
        {
            // ====================================================================
            // 0. FORZAR ID DEL USUARIO DESDE EL TOKEN (Auditoría del Admin)
            // ====================================================================
            var claimId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                          ?? User.FindFirst("id")?.Value
                          ?? User.FindFirst("Id")?.Value;

            if (!int.TryParse(claimId, out int idAdminLogueado))
            {
                return Unauthorized("Token inválido: No se pudo identificar al administrador que intenta agendar.");
            }

            // ====================================================================
            // 1. VALIDACIONES DE SEGURIDAD (Aislamiento y Coherencia)
            // ====================================================================
            // Verificar que la empresa exista
            var empresaExiste = await _context.Empresas.AnyAsync(e => e.Id == dto.EmpresaId);
            if (!empresaExiste) return BadRequest("La empresa cliente seleccionada no existe en el sistema.");

            // Verificar que el trabajador exista, esté activo y sea un técnico (Trabajador)
            var tecnicoValido = await _context.Usuarios
                .AnyAsync(u => u.Id == dto.TrabajadorId && u.Rol.Nombre == "Trabajador" && u.Activo == true);
            if (!tecnicoValido) return BadRequest("El usuario seleccionado no existe, no está activo o no tiene rol de Técnico.");

            // Validar que la fecha programada no sea en el pasado (con 5 minutos de gracia por latencia)
            if (dto.FechaProgramada < DateTime.Now.AddMinutes(-5))
            {
                return BadRequest("La fecha programada no puede estar en el pasado.");
            }

            // ====================================================================
            // 2. TRANSACCIÓN DE BASE DE DATOS (Guardado Consistente Todo o Nada)
            // ====================================================================
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // A. Auto-generar un consecutivo elegante de programación (Ej: PRG-20261111-001)
                var fecha = DateTime.Now;
                var totalVisitas = await _context.HojasVisita.CountAsync();
                var consecutivo = $"PRG-{fecha:yyyyMMdd}-{(totalVisitas + 1):D3}";

                // B. Crear la Cabecera (HojasVisitum) en estado "Programada"
                var nuevaAsignacion = new HojasVisitum
                {
                    Consecutivo = consecutivo,
                    EmpresaId = dto.EmpresaId,
                    TrabajadorId = dto.TrabajadorId, // El ID del técnico asignado (viene del DTO)

                    // Requerido por tu BD (No NULL). Usamos 1 por defecto; el técnico lo cambiará al ejecutar
                    PlantillaVisitaId = dto.PlantillaVisitaId > 0 ? dto.PlantillaVisitaId : 1,

                    // Tu BD exige FechaInicio (No NULL). Se pre-llena con la programada temporalmente
                    FechaInicio = dto.FechaProgramada,
                    FechaProgramada = dto.FechaProgramada,

                    ObservacionesGenerales = dto.NotasAdmin,
                    Estado = "Programada",
                    CorreoEnviado = false
                };

                _context.HojasVisita.Add(nuevaAsignacion);

                // C. Guardado Final en la base de datos
                await _context.SaveChangesAsync();

                // D. Confirmar la transacción (LA ASIGNACIÓN SE GUARDA AQUÍ)
                await transaction.CommitAsync();

                return Ok(new { consecutivo = nuevaAsignacion.Consecutivo, mensaje = "Visita agendada y asignada al técnico exitosamente." });
            }
            catch (Exception ex)
            {
                // Atrapa errores antes del CommitAsync (ej. errores de SQL o constraint)
                // Validamos si la transacción sigue viva antes de hacer rollback
                try
                {
                    await transaction.RollbackAsync();
                }
                catch
                {
                    // Si falla (porque ya se había cerrado o se cayó la red), simplemente lo ignoramos
                }

                return StatusCode(500, $"Error crítico. No se agendó la visita: {ex.Message}");
            }
        }
    }
}