using System;
using System.Collections.Generic;

namespace SistemaVisitas.Api.Models;

public partial class HojasVisitum
{
    public int Id { get; set; }

    public string Consecutivo { get; set; } = null!;

    public int EmpresaId { get; set; }

    public int TrabajadorId { get; set; }

    public int PlantillaVisitaId { get; set; }

    public DateTime FechaInicio { get; set; }

    public DateTime? FechaFin { get; set; }

    public string? ObservacionesGenerales { get; set; }

    public string? NombreQuienRecibe { get; set; }

    public string? CargoQuienRecibe { get; set; }

    public string? DocumentoQuienRecibe { get; set; }

    public string? FirmaClienteBase64 { get; set; }

    public string? Estado { get; set; }

    public bool? CorreoEnviado { get; set; }
    public DateTime FechaProgramada { get; set; }

    public virtual Empresa Empresa { get; set; } = null!;

    public virtual ICollection<HojaVisitaRespuesta> HojaVisitaRespuesta { get; set; } = new List<HojaVisitaRespuesta>();

    public virtual PlantillasVisitum PlantillaVisita { get; set; } = null!;

    public virtual Usuario Trabajador { get; set; } = null!;

    public virtual ICollection<VisitaDispositivo> VisitaDispositivos { get; set; } = new List<VisitaDispositivo>();
}
