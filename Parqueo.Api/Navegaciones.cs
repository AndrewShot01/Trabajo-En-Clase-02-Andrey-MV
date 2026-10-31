using Parqueo.Modelos;
using Parqueo.Repositorios;
namespace Parqueo.Api;
// Carga las relaciones en lotes y las proyecta sin referencias circulares.
public sealed class Navegaciones(IAlmacen datos, IIngresos repositorio)
{
    static ResumenIngreso Resumen(Ingreso i) => new(i.Consecutivo, i.IdParqueo, i.IdAutomovil, i.Entrada, i.Salida);
    public IReadOnlyList<AutomovilRespuesta> Automoviles(IEnumerable<Automovil> registros)
    {
        var lista = registros.ToList(); var ids = lista.Select(x => x.Id).ToArray();
        var hijos = datos.Consultar<Ingreso>().Where(x => ids.Contains(x.IdAutomovil)).OrderBy(x => x.Consecutivo).ToList();
        foreach (var a in lista) a.Ingreso = hijos.Where(x => x.IdAutomovil == a.Id).ToList();
        return lista.Select(a => new AutomovilRespuesta(a.Id, a.Color, a.Anio, a.Fabricante, a.Tipo, a.Ingreso.Select(Resumen).ToList())).ToList();
    }
    public IReadOnlyList<ParqueoRespuesta> Parqueos(IEnumerable<Estacionamiento> registros)
    {
        var lista = registros.ToList(); var ids = lista.Select(x => x.Id).ToArray();
        var hijos = datos.Consultar<Ingreso>().Where(x => ids.Contains(x.IdParqueo)).OrderBy(x => x.Consecutivo).ToList();
        foreach (var p in lista) p.Ingreso = hijos.Where(x => x.IdParqueo == p.Id).ToList();
        return lista.Select(p => new ParqueoRespuesta(p.Id, p.Provincia, p.Nombre, p.PrecioPorHora, p.Ingreso.Select(Resumen).ToList())).ToList();
    }
    public IReadOnlyList<IngresoRespuesta> Ingresos(IEnumerable<Ingreso> registros)
    {
        var lista = registros.ToList(); var autosIds = lista.Select(x => x.IdAutomovil).Distinct().ToArray(); var parqueosIds = lista.Select(x => x.IdParqueo).Distinct().ToArray();
        var autos = datos.Consultar<Automovil>().Where(x => autosIds.Contains(x.Id)).ToDictionary(x => x.Id);
        var parqueos = datos.Consultar<Estacionamiento>().Where(x => parqueosIds.Contains(x.Id)).ToDictionary(x => x.Id);
        var tarifas = parqueosIds.ToDictionary(id => id, repositorio.ObtenerPrecioPorHoraPorParqueo);
        return lista.Select(i =>
        {
            i.IdAutomovilNavigation = autos[i.IdAutomovil]; i.IdParqueoNavigation = parqueos[i.IdParqueo]; i.PrecioPorHora = tarifas[i.IdParqueo];
            var a = i.IdAutomovilNavigation; var p = i.IdParqueoNavigation;
            return new IngresoRespuesta(i.Consecutivo, i.IdParqueo, i.IdAutomovil, i.Entrada, i.Salida, i.DuracionMinutos, i.DuracionHoras, i.MontoTotal,
             new(a.Id, a.Color, a.Anio, a.Fabricante, a.Tipo), new(p.Id, p.Provincia, p.Nombre, p.PrecioPorHora));
        }).ToList();
    }
}
