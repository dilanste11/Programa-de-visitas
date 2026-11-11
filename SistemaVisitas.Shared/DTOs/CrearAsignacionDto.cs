public class CrearAsignacionDto
{
    public int EmpresaId { get; set; }
    public int TrabajadorId { get; set; }
    public int PlantillaVisitaId { get; set; } // Opcional en esta etapa
    public DateTime FechaProgramada { get; set; }
    public string NotasAdmin { get; set; } = string.Empty;
    public string Estado { get; set; } = "Programada"; // "Programada", "En Curso", "Finalizada"
}