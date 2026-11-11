using System.ComponentModel.DataAnnotations;

namespace SistemaVisitas.Shared.DTOs
{
    public class EmpresaDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de la empresa es obligatorio")]
        public string Nombre { get; set; } = string.Empty;

        public string Nit { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        public string CorreoNotificaciones { get; set; } = string.Empty;
    }
}