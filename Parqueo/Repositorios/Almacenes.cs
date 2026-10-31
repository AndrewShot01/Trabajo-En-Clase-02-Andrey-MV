using System.Linq.Expressions;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Parqueo.Modelos;
namespace Parqueo.Repositorios;

public static class Claves
{
    public static Expression<Func<T, int>> Orden<T>() where T : IRegistro
    {
        var x = Expression.Parameter(typeof(T), "x");
        return Expression.Lambda<Func<T, int>>(Expression.Property(x, typeof(T) == typeof(Ingreso) ? "Consecutivo" : "Id"), x);
    }
    public static Expression<Func<T, bool>> Es<T>(int id) where T : IRegistro
    {
        var x = Expression.Parameter(typeof(T), "x");
        return Expression.Lambda<Func<T, bool>>(Expression.Equal(Expression.Property(x, typeof(T) == typeof(Ingreso) ? "Consecutivo" : "Id"), Expression.Constant(id)), x);
    }
}
public sealed class AlmacenEf(ParqueoContext contexto) : IAlmacen
{
    public IQueryable<T> Consultar<T>() where T : class, IRegistro => contexto.Set<T>().AsNoTracking();
    public void Cambiar<T>(T registro, string operacion) where T : class, IRegistro
    {
        using var transaccion = contexto.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);
        try
        {
            Validacion.Registro(this, registro, operacion);
            if (operacion == "insertar") contexto.Add(registro);
            else if (operacion == "actualizar") contexto.Update(registro);
            else contexto.Remove(registro);
            contexto.SaveChanges();
            transaccion.Commit();
        }
        finally { contexto.ChangeTracker.Clear(); }
    }
    public void Dispose() => contexto.Dispose();
}
public sealed class AlmacenJson : IAlmacen
{
    readonly string directorio;
    readonly FileStream bloqueo;
    readonly JsonSerializerOptions opciones = new() { WriteIndented = true, PropertyNameCaseInsensitive = true };
    public AlmacenJson(string directorio)
    {
        this.directorio = Path.GetFullPath(directorio);
        if (!Directory.Exists(this.directorio)) throw new DirectoryNotFoundException(this.directorio);
        // Una sesión por directorio: evita carreras entre procesos y validaciones sobre datos desactualizados.
        bloqueo = new FileStream(Path.Combine(this.directorio, ".parqueo.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
    }
    string Ruta<T>() => Path.Combine(directorio, typeof(T) == typeof(Automovil) ? "automoviles.json" : typeof(T) == typeof(Estacionamiento) ? "parqueos.json" : typeof(T) == typeof(Ingreso) ? "ingresos.json" : throw new NotSupportedException());
    public IQueryable<T> Consultar<T>() where T : class, IRegistro
    {
        using var stream = File.OpenRead(Ruta<T>());
        return (JsonSerializer.Deserialize<List<T>>(stream, opciones) ?? throw new InvalidDataException("El JSON debe contener una lista.")).AsQueryable();
    }
    public void Cambiar<T>(T registro, string operacion) where T : class, IRegistro
    {
        Validacion.Registro(this, registro, operacion);
        var lista = Consultar<T>().ToList();
        lista.RemoveAll(x => x.Clave == registro.Clave);
        if (operacion != "borrar") lista.Add(registro);
        var ruta = Ruta<T>();
        var temporal = ruta + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            using (var stream = new FileStream(temporal, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            { JsonSerializer.Serialize(stream, lista, opciones); stream.Flush(true); }
            File.Move(temporal, ruta, true);
        }
        finally { if (File.Exists(temporal)) File.Delete(temporal); }
    }
    public void Dispose() => bloqueo.Dispose();
}
