using System;
using System.Collections.Generic;

namespace SistemaVisitas.Api.Models;

public partial class TiposDispositivo
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string EsquemaMolde { get; set; } = null!;

    public bool? Activo { get; set; }

    public virtual ICollection<Dispositivo> Dispositivos { get; set; } = new List<Dispositivo>();
}
