using Parqueo.Modelos;
using Parqueo.Repositorios;
using Microsoft.EntityFrameworkCore;
namespace Parqueo;

public static class Pruebas
{
    public static void Ejecutar(string origen)
    {
        var temporal = Path.Combine(Path.GetTempPath(), "prq-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temporal);
        foreach (var archivo in Directory.GetFiles(origen, "*.json")) File.Copy(archivo, Path.Combine(temporal, Path.GetFileName(archivo)));
        int total = 0;
        void Ver(bool ok, string nombre) { if (!ok) throw new Exception("Falló: " + nombre); total++; Console.WriteLine("OK " + nombre); }
        void Rechaza(Action accion, string nombre)
        { bool rechazo = false; try { accion(); } catch (Exception e) when (e is ArgumentException or InvalidOperationException or KeyNotFoundException) { rechazo = true; } Ver(rechazo, nombre); }
        try
        {
            using (var d = new AlmacenJson(temporal))
            {
                var a = new Automoviles(d); var p = new Parqueos(d); var i = new Ingresos(d);
                bool bloqueado = false;
                try { using var segundo = new AlmacenJson(temporal); } catch (IOException) { bloqueado = true; }
                Ver(bloqueado, "bloqueo entre sesiones JSON");
                Ver(d.Consultar<Automovil>().Count() == 5 && d.Consultar<Estacionamiento>().Count() == 2 && d.Consultar<Ingreso>().Count() == 15, "cantidades iniciales");
                Ver(a.PorColor("ROJO").Count == 2, "color aproximado");
                Ver(a.PorFabricante("yot").Count == 2, "fabricante aproximado");
                Ver(a.PorTipo("4x").Count == 2, "tipo aproximado");
                Ver(a.PorAnios(2020, 2022).Count == 2, "años inclusivos");
                Ver(p.PorProvincia("jOsÉ").Count == 1, "provincia aproximada");
                Ver(p.PorNombre("flor").Count == 1, "nombre aproximado");
                Ver(p.PorPrecios(900, 1200).Count == 2, "precios inclusivos");
                Ver(a.PorId(999) == null && a.PorColor("inexistente").Count == 0, "ausencias");
                Ver(i.ObtenerPrecioPorHoraPorParqueo(1) == 1200, "tarifa por parqueo");
                var primero = i.PorId(1)!;
                Ver(primero.DuracionMinutos == 45 && primero.DuracionHoras == 0.75m && primero.MontoTotal == 675, "cálculos conocidos");
                var abierto = i.PorId(14)!;
                Ver(abierto.DuracionMinutos == null && abierto.DuracionHoras == null && abierto.MontoTotal == null, "salida nula");
                Ver(i.PorTipo("sed", new(2026, 10, 1), new(2026, 10, 16)).Count == 6, "ingresos por tipo");
                Ver(i.PorProvincia("here", new(2026, 10, 1), new(2026, 10, 16)).Count == 8, "ingresos por provincia");
                Ver(i.PorTipo("sed", primero.Entrada, primero.Entrada).Count == 1, "fecha exacta inclusiva");
                Ver(i.PorProvincia("jOs", new(2026, 10, 1), new(2026, 10, 16)).Any(x => x.Ingreso.MontoTotal == null), "monto abierto en listado");
                var redondeo = new Ingreso { Entrada = new(2026, 1, 1), Salida = new DateTime(2026, 1, 1).AddMinutes(1), PrecioPorHora = 1m };
                Ver(redondeo.MontoTotal == 0.02m, "redondeo a dos decimales");
                a.Insertar(new() { Id = 90, Color = "Verde", Anio = 2023, Fabricante = "Prueba", Tipo = "moto" });
                var nuevo = a.PorId(90)!; nuevo.Color = "Gris"; a.Actualizar(nuevo); Ver(a.PorId(90)!.Color == "Gris", "insertar y actualizar automóvil");
                p.Insertar(new() { Id = 90, Provincia = "Cartago", Nombre = "Prueba", PrecioPorHora = 500 });
                var parqueo = p.PorId(90)!; parqueo.PrecioPorHora = 600; p.Actualizar(parqueo); Ver(p.PorId(90)!.PrecioPorHora == 600, "insertar y actualizar parqueo");
                i.Insertar(new() { Consecutivo = 90, IdAutomovil = 90, IdParqueo = 90, Entrada = new(2026, 10, 20) });
                var ingreso = i.PorId(90)!; ingreso.Salida = ingreso.Entrada.AddHours(2); i.Actualizar(ingreso); Ver(i.PorId(90)!.MontoTotal == 1200, "insertar y actualizar ingreso");
                Rechaza(() => a.Borrar(90), "no borrar padre relacionado");
                Rechaza(() => p.Borrar(90), "no borrar parqueo relacionado");
                Rechaza(() => a.Insertar(nuevo), "no duplicar clave");
                Rechaza(() => i.Insertar(new() { Consecutivo = 91, IdAutomovil = 999, IdParqueo = 90, Entrada = DateTime.Today }), "clave foránea inválida");
                ingreso.Salida = ingreso.Entrada.AddMinutes(-1); Rechaza(() => i.Actualizar(ingreso), "salida anterior");
                Rechaza(() => a.PorAnios(2024, 2020), "rango invertido");
                Rechaza(() => a.PorColor(" "), "texto vacío");
                Rechaza(() => i.ObtenerPrecioPorHoraPorParqueo(999), "tarifa inexistente");
                Rechaza(() => p.Actualizar(new() { Id = 999 }), "actualización inexistente");
                Rechaza(() => a.Borrar(999), "borrado inexistente");
                i.Borrar(90); a.Borrar(90); p.Borrar(90);
                Ver(i.PorId(90) == null && a.PorId(90) == null && p.PorId(90) == null, "borrar las tres entidades");
                var archivo = Path.Combine(temporal, "automoviles.json");
                var anterior = File.ReadAllText(archivo);
                File.SetAttributes(archivo, FileAttributes.ReadOnly);
                bool falloEscritura = false;
                try { a.Insertar(new() { Id = 91, Color = "Azul", Anio = 2020, Fabricante = "Prueba", Tipo = "moto" }); }
                catch (Exception e) when (e is IOException or UnauthorizedAccessException) { falloEscritura = true; }
                finally { File.SetAttributes(archivo, FileAttributes.Normal); }
                Ver(falloEscritura && File.ReadAllText(archivo) == anterior, "fallo de escritura conserva original");
                Ver(Directory.GetFiles(temporal, "*.tmp").Length == 0, "limpieza del temporal fallido");
                File.WriteAllText(archivo, "JSON inválido");
                bool corrupto = false;
                try { a.PorId(1); }
                catch (System.Text.Json.JsonException) { corrupto = true; }
                finally { File.WriteAllText(archivo, anterior); }
                Ver(corrupto, "JSON inválido informa error");
            }
            using (var d = new AlmacenJson(temporal)) Ver(d.Consultar<Ingreso>().Count() == 15 && d.Consultar<Automovil>().Count() == 5, "persistencia al reabrir");
            using (var ctx = new ParqueoContext(new DbContextOptionsBuilder<ParqueoContext>().UseSqlServer("Server=localhost;Database=SoloTraduccion;Integrated Security=true;TrustServerCertificate=true").Options))
            {
                var sql = ctx.Ingresos.Where(x => x.Entrada >= new DateTime(2026, 10, 1)).Join(ctx.Automoviles, x => x.IdAutomovil, x => x.Id, (i, a) => new { i, a }).Where(x => x.a.Tipo.ToUpper().Contains("SED")).ToQueryString();
                Ver(sql.Contains("PRQIngresosAutomoviles") && sql.Contains("JOIN"), "traducción SQL sin conexión");
            }
            Console.WriteLine($"{total} comprobaciones correctas. SQL Server remoto no probado. Copias de prueba: {temporal}");
        }
        catch { Console.Error.WriteLine("Datos de diagnóstico: " + temporal); throw; }
    }
}
