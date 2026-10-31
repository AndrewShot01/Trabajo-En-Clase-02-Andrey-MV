using Parqueo;
using Parqueo.Repositorios;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
namespace Parqueo.Api;
public sealed class AccesoDatos(IConfiguration config,IHttpContextAccessor http)
{
 readonly SemaphoreSlim semaforo=new(1,1);
 public IAlmacen Crear()
 {
  var fuente=http.HttpContext?.Request.Headers["X-Fuente-Datos"].ToString();
  if(string.IsNullOrEmpty(fuente))fuente=config["Parqueo:Fuente"] ?? "json";
  fuente=fuente.ToLowerInvariant();
  if(fuente is not ("json" or "postgres"))throw new ArgumentException("X-Fuente-Datos debe ser json o postgres.");
  http.HttpContext!.Response.Headers["X-Fuente-Datos"]=fuente;
  if(fuente=="postgres")return new AlmacenEf(new FabricaContexto().CreateDbContext([]));
  semaforo.Wait();
  try{return new SesionJson(new AlmacenJson(config["Parqueo:RutaJson"]!),semaforo);}
  catch{semaforo.Release();throw;}
 }
 sealed class SesionJson(IAlmacen almacen,SemaphoreSlim semaforo):IAlmacen
 {
  bool cerrado;
  public IQueryable<T> Consultar<T>() where T:class,Parqueo.Modelos.IRegistro=>almacen.Consultar<T>();
  public void Cambiar<T>(T r,string operacion) where T:class,Parqueo.Modelos.IRegistro=>almacen.Cambiar(r,operacion);
  public void Dispose(){if(cerrado)return;cerrado=true;try{almacen.Dispose();}finally{semaforo.Release();}}
 }
}
public static class Errores
{
 public static async Task Responder(HttpContext contexto,Exception e)
 {
  var (estado,titulo)=e switch {
   KeyNotFoundException=>(404,"Registro no encontrado"),
   ArgumentException=>(400,"Parámetros inválidos"),
   InvalidOperationException=>(409,"La operación entra en conflicto con los datos"),
   DbUpdateException=>(409,"No se pudo guardar por una restricción de datos"),
   Npgsql.NpgsqlException=>(503,"PostgreSQL no está disponible"),
   IOException or JsonException or UnauthorizedAccessException=>(503,"No se pudo acceder a los datos JSON"),
   _=>(500,"No se pudo completar la solicitud")
  };
  var detalle=e is ArgumentException or KeyNotFoundException or InvalidOperationException ? e.Message : "Revise la configuración local o los registros del servidor.";
  await Results.Problem(statusCode:estado,title:titulo,detail:detalle).ExecuteAsync(contexto);
 }
}
