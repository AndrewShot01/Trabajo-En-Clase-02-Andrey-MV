using System.ComponentModel.DataAnnotations;
using Parqueo.Modelos;
namespace Parqueo.Api;

public sealed record AutomovilSolicitud(
 [Range(1, int.MaxValue)] int Id,
 [Required, StringLength(50)] string Color,
 [Range(1886, 2100)] int Anio,
 [Required, StringLength(80)] string Fabricante,
 [Required, RegularExpression("^(sedán|4x4|moto)$")] string Tipo)
{
    public Automovil Entidad() => new() { Id = Id, Color = Color, Anio = Anio, Fabricante = Fabricante, Tipo = Tipo };
}
public sealed record ParqueoSolicitud(
 [Range(1, int.MaxValue)] int Id,
 [Required, StringLength(50)] string Provincia,
 [Required, StringLength(100)] string Nombre,
 [Range(typeof(decimal), "0.01", "9999999999.99")] decimal PrecioPorHora)
{
    public Estacionamiento Entidad() => new() { Id = Id, Provincia = Provincia, Nombre = Nombre, PrecioPorHora = PrecioPorHora };
}
public sealed record IngresoSolicitud(
 [Range(1, int.MaxValue)] int Consecutivo,
 [Range(1, int.MaxValue)] int IdParqueo,
 [Range(1, int.MaxValue)] int IdAutomovil,
 DateTime Entrada, DateTime? Salida)
{
    public Ingreso Entidad() => new() { Consecutivo = Consecutivo, IdParqueo = IdParqueo, IdAutomovil = IdAutomovil, Entrada = Entrada, Salida = Salida };
}
public record ResumenIngreso(int Consecutivo, int IdParqueo, int IdAutomovil, DateTime Entrada, DateTime? Salida);
public record AutomovilRespuesta(int Id, string Color, int Anio, string Fabricante, string Tipo, IReadOnlyList<ResumenIngreso> Ingresos);
public record ParqueoRespuesta(int Id, string Provincia, string Nombre, decimal PrecioPorHora, IReadOnlyList<ResumenIngreso> Ingresos);
public record IngresoRespuesta(int Consecutivo, int IdParqueo, int IdAutomovil, DateTime Entrada, DateTime? Salida,
 decimal? DuracionMinutos, decimal? DuracionHoras, decimal? MontoTotal, AutomovilSolicitud Automovil, ParqueoSolicitud Parqueo);
