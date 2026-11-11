namespace SistemaVisitas.Shared.DTOs
{
    public class ActaVisitaDto
    {
        public int Id { get; set; }
        public string? Consecutivo { get; set; }
        public string EmpresaNombre { get; set; } = string.Empty;
        public string TecnicoNombre { get; set; } = string.Empty;
        public string PlantillaNombre { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string? ObservacionesGenerales { get; set; }
        public string NombreQuienRecibe { get; set; } = string.Empty;
        public string? CargoQuienRecibe { get; set; }
        public string? DocumentoQuienRecibe { get; set; }
        public string FirmaClienteBase64 { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string CorreoEnviado { get; set; } = string.Empty;

        public List<string> Equipos { get; set; } = new();
        public List<ActaRespuestaDto> Respuestas { get; set; } = new();
    }

    public class ActaRespuestaDto
    {
        public string Seccion { get; set; } = string.Empty;
        public string Pregunta { get; set; } = string.Empty;
        public string Respuesta { get; set; } = string.Empty;
        public string? Observacion { get; set; }
        public string TipoCampo { get; set; } = string.Empty;
    }
}