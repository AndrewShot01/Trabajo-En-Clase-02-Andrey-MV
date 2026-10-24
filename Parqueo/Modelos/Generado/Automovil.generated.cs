using System;
using System.Collections.Generic;

namespace Parqueo.Modelos;

public partial class Automovil
{
    public int Id { get; set; }

    public string Color { get; set; } = null!;

    public int Anio { get; set; }

    public string Fabricante { get; set; } = null!;

    public string Tipo { get; set; } = null!;

    [System.Text.Json.Serialization.JsonIgnore]
    public virtual ICollection<Ingreso> Ingreso { get; set; } = new List<Ingreso>();
}
