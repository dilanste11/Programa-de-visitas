using System;
using System.Collections.Generic;

namespace SistemaVisitas.Api.Models;

public partial class Usuario
{
    public int Id { get; set; }

    public int RolId { get; set; }

    public int? EmpresaId { get; set; }

    public string NombreCompleto { get; set; } = null!;

    public string Correo { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public bool? Activo { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public virtual Empresa? Empresa { get; set; }

    public virtual ICollection<HojasVisitum> HojasVisita { get; set; } = new List<HojasVisitum>();

    public virtual Role Rol { get; set; } = null!;
}
