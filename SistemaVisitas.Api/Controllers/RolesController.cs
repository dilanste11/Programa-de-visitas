using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVisitas.Api.Models;

namespace SistemaVisitas.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RolesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            // Extrae solo el Id y el Nombre para llenar el select de Blazor
            var roles = await _context.Roles
                .Select(r => new { Id = r.Id, Nombre = r.Nombre })
                .ToListAsync();

            return Ok(roles);
        }
    }
}