using System;
using System.Collections.Generic;

namespace SistemaVisitas.Api.Models;

public partial class HojaVisitaRespuesta
{
    public int Id { get; set; }

    public int HojaVisitaId { get; set; }

    public int PlantillaDetalleItemId { get; set; }

    public string ValorRespuesta { get; set; } = null!;

    public string? ObservacionItem { get; set; }

    public virtual HojasVisitum HojaVisita { get; set; } = null!;

    public virtual PlantillaDetalleItem PlantillaDetalleItem { get; set; } = null!;
}
