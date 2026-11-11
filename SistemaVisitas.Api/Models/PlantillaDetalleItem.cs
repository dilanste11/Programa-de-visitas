using System;
using System.Collections.Generic;

namespace SistemaVisitas.Api.Models;

public partial class PlantillaDetalleItem
{
    public int Id { get; set; }

    public int PlantillaDetalleId { get; set; }

    public string NombreItem { get; set; } = null!;

    public string TipoCampo { get; set; } = null!;

    public bool? EsObligatorio { get; set; }

    public int Orden { get; set; }

    public bool? Activo { get; set; }

    public virtual ICollection<HojaVisitaRespuesta> HojaVisitaRespuesta { get; set; } = new List<HojaVisitaRespuesta>();

    public virtual PlantillaDetalle? PlantillaDetalle { get; set; }
}
