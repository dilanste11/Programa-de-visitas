using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SistemaVisitas.Api.Models;
using SistemaVisitas.Shared.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SistemaVisitas.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            // 1. Buscar el usuario por correo incluyendo la información de su Rol
            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Correo == request.Correo && u.Activo == true);

            // 2. Validar si existe y si la contraseña coincide con el Hash
            if (usuario == null || !BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash))
            {
                return Unauthorized("Credenciales incorrectas.");
            }

            // 3. Generar el JWT
            var token = GenerarJwt(usuario);

            // 4. Devolver los datos al Frontend
            return Ok(new LoginResponseDto
            {
                Token = token,
                NombreCompleto = usuario.NombreCompleto,
                Correo = usuario.Correo,
                Rol = usuario.Rol?.Nombre ?? "",
                EmpresaId = usuario.EmpresaId
            });
        }

        // Endpoint auxiliar para crear tu primer usuario Administrador desde Swagger
        [HttpPost("registrar-admin")]
        public async Task<IActionResult> RegistrarAdmin(string nombre, string correo, string password)
        {
            if (await _context.Usuarios.AnyAsync(u => u.Correo == correo))
                return BadRequest("El correo ya existe.");

            var nuevoUsuario = new Usuario
            {
                NombreCompleto = nombre,
                Correo = correo,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                RolId = 1, // 1 es Admin según nuestra semilla de datos
                Activo = true
            };

            _context.Usuarios.Add(nuevoUsuario);
            await _context.SaveChangesAsync();

            return Ok("Administrador creado exitosamente. Ya puedes iniciar sesión.");
        }

        private string GenerarJwt(Usuario usuario)
        {
            var jwtSettings = _config.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Los "Claims" son los datos que viajarán encriptados dentro del token
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, usuario.Correo),
                new Claim(ClaimTypes.Role, usuario.Rol!.Nombre),
                new Claim("EmpresaId", usuario.EmpresaId?.ToString() ?? "")
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8), // El token durará 8 horas
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}