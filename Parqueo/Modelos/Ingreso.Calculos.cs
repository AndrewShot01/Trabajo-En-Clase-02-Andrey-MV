using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
namespace Parqueo.Modelos;
public partial class Ingreso
{
 [NotMapped, JsonIgnore] public decimal? PrecioPorHora { get; set; }
 [NotMapped, JsonIgnore] public decimal? DuracionMinutos => Salida is DateTime fin ? (decimal)(fin - Entrada).Ticks / TimeSpan.TicksPerMinute : null;
 [NotMapped, JsonIgnore] public decimal? DuracionHoras => DuracionMinutos / 60m;
 [NotMapped, JsonIgnore] public decimal? MontoTotal => DuracionHoras is decimal horas && PrecioPorHora is decimal precio
   ? decimal.Round(horas * precio, 2, MidpointRounding.AwayFromZero) : null;
}
