using System;
using System.Collections.Generic;

namespace Parqueo.Modelos;

public partial class Ingreso
{
    public int Consecutivo { get; set; }

    public int IdParqueo { get; set; }

    public int IdAutomovil { get; set; }

    public DateTime Entrada { get; set; }

    public DateTime? Salida { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public virtual Automovil IdAutomovilNavigation { get; set; } = null!;

    [System.Text.Json.Serialization.JsonIgnore]
    public virtual Estacionamiento IdParqueoNavigation { get; set; } = null!;
}
