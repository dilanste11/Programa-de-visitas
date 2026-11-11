using System.ComponentModel.DataAnnotations;

namespace SistemaVisitas.Shared.DTOs
{
    // 1. La cabecera de la visita (El paquete principal)
    public class EjecucionVisitaDto
    {
        public int Id { get; set; }

        public string? Consecutivo { get; set; }

        [Required(ErrorMessage = "La empresa es obligatoria")]
        public int EmpresaId { get; set; }

        [Required]
        public int TrabajadorId { get; set; } // El técnico que hace la visita

        [Required(ErrorMessage = "Debe seleccionar una plantilla")]
        public int PlantillaVisitaId { get; set; }

        public DateTime FechaInicio { get; set; } = DateTime.Now;
        public DateTime? FechaFin { get; set; }

        public string? ObservacionesGenerales { get; set; }

        [Required(ErrorMessage = "El nombre de quien recibe es obligatorio")]
        public string NombreQuienRecibe { get; set; } = string.Empty;

        public string? CargoQuienRecibe { get; set; }
        public string? DocumentoQuienRecibe { get; set; }

        // Aquí guardaremos el trazo del lienzo (Canvas) en texto plano (Base64)
        public string FirmaClienteBase64 { get; set; } = string.Empty;
        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        public string CorreoEnviado { get; set; } = string.Empty;
        public string Estado { get; set; } = "Finalizada"; // Programada, En Proceso, Finalizada

        // Las listas anidadas para guardar todo de un solo golpe
        public List<VisitaDispositivoDto> EquiposRevisados { get; set; } = new();
        public List<VisitaRespuestaDto> Respuestas { get; set; } = new();
    }

    // 2. Equipos que se incluyeron en esta visita
    public class VisitaDispositivoDto
    {
        public int DispositivoId { get; set; }
        public string? NombreDispositivo { get; set; } // Solo para mostrar en UI
        public string? ObservacionEquipo { get; set; }
    }

    // 3. Las respuestas a las preguntas de la plantilla
    public class VisitaRespuestaDto
    {
        public int PlantillaDetalleItemId { get; set; }

        public string? Pregunta { get; set; } // Solo para mostrar en UI
        public string? TipoCampo { get; set; } // Booleano, Texto, Numero
        public bool EsObligatorio { get; set; } // Para validación en el frontend
        public string? ValorRespuesta { get; set; }
        public string? ObservacionItem { get; set; }
    }
}