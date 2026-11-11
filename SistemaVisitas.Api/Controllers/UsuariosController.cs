using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVisitas.Api.Models; // Ajusta a tu namespace real
using SistemaVisitas.Shared.DTOs;
using BCrypt.Net;

namespace SistemaVisitas.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize(Roles = "Administrador")] // Descomenta esto si ya inyectas el Claim de Rol en tu JWT
    public class UsuariosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsuariosController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<UsuarioDto>>> GetUsuarios()
        {
            var usuarios = await _context.Usuarios
                .Include(u => u.Rol)
                .Include(u => u.Empresa)
                .Select(u => new UsuarioDto
                {
                    Id = u.Id,
                    NombreCompleto = u.NombreCompleto,
                    Correo = u.Correo,
                    RolId = u.RolId,
                    RolNombre = u.Rol.Nombre,
                    EmpresaId = u.EmpresaId,
                    EmpresaNombre = u.Empresa != null ? u.Empresa.Nombre : "Tecnoware Solutions",
                    Activo = u.Activo ?? false
                }).ToListAsync();

            return Ok(usuarios);
        }

        [HttpPost]
        public async Task<ActionResult> CrearUsuario(UsuarioCrearDto dto)
        {
            if (await _context.Usuarios.AnyAsync(u => u.Correo == dto.Correo))
                return BadRequest("El correo ya está registrado.");

            var nuevoUsuario = new Usuario // Ajusta al nombre de tu entidad real
            {
                NombreCompleto = dto.NombreCompleto,
                Correo = dto.Correo,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                RolId = dto.RolId,
                EmpresaId = dto.EmpresaId,
                Activo = true,
                FechaCreacion = DateTime.Now
            };

            _context.Usuarios.Add(nuevoUsuario);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Usuario creado exitosamente" });
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> EditarUsuario(int id, UsuarioEditarDto dto)
        {
            if (id != dto.Id) return BadRequest("ID no coincide");

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            usuario.NombreCompleto = dto.NombreCompleto;
            usuario.Correo = dto.Correo;
            usuario.RolId = dto.RolId;
            usuario.EmpresaId = dto.EmpresaId;
            usuario.Activo = dto.Activo;

            // Si el admin escribió algo en el campo de contraseña, se actualiza
            if (!string.IsNullOrEmpty(dto.Password))
            {
                usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }

            await _context.SaveChangesAsync();
            return Ok(new { mensaje = "Usuario actualizado exitosamente" });
        }

        [HttpGet("Tecnicos")]
        public async Task<ActionResult<List<UsuarioDto>>> GetTecnicos()
        {
            var tecnicos = await _context.Usuarios
                .Include(u => u.Rol)
                .Include(u => u.Empresa)
                // Filtramos solo a los que están activos y tienen el rol de técnico/trabajador
                .Where(u => u.Rol.Nombre == "Trabajador" && u.Activo == true)
                .Select(u => new UsuarioDto
                {
                    Id = u.Id,
                    NombreCompleto = u.NombreCompleto,
                    Correo = u.Correo,
                    RolId = u.RolId,
                    RolNombre = u.Rol.Nombre,
                    EmpresaId = u.EmpresaId,
                    EmpresaNombre = u.Empresa != null ? u.Empresa.Nombre : "Tecnoware Solutions",
                    Activo = u.Activo ?? false
                }).ToListAsync();

            return Ok(tecnicos);
        }
    }
}