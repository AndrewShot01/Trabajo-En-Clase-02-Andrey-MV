using System.ComponentModel.DataAnnotations;
namespace Parqueo.Mvc.Models;

public class AutomovilFormulario
{
    [Range(1, int.MaxValue, ErrorMessage = "El identificador debe ser positivo.")]
    public int Id { get; set; }
    [Required(ErrorMessage = "Ingrese el color."), StringLength(50), Display(Name = "Color")]
    public string Color { get; set; } = "";
    [Range(1886, 2100, ErrorMessage = "El año debe estar entre 1886 y 2100."), Display(Name = "Año")]
    public int Anio { get; set; }
    [Required(ErrorMessage = "Ingrese el fabricante."), StringLength(80), Display(Name = "Fabricante")]
    public string Fabricante { get; set; } = "";
    [Required, RegularExpression("^(sedán|4x4|moto)$", ErrorMessage = "Seleccione un tipo válido."), Display(Name = "Tipo")]
    public string Tipo { get; set; } = "sedán";
}
public class AutomovilVista : AutomovilFormulario
{
    public List<IngresoResumen> Ingresos { get; set; } = [];
}
public record IngresoResumen(int Consecutivo, int IdParqueo, int IdAutomovil, DateTime Entrada, DateTime? Salida);
public record ParqueoVista(int Id, string Provincia, string Nombre, decimal PrecioPorHora);
public record IngresoVista(int Consecutivo, int IdParqueo, int IdAutomovil, DateTime Entrada, DateTime? Salida,
 decimal? DuracionMinutos, decimal? DuracionHoras, decimal? MontoTotal, AutomovilFormulario Automovil, ParqueoVista Parqueo);
public class ListadoAutomoviles
{
    [Range(1886, 2100), Display(Name = "Desde el año")] public int? Desde { get; set; } = 1886;
    [Range(1886, 2100), Display(Name = "Hasta el año")] public int? Hasta { get; set; } = 2100;
    public IReadOnlyList<AutomovilVista> Automoviles { get; set; } = [];
    public string Fuente { get; set; } = "";
}
public class MaestroDetalle
{
    public string Tipo { get; set; } = "sedán";
    public string Fuente { get; set; } = "";
    public IReadOnlyList<AutomovilVista> Automoviles { get; set; } = [];
}
public class ParqueoOpciones
{
    public string BaseUrl { get; set; } = "http://localhost:5080/";
    public string FuenteMantenimiento { get; set; } = "json";
    public string FuenteConsultas { get; set; } = "postgres";
}
