using System.ComponentModel.DataAnnotations;

namespace SistemaVisitas.Shared.DTOs
{
    // 1. La Cabecera de la Plantilla
    public class PlantillaVisitaDto
    {
        public int Id { get; set; }

        public string? Codigo { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public bool Activa { get; set; } = true;

        // Lista anidada para enviar todo en un solo JSON
        public List<PlantillaDetalleDto> Detalles { get; set; } = new();
    }

    // 2. Las Secciones de la Plantilla
    public class PlantillaDetalleDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El título de la sección es obligatorio")]
        public string TituloSeccion { get; set; } = string.Empty;

        public int Orden { get; set; }

        public bool Activo { get; set; } = true;

        // Lista de preguntas de esta sección
        public List<PlantillaDetalleItemDto> Items { get; set; } = new();
    }

    // 3. Las Preguntas/Validaciones de la Sección
    public class PlantillaDetalleItemDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La pregunta es obligatoria")]
        public string NombreItem { get; set; } = string.Empty;

        [Required]
        public string TipoCampo { get; set; } = "Booleano"; // Ej: Texto, Numero, Booleano

        public bool EsObligatorio { get; set; } = true;

        public int Orden { get; set; }

        public bool Activo { get; set; } = true;
    }
}