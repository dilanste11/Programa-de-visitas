using System;
using System.Collections.Generic;

namespace SistemaVisitas.Api.Models;

public partial class VisitaDispositivo
{
    public int Id { get; set; }

    public int HojaVisitaId { get; set; }

    public int DispositivoId { get; set; }

    public string? ObservacionEquipo { get; set; }

    public virtual Dispositivo Dispositivo { get; set; } = null!;

    public virtual HojasVisitum HojaVisita { get; set; } = null!;
}
