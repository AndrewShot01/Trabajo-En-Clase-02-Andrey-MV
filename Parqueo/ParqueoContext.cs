using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Parqueo.Modelos;
namespace Parqueo;
// Modelo local equivalente al DDL; no se presenta como scaffold ejecutado.
public class ParqueoContext(DbContextOptions<ParqueoContext> options) : DbContext(options)
{
 public DbSet<Automovil> Automoviles => Set<Automovil>();
 public DbSet<Estacionamiento> Parqueos => Set<Estacionamiento>();
 public DbSet<Ingreso> Ingresos => Set<Ingreso>();
 protected override void OnModelCreating(ModelBuilder b)
 {
  b.Entity<Automovil>().ToTable("PRQAutomoviles", "dbo").HasKey(x => x.Id);
  b.Entity<Automovil>().Property(x => x.Id).ValueGeneratedNever();
  b.Entity<Automovil>().Property(x => x.Color).HasMaxLength(50).IsRequired();
  b.Entity<Automovil>().Property(x => x.Fabricante).HasMaxLength(80).IsRequired();
  b.Entity<Automovil>().Property(x => x.Tipo).HasMaxLength(10).IsRequired();
  b.Entity<Estacionamiento>().ToTable("PRQParqueos", "dbo").HasKey(x => x.Id);
  b.Entity<Estacionamiento>().Property(x => x.Id).ValueGeneratedNever();
  b.Entity<Estacionamiento>().Property(x => x.Provincia).HasMaxLength(50).IsRequired();
  b.Entity<Estacionamiento>().Property(x => x.Nombre).HasMaxLength(100).IsRequired();
  b.Entity<Estacionamiento>().Property(x => x.PrecioPorHora).HasPrecision(12,2);
  b.Entity<Ingreso>().ToTable("PRQIngresosAutomoviles", "dbo").HasKey(x => x.Consecutivo);
  b.Entity<Ingreso>().Property(x => x.Consecutivo).ValueGeneratedNever();
  b.Entity<Ingreso>().HasOne<Automovil>().WithMany().HasForeignKey(x => x.IdAutomovil).OnDelete(DeleteBehavior.Restrict);
  b.Entity<Ingreso>().HasOne<Estacionamiento>().WithMany().HasForeignKey(x => x.IdParqueo).OnDelete(DeleteBehavior.Restrict);
 }
}
public class FabricaContexto : IDesignTimeDbContextFactory<ParqueoContext>
{
 public ParqueoContext CreateDbContext(string[] args)
 {
  var config = new ConfigurationBuilder().AddUserSecrets<FabricaContexto>().Build();
  var cadena = config.GetConnectionString("Parqueo") ?? throw new InvalidOperationException("Configure el secreto ConnectionStrings:Parqueo.");
  return new(new DbContextOptionsBuilder<ParqueoContext>().UseSqlServer(cadena).Options);
 }
}
