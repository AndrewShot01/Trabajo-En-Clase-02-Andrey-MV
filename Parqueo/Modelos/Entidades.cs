using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
namespace Parqueo.Modelos;

public interface IRegistro { int Clave { get; } }
public partial class Automovil : IRegistro
{
    public int Id { get; set; }
    public string Color { get; set; } = "";
    public int Anio { get; set; }
    public string Fabricante { get; set; } = "";
    public string Tipo { get; set; } = "";
    [NotMapped, JsonIgnore] public int Clave => Id;
}
public partial class Estacionamiento : IRegistro
{
    public int Id { get; set; }
    public string Provincia { get; set; } = "";
    public string Nombre { get; set; } = "";
    public decimal PrecioPorHora { get; set; }
    [NotMapped, JsonIgnore] public int Clave => Id;
}
public partial class Ingreso : IRegistro
{
    public int Consecutivo { get; set; }
    public int IdParqueo { get; set; }
    public int IdAutomovil { get; set; }
    public DateTime Entrada { get; set; }
    public DateTime? Salida { get; set; }
    [NotMapped, JsonIgnore] public int Clave => Consecutivo;
}
