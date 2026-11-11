using System.Security.Claims;

namespace SistemaVisitas.Api.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        // Este método extrae el ID de la empresa directamente del token JWT
        public static int? GetEmpresaId(this ClaimsPrincipal user)
        {
            var empresaIdClaim = user.FindFirst("EmpresaId")?.Value;

            if (!string.IsNullOrEmpty(empresaIdClaim) && int.TryParse(empresaIdClaim, out int empresaId))
            {
                return empresaId;
            }
            return null; // Retorna null si es un Admin o un Trabajador (que no pertenecen a una sola empresa)
        }

        // Método extra para obtener el ID del Usuario que hace la petición
        public static int GetUsuarioId(this ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out int userId) ? userId : 0;
        }
    }
}