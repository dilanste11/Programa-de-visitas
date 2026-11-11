namespace SistemaVisitas.Shared.DTOs
{
    public class DashboardDto
    {
        // Métricas de Admin
        public int TotalEmpresas { get; set; }
        public int VisitasMesAdmin { get; set; }
        public int EquiposEnReparacionAdmin { get; set; }
        public int TecnicosActivos { get; set; } // Opcional, si tienes tabla de usuarios

        // Métricas de Trabajador
        public int VisitasMesTrabajador { get; set; }
        public int TotalEquiposRevisados { get; set; }

        // Métricas de Empresa
        public int TotalMisEquipos { get; set; }
        public int MisEquiposEnReparacion { get; set; }
        public DateTime? FechaUltimaVisita { get; set; }

        public class VisitaProgramadaDto
        {
            public int Id { get; set; }
            public string EmpresaNombre { get; set; } = "";
            public DateTime FechaProgramada { get; set; }
            public string NotasAdmin { get; set; } = "";
        }

        // Dentro de tu class DashboardDto, agrega esta propiedad en la sección de Trabajador:
        public List<VisitaProgramadaDto> VisitasProgramadas { get; set; } = new();
    }
}