using Parqueo.Modelos;
namespace Parqueo.Repositorios;

public class Repositorio<T>(IAlmacen almacen) : IRepositorio<T> where T : class, IRegistro
{
    protected readonly IAlmacen Datos = almacen;
    public virtual T? PorId(int id)
    {
        if (id <= 0) throw new ArgumentException("La clave debe ser positiva.");
        return Datos.Consultar<T>().SingleOrDefault(Claves.Es<T>(id));
    }
    public void Insertar(T registro) => Datos.Cambiar(registro, "insertar");
    public void Actualizar(T registro) => Datos.Cambiar(registro, "actualizar");
    public void Borrar(int id) => Datos.Cambiar(PorId(id) ?? throw new KeyNotFoundException("Registro inexistente."), "borrar");
}
public sealed class Automoviles(IAlmacen datos) : Repositorio<Automovil>(datos), IAutomoviles
{
    public IReadOnlyList<Automovil> PorColor(string texto)
    { var t = Validacion.Texto(texto).ToUpperInvariant(); return Datos.Consultar<Automovil>().Where(x => x.Color.ToUpper().Contains(t)).ToList(); }
    public IReadOnlyList<Automovil> PorFabricante(string texto)
    { var t = Validacion.Texto(texto).ToUpperInvariant(); return Datos.Consultar<Automovil>().Where(x => x.Fabricante.ToUpper().Contains(t)).ToList(); }
    public IReadOnlyList<Automovil> PorTipo(string texto)
    { var t = Validacion.Texto(texto).ToUpperInvariant(); return Datos.Consultar<Automovil>().Where(x => x.Tipo.ToUpper().Contains(t)).ToList(); }
    public IReadOnlyList<Automovil> PorAnios(int desde, int hasta)
    { Validacion.Rango(desde, hasta); if (desde < 1886 || hasta > 2100) throw new ArgumentException("Años inválidos."); return Datos.Consultar<Automovil>().Where(x => x.Anio >= desde && x.Anio <= hasta).ToList(); }
}
public sealed class Parqueos(IAlmacen datos) : Repositorio<Estacionamiento>(datos), IParqueos
{
    public IReadOnlyList<Estacionamiento> PorProvincia(string texto)
    { var t = Validacion.Texto(texto).ToUpperInvariant(); return Datos.Consultar<Estacionamiento>().Where(x => x.Provincia.ToUpper().Contains(t)).ToList(); }
    public IReadOnlyList<Estacionamiento> PorNombre(string texto)
    { var t = Validacion.Texto(texto).ToUpperInvariant(); return Datos.Consultar<Estacionamiento>().Where(x => x.Nombre.ToUpper().Contains(t)).ToList(); }
    public IReadOnlyList<Estacionamiento> PorPrecios(decimal desde, decimal hasta)
    { Validacion.Rango(desde, hasta); if (desde < 0) throw new ArgumentException("Precio negativo."); return Datos.Consultar<Estacionamiento>().Where(x => x.PrecioPorHora >= desde && x.PrecioPorHora <= hasta).ToList(); }
}
public sealed class Ingresos(IAlmacen datos) : Repositorio<Ingreso>(datos), IIngresos
{
    public decimal ObtenerPrecioPorHoraPorParqueo(int id) => new Parqueos(Datos).PorId(id)?.PrecioPorHora ?? throw new KeyNotFoundException("Parqueo inexistente.");
    public override Ingreso? PorId(int id)
    { var i = base.PorId(id); if (i is not null) i.PrecioPorHora = ObtenerPrecioPorHoraPorParqueo(i.IdParqueo); return i; }
    public IReadOnlyList<DetalleIngreso> PorTipo(string tipo, DateTime desde, DateTime hasta) => Buscar(tipo, desde, hasta, true);
    public IReadOnlyList<DetalleIngreso> PorProvincia(string provincia, DateTime desde, DateTime hasta) => Buscar(provincia, desde, hasta, false);
    IReadOnlyList<DetalleIngreso> Buscar(string texto, DateTime desde, DateTime hasta, bool porTipo)
    {
        var t = Validacion.Texto(texto).ToUpperInvariant(); Validacion.Rango(desde, hasta);
        var consulta = from i in Datos.Consultar<Ingreso>()
                       join a in Datos.Consultar<Automovil>() on i.IdAutomovil equals a.Id
                       join p in Datos.Consultar<Estacionamiento>() on i.IdParqueo equals p.Id
                       where i.Entrada >= desde && i.Entrada <= hasta && (porTipo ? a.Tipo.ToUpper().Contains(t) : p.Provincia.ToUpper().Contains(t))
                       select new DetalleIngreso(i, a, p);
        var resultado = consulta.ToList();
        // Una llamada por parqueo distinto, no una consulta por ingreso.
        var precios = resultado.Select(x => x.Parqueo.Id).Distinct().ToDictionary(id => id, ObtenerPrecioPorHoraPorParqueo);
        foreach (var fila in resultado) fila.Ingreso.PrecioPorHora = precios[fila.Parqueo.Id];
        return resultado;
    }
}
