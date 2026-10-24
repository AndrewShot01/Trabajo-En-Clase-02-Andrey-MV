using Parqueo;
using Parqueo.Repositorios;
using System.Text.Json;
try
{
    var indice = Array.IndexOf(args, "--datos");
    if (indice >= 0 && indice + 1 >= args.Length) throw new ArgumentException("Indique la ruta después de --datos.");
    var ruta = indice >= 0 ? args[indice + 1] : Path.Combine(AppContext.BaseDirectory, "Datos");
    if (args.Contains("--pruebas")) { Pruebas.Ejecutar(ruta); return; }
    using IAlmacen datos = args.Contains("--sql") ? new AlmacenEf(new FabricaContexto().CreateDbContext([])) : new AlmacenJson(ruta);
    var repositorio = new Ingresos(datos);
    var filas = repositorio.PorProvincia("San", DateTime.MinValue, DateTime.MaxValue);
    Console.WriteLine(JsonSerializer.Serialize(filas, new JsonSerializerOptions { WriteIndented = true }));
}
catch (Exception e) when (e is not OutOfMemoryException)
{ Console.Error.WriteLine($"No se pudo completar la operación: {e.Message}"); Environment.ExitCode = 1; }
