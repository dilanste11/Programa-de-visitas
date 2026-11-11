using System;
using System.Collections.Generic;

namespace SistemaVisitas.Api.Models;

public partial class PlantillasVisitum
{
    public int Id { get; set; }

    public string? Codigo { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool? Activa { get; set; }

    public virtual ICollection<HojasVisitum> HojasVisita { get; set; } = new List<HojasVisitum>();

    public virtual ICollection<PlantillaDetalle> PlantillaDetalles { get; set; } = new List<PlantillaDetalle>();
}
