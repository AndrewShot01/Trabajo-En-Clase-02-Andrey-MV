using Parqueo.Modelos;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Parqueo;

public partial class ParqueoContext : DbContext
{
    public ParqueoContext(DbContextOptions<ParqueoContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Automovil> Automoviles { get; set; }

    public virtual DbSet<Ingreso> Ingresos { get; set; }

    public virtual DbSet<Estacionamiento> Parqueos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Automovil>(entity =>
                {
                    entity.HasKey(e => e.Id).HasName("PRQAutomoviles_pkey");

                    entity.ToTable("PRQAutomoviles", "user_07");

                    entity.Property(e => e.Id).ValueGeneratedNever();
                    entity.Property(e => e.Color).HasMaxLength(50);
                    entity.Property(e => e.Fabricante).HasMaxLength(80);
                    entity.Property(e => e.Tipo).HasMaxLength(10);
                });

        modelBuilder.Entity<Ingreso>(entity =>
        {
            entity.HasKey(e => e.Consecutivo).HasName("PRQIngresosAutomoviles_pkey");

            entity.ToTable("PRQIngresosAutomoviles", "user_07");

            entity.HasIndex(e => e.IdAutomovil, "ix_prqingresos_automovil");

            entity.HasIndex(e => e.Entrada, "ix_prqingresos_entrada");

            entity.HasIndex(e => e.IdParqueo, "ix_prqingresos_parqueo");

            entity.Property(e => e.Consecutivo).ValueGeneratedNever();
            entity.Property(e => e.Entrada).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Salida).HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.IdAutomovilNavigation).WithMany(p => p.Ingreso)
                .HasForeignKey(d => d.IdAutomovil)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("PRQIngresosAutomoviles_IdAutomovil_fkey");

            entity.HasOne(d => d.IdParqueoNavigation).WithMany(p => p.Ingreso)
                .HasForeignKey(d => d.IdParqueo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("PRQIngresosAutomoviles_IdParqueo_fkey");
        });

        modelBuilder.Entity<Estacionamiento>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRQParqueos_pkey");

            entity.ToTable("PRQParqueos", "user_07");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.PrecioPorHora).HasPrecision(12, 2);
            entity.Property(e => e.Provincia).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
