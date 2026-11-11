using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SistemaVisitas.Api.Models;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Dispositivo> Dispositivos { get; set; }

    public virtual DbSet<Empresa> Empresas { get; set; }

    public virtual DbSet<HojaVisitaRespuesta> HojaVisitaRespuestas { get; set; }

    public virtual DbSet<HojasVisitum> HojasVisita { get; set; }

    public virtual DbSet<PlantillaDetalle> PlantillaDetalles { get; set; }

    public virtual DbSet<PlantillaDetalleItem> PlantillaDetalleItems { get; set; }

    public virtual DbSet<PlantillasVisitum> PlantillasVisita { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<TiposDispositivo> TiposDispositivos { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<VisitaDispositivo> VisitaDispositivos { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-KMODM9U\\SQLEXPRESS;Database=TecnowareVisitas;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Dispositivo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Disposit__3214EC0720BF7926");

            entity.Property(e => e.CodigoActivo).HasMaxLength(50);
            entity.Property(e => e.Estado)
                .HasMaxLength(50)
                .HasDefaultValue("Operativo");
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Marca).HasMaxLength(100);
            entity.Property(e => e.Modelo).HasMaxLength(100);
            entity.Property(e => e.NumeroSerie).HasMaxLength(100);
            entity.Property(e => e.Ubicacion).HasMaxLength(150);

            entity.HasOne(d => d.Empresa).WithMany(p => p.Dispositivos)
                .HasForeignKey(d => d.EmpresaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Dispositivos_Empresas");

            entity.HasOne(d => d.TipoDispositivo).WithMany(p => p.Dispositivos)
                .HasForeignKey(d => d.TipoDispositivoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Dispositivos_Tipos");
        });

        modelBuilder.Entity<Empresa>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Empresas__3214EC0745478A49");

            entity.HasIndex(e => e.Nit, "UQ__Empresas__C7D1D6DA41A6C127").IsUnique();

            entity.Property(e => e.Activa).HasDefaultValue(true);
            entity.Property(e => e.CorreoNotificaciones).HasMaxLength(150);
            entity.Property(e => e.Direccion).HasMaxLength(200);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Nit).HasMaxLength(30);
            entity.Property(e => e.Nombre).HasMaxLength(150);
            entity.Property(e => e.Telefono).HasMaxLength(50);
        });

        modelBuilder.Entity<HojaVisitaRespuesta>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__HojaVisi__3214EC0786FFE997");

            entity.Property(e => e.ObservacionItem).HasMaxLength(500);
            entity.Property(e => e.ValorRespuesta).HasMaxLength(500);

            entity.HasOne(d => d.HojaVisita).WithMany(p => p.HojaVisitaRespuesta)
                .HasForeignKey(d => d.HojaVisitaId)
                .HasConstraintName("FK_Respuestas_HojaVisita");

            entity.HasOne(d => d.PlantillaDetalleItem).WithMany(p => p.HojaVisitaRespuesta)
                .HasForeignKey(d => d.PlantillaDetalleItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Respuestas_ItemMolde");
        });

        modelBuilder.Entity<HojasVisitum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__HojasVis__3214EC072B84F7DF");

            entity.HasIndex(e => e.Consecutivo, "UQ__HojasVis__AB810F66BB5E67EC").IsUnique();

            entity.Property(e => e.CargoQuienRecibe).HasMaxLength(100);
            entity.Property(e => e.Consecutivo).HasMaxLength(30);
            entity.Property(e => e.CorreoEnviado).HasDefaultValue(false);
            entity.Property(e => e.DocumentoQuienRecibe).HasMaxLength(50);
            entity.Property(e => e.Estado)
                .HasMaxLength(30)
                .HasDefaultValue("En Proceso");
            entity.Property(e => e.NombreQuienRecibe).HasMaxLength(150);

            entity.HasOne(d => d.Empresa).WithMany(p => p.HojasVisita)
                .HasForeignKey(d => d.EmpresaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Visitas_Empresas");

            entity.HasOne(d => d.PlantillaVisita).WithMany(p => p.HojasVisita)
                .HasForeignKey(d => d.PlantillaVisitaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Visitas_Plantilla");

            entity.HasOne(d => d.Trabajador).WithMany(p => p.HojasVisita)
                .HasForeignKey(d => d.TrabajadorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Visitas_Trabajador");
        });

        modelBuilder.Entity<PlantillaDetalle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Plantill__3214EC07B731894B");

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Orden).HasDefaultValue(1);
            entity.Property(e => e.TituloSeccion).HasMaxLength(150);

            entity.HasOne(d => d.PlantillaVisita).WithMany(p => p.PlantillaDetalles)
                .HasForeignKey(d => d.PlantillaVisitaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PlantillaDetalles_Plantilla");
        });

        modelBuilder.Entity<PlantillaDetalleItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Plantill__3214EC07645A1E7E");

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.EsObligatorio).HasDefaultValue(true);
            entity.Property(e => e.NombreItem).HasMaxLength(200);
            entity.Property(e => e.Orden).HasDefaultValue(1);
            entity.Property(e => e.TipoCampo)
                .HasMaxLength(50)
                .HasDefaultValue("Texto");

            entity.HasOne(d => d.PlantillaDetalle).WithMany(p => p.PlantillaDetalleItems)
                .HasForeignKey(d => d.PlantillaDetalleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleItems_Detalle");
        });

        modelBuilder.Entity<PlantillasVisitum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Plantill__3214EC07117F7154");

            entity.HasIndex(e => e.Codigo, "UQ__Plantill__06370DAC43D2615A").IsUnique();

            entity.Property(e => e.Activa).HasDefaultValue(true);
            entity.Property(e => e.Codigo).HasMaxLength(30);
            entity.Property(e => e.Descripcion).HasMaxLength(300);
            entity.Property(e => e.Nombre).HasMaxLength(150);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Roles__3214EC071B9956DB");

            entity.HasIndex(e => e.Nombre, "UQ__Roles__75E3EFCF4CB4C36D").IsUnique();

            entity.Property(e => e.Nombre).HasMaxLength(50);
        });

        modelBuilder.Entity<TiposDispositivo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TiposDis__3214EC07C9F804B7");

            entity.ToTable("TiposDispositivo");

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Nombre).HasMaxLength(100);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Usuarios__3214EC07E7D812E4");

            entity.HasIndex(e => e.Correo, "UQ__Usuarios__60695A19A96B49A7").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Correo).HasMaxLength(150);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.NombreCompleto).HasMaxLength(150);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);

            entity.HasOne(d => d.Empresa).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.EmpresaId)
                .HasConstraintName("FK_Usuarios_Empresas");

            entity.HasOne(d => d.Rol).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.RolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Usuarios_Roles");
        });

        modelBuilder.Entity<VisitaDispositivo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__VisitaDi__3214EC07ABE81D58");

            entity.Property(e => e.ObservacionEquipo).HasMaxLength(500);

            entity.HasOne(d => d.Dispositivo).WithMany(p => p.VisitaDispositivos)
                .HasForeignKey(d => d.DispositivoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VisitaDisp_Dispositivo");

            entity.HasOne(d => d.HojaVisita).WithMany(p => p.VisitaDispositivos)
                .HasForeignKey(d => d.HojaVisitaId)
                .HasConstraintName("FK_VisitaDisp_Visita");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
