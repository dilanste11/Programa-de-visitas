using System.ComponentModel.DataAnnotations;

namespace SistemaVisitas.Shared.DTOs
{
    public class TipoDispositivoDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El esquema es obligatorio")]
        public string EsquemaMolde { get; set; } = "[\n  {\n    \"nombreCampo\": \"Ejemplo\",\n    \"tipoDato\": \"texto\",\n    \"requerido\": true\n  }\n]";
    }
}