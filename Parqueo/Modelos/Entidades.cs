using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
namespace Parqueo.Modelos;

public interface IRegistro { int Clave { get; } }
// Personalizaciones separadas de los archivos regenerables del scaffold.
public partial class Automovil : IRegistro
{
    [NotMapped, JsonIgnore] public int Clave => Id;
}
public partial class Estacionamiento : IRegistro
{
    [NotMapped, JsonIgnore] public int Clave => Id;
}
public partial class Ingreso : IRegistro
{
    [NotMapped, JsonIgnore] public int Clave => Consecutivo;
}
