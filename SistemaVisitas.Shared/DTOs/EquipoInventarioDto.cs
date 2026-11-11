namespace SistemaVisitas.Shared.DTOs
{
    public class EquipoInventarioDto
    {
        public int Id { get; set; }
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string NumeroSerie { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string DetallesTecnicos { get; set; } = string.Empty; // Aquí viajará tu JSON dinámico
    }
}