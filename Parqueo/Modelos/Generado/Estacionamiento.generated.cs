using System;
using System.Collections.Generic;

namespace Parqueo.Modelos;

public partial class Estacionamiento
{
    public int Id { get; set; }

    public string Provincia { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public decimal PrecioPorHora { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public virtual ICollection<Ingreso> Ingreso { get; set; } = new List<Ingreso>();
}
