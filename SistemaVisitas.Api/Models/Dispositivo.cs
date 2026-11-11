using System;
using System.Collections.Generic;

namespace SistemaVisitas.Api.Models;

public partial class Dispositivo
{
    public int Id { get; set; }

    public int EmpresaId { get; set; }

    public int TipoDispositivoId { get; set; }

    public string? CodigoActivo { get; set; }

    public string? NumeroSerie { get; set; }

    public string? Marca { get; set; } = null!;

    public string? Modelo { get; set; } = null!;

    public string? Ubicacion { get; set; }

    public string? Estado { get; set; }

    public string DetallesTecnicos { get; set; } = null!;

    public DateTime? FechaRegistro { get; set; }

    public virtual Empresa? Empresa { get; set; } = null!;

    public virtual TiposDispositivo? TipoDispositivo { get; set; } = null!;

    public virtual ICollection<VisitaDispositivo> VisitaDispositivos { get; set; } = new List<VisitaDispositivo>();
}
