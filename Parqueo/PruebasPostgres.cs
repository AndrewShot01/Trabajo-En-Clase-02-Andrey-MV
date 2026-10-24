using System.Text.Json;
using Parqueo.Modelos;
using Parqueo.Repositorios;
namespace Parqueo;

public static class PruebasPostgres
{
    public static void Ejecutar(string ruta)
    {
        using IAlmacen remoto = new AlmacenEf(new FabricaContexto().CreateDbContext([]));
        using IAlmacen local = new AlmacenJson(ruta);
        var a = new Automoviles(remoto); var p = new Parqueos(remoto); var i = new Ingresos(remoto);
        var aj = new Automoviles(local); var pj = new Parqueos(local); var ij = new Ingresos(local);
        int total = 0;
        void Ver(bool ok, string nombre) { if (!ok) throw new Exception("Falló PostgreSQL: " + nombre); total++; Console.WriteLine("OK PostgreSQL: " + nombre); }
        void Igual<T>(IEnumerable<T> x, IEnumerable<T> y, string nombre) => Ver(JsonElement.DeepEquals(JsonSerializer.SerializeToElement(x), JsonSerializer.SerializeToElement(y)), nombre);
        void Rechaza(Action accion, string nombre) { bool rechazo = false; try { accion(); } catch (Exception e) when (e is ArgumentException or InvalidOperationException or KeyNotFoundException) { rechazo = true; } Ver(rechazo, nombre); }
        var antesA = remoto.Consultar<Automovil>().OrderBy(x => x.Id).ToList();
        var antesP = remoto.Consultar<Estacionamiento>().OrderBy(x => x.Id).ToList();
        var antesI = remoto.Consultar<Ingreso>().OrderBy(x => x.Consecutivo).ToList();
        Igual(antesA, local.Consultar<Automovil>().OrderBy(x => x.Id), "automóviles SQL/JSON");
        Igual(antesP, local.Consultar<Estacionamiento>().OrderBy(x => x.Id), "parqueos SQL/JSON");
        Igual(antesI, local.Consultar<Ingreso>().OrderBy(x => x.Consecutivo), "ingresos SQL/JSON");
        Igual(a.PorColor("roj").OrderBy(x => x.Id), aj.PorColor("roj").OrderBy(x => x.Id), "color aproximado");
        Igual(a.PorAnios(2020, 2022).OrderBy(x => x.Id), aj.PorAnios(2020, 2022).OrderBy(x => x.Id), "intervalo de años");
        Igual(a.PorFabricante("yot").OrderBy(x => x.Id), aj.PorFabricante("yot").OrderBy(x => x.Id), "fabricante");
        Igual(a.PorTipo("4x").OrderBy(x => x.Id), aj.PorTipo("4x").OrderBy(x => x.Id), "tipo");
        Igual(p.PorProvincia("jOsÉ").OrderBy(x => x.Id), pj.PorProvincia("jOsÉ").OrderBy(x => x.Id), "provincia");
        Igual(p.PorNombre("flor").OrderBy(x => x.Id), pj.PorNombre("flor").OrderBy(x => x.Id), "nombre");
        Igual(p.PorPrecios(900, 1200).OrderBy(x => x.Id), pj.PorPrecios(900, 1200).OrderBy(x => x.Id), "precios");
        var desde = new DateTime(2026, 10, 1); var hasta = new DateTime(2026, 10, 16);
        Igual(i.PorTipo("sed", desde, hasta).OrderBy(x => x.Ingreso.Consecutivo), ij.PorTipo("sed", desde, hasta).OrderBy(x => x.Ingreso.Consecutivo), "listado por tipo y montos");
        Igual(i.PorProvincia("here", desde, hasta).OrderBy(x => x.Ingreso.Consecutivo), ij.PorProvincia("here", desde, hasta).OrderBy(x => x.Ingreso.Consecutivo), "listado por provincia y montos");
        Ver(i.PorId(1)!.MontoTotal == 675 && i.PorId(14)!.MontoTotal == null, "monto cerrado y abierto");
        Ver(i.PorTipo("sed", desde.AddHours(8), desde.AddHours(8)).Count == 1, "fecha inclusiva exacta");
        var clave = Random.Shared.Next(100000000, 200000000);
        if (a.PorId(clave) != null || p.PorId(clave) != null || i.PorId(clave) != null) throw new Exception("Clave de prueba ocupada.");
        bool autoCreado = false, parqueoCreado = false, ingresoCreado = false;
        try
        {
            a.Insertar(new() { Id = clave, Color = "Verde", Anio = 2023, Fabricante = "Prueba", Tipo = "moto" }); autoCreado = true;
            var auto = a.PorId(clave)!; auto.Color = "Gris"; a.Actualizar(auto); Ver(a.PorId(clave)!.Color == "Gris", "CRUD automóvil: insertar y actualizar");
            p.Insertar(new() { Id = clave, Provincia = "Cartago", Nombre = "Prueba", PrecioPorHora = 500 }); parqueoCreado = true;
            var parqueo = p.PorId(clave)!; parqueo.PrecioPorHora = 600; p.Actualizar(parqueo); Ver(p.PorId(clave)!.PrecioPorHora == 600, "CRUD parqueo: insertar y actualizar");
            i.Insertar(new() { Consecutivo = clave, IdAutomovil = clave, IdParqueo = clave, Entrada = desde }); ingresoCreado = true;
            Ver(i.PorId(clave)!.MontoTotal == null, "ingreso nuevo abierto");
            var ingreso = i.PorId(clave)!; ingreso.Salida = desde.AddHours(2); i.Actualizar(ingreso); Ver(i.PorId(clave)!.MontoTotal == 1200, "CRUD ingreso: actualizar salida y monto");
            Rechaza(() => a.Borrar(clave), "protección del automóvil relacionado");
            Rechaza(() => p.Borrar(clave), "protección del parqueo relacionado");
            Rechaza(() => a.Insertar(auto), "clave duplicada");
            ingreso.Salida = desde.AddMinutes(-1); Rechaza(() => i.Actualizar(ingreso), "salida inválida");
            Rechaza(() => a.Actualizar(new() { Id = int.MaxValue }), "actualización inexistente");
            Rechaza(() => p.Borrar(int.MaxValue), "borrado inexistente");
        }
        finally
        {
            if (ingresoCreado) i.Borrar(clave);
            if (autoCreado) a.Borrar(clave);
            if (parqueoCreado) p.Borrar(clave);
        }
        Ver(a.PorId(clave) == null && p.PorId(clave) == null && i.PorId(clave) == null, "borrado de las tres entidades de prueba");
        Igual(antesA, remoto.Consultar<Automovil>().OrderBy(x => x.Id), "automóviles originales conservados");
        Igual(antesP, remoto.Consultar<Estacionamiento>().OrderBy(x => x.Id), "parqueos originales conservados");
        Igual(antesI, remoto.Consultar<Ingreso>().OrderBy(x => x.Consecutivo), "ingresos originales conservados");
        Console.WriteLine($"{total} comprobaciones PostgreSQL correctas; datos de prueba CRUD retirados.");
    }
}
