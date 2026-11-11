using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVisitas.Api.Models;
using SistemaVisitas.Shared.DTOs;
using System.Security.Claims;
using static SistemaVisitas.Shared.DTOs.DashboardDto;

namespace SistemaVisitas.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<DashboardDto>> GetEstadisticas()
        {
            var dto = new DashboardDto();
            var mesActual = DateTime.Now.Month;
            var anioActual = DateTime.Now.Year;

            // Extraemos el ID del usuario desde el Token
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? User.FindFirst("id")?.Value
                            ?? User.FindFirst("Id")?.Value;

            int.TryParse(userIdStr, out int userId);

            // ==========================================
            // 👑 ROL: ADMIN
            // ==========================================
            if (User.IsInRole("Admin"))
            {
                dto.TotalEmpresas = await _context.Empresas.CountAsync();

                dto.VisitasMesAdmin = await _context.HojasVisita
                    .Where(v => v.FechaInicio.Month == mesActual && v.FechaInicio.Year == anioActual)
                    .CountAsync();

                dto.EquiposEnReparacionAdmin = await _context.Dispositivos
                    .Where(d => d.Estado == "En Reparación")
                    .CountAsync();
            }
            // ==========================================
            // 🛠️ ROL: TRABAJADOR
            // ==========================================
            else if (User.IsInRole("Trabajador"))
            {
                dto.VisitasMesTrabajador = await _context.HojasVisita
                    .Where(v => v.TrabajadorId == userId && v.FechaInicio.Month == mesActual && v.FechaInicio.Year == anioActual)
                    .CountAsync();

                // Cuenta cuántos dispositivos únicos ha revisado en la historia
                dto.TotalEquiposRevisados = await _context.VisitaDispositivos
                    .Where(vd => vd.HojaVisita.TrabajadorId == userId)
                    .Select(vd => vd.DispositivoId)
                    .Distinct()
                    .CountAsync();

                dto.VisitasProgramadas = await _context.HojasVisita
                    .Include(v => v.Empresa) // Para obtener el nombre
                    .Where(v => v.TrabajadorId == userId && v.Estado == "Programada")
                    .OrderBy(v => v.FechaProgramada)
                    .Select(v => new VisitaProgramadaDto
                    {
                        Id = v.Id,
                        EmpresaNombre = v.Empresa.Nombre,
                        FechaProgramada = v.FechaProgramada,
                        NotasAdmin = v.ObservacionesGenerales
                    })
                    .ToListAsync();
            }
            // ==========================================
            // 🏢 ROL: EMPRESA
            // ==========================================
            else if (User.IsInRole("Empresa"))
            {
                // Asumimos que para la Empresa, su userId en el token corresponde a su EmpresaId
                // Si guardaste el EmpresaId en otro claim (ej. "EmpresaId"), cámbialo aquí:
                var empresaIdClaim = User.FindFirst("EmpresaId")?.Value ?? userId.ToString();
                int.TryParse(empresaIdClaim, out int empresaId);

                dto.TotalMisEquipos = await _context.Dispositivos
                    .Where(d => d.EmpresaId == empresaId)
                    .CountAsync();

                dto.MisEquiposEnReparacion = await _context.Dispositivos
                    .Where(d => d.EmpresaId == empresaId && d.Estado == "En Reparación")
                    .CountAsync();

                dto.FechaUltimaVisita = await _context.HojasVisita
                    .Where(v => v.EmpresaId == empresaId)
                    .OrderByDescending(v => v.FechaInicio)
                    .Select(v => (DateTime?)v.FechaInicio)
                    .FirstOrDefaultAsync();
            }

            return Ok(dto);
        }
    }
}