namespace SistemaVisitas.Shared.DTOs
{
    public class VisitaResumenDto
    {
        public int Id { get; set; }
        public string? Consecutivo { get; set; }
        public string EmpresaNombre { get; set; } = string.Empty;
        public string PlantillaNombre { get; set; } = string.Empty;
        public string TecnicoNombre { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public string Estado { get; set; } = string.Empty;
    }
}