using System.ComponentModel.DataAnnotations;

namespace SistemaVisitas.Shared.DTOs
{
    public class DispositivoDto
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty; 

        public string? CodigoActivo { get; set; }
        public string? NumeroSerie { get; set; }

        [Required(ErrorMessage = "La marca es obligatoria")]
        public string Marca { get; set; } = string.Empty;

        [Required(ErrorMessage = "El modelo es obligatorio")]
        public string Modelo { get; set; } = string.Empty;

        public string? Ubicacion { get; set; }
        public string? Estado { get; set; } = "Activo"; // Valor por defecto

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un tipo de equipo")]
        public int TipoDispositivoId { get; set; }
        public string? Tipo { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Debe asignar el equipo a una empresa")]
        public int EmpresaId { get; set; }

        public string DetallesTecnicos { get; set; } = "{}";
    }
}