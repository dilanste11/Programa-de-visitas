namespace SistemaVisitas.Shared.DTOs
{
    public class VisitaResponseDto
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
        public string NombreEmpresa { get; set; } = string.Empty;
        public DateTime FechaVisita { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string FirmaBase64 { get; set; } = string.Empty;
    }
}