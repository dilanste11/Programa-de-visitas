using System;
using System.Collections.Generic;

namespace SistemaVisitas.Api.Models;

public partial class PlantillaDetalle
{
    public int Id { get; set; }

    public int PlantillaVisitaId { get; set; }

    public string TituloSeccion { get; set; } = null!;

    public int Orden { get; set; }

    public bool? Activo { get; set; }

    public virtual ICollection<PlantillaDetalleItem> PlantillaDetalleItems { get; set; } = new List<PlantillaDetalleItem>();

    public virtual PlantillasVisitum? PlantillaVisita { get; set; }
}
