using System.ComponentModel.DataAnnotations;

namespace SistemaVisitas.Shared.DTOs
{
    public class UsuarioDto
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public int RolId { get; set; }
        public string? RolNombre { get; set; }
        public int? EmpresaId { get; set; }
        public string? EmpresaNombre { get; set; }
        public bool Activo { get; set; }
    }

    public class UsuarioCrearDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato inválido")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe asignar un rol")]
        public int RolId { get; set; }

        public int? EmpresaId { get; set; } // Nulo para los administradores o técnicos internos
    }

    public class UsuarioEditarDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Correo { get; set; } = string.Empty;

        public string? Password { get; set; } // Opcional al editar

        [Required]
        public int RolId { get; set; }

        public int? EmpresaId { get; set; }
        public bool Activo { get; set; }
    }
}