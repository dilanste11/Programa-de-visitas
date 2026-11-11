using System;
using System.Collections.Generic;

namespace SistemaVisitas.Api.Models;

public partial class Empresa
{
    public int Id { get; set; }

    public string Nit { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string? Direccion { get; set; }

    public string? Telefono { get; set; }

    public string CorreoNotificaciones { get; set; } = null!;

    public bool? Activa { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public virtual ICollection<Dispositivo> Dispositivos { get; set; } = new List<Dispositivo>();

    public virtual ICollection<HojasVisitum> HojasVisita { get; set; } = new List<HojasVisitum>();

    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
