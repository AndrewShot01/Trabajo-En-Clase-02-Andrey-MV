using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Parqueo.Mvc.Models;
namespace Parqueo.Mvc.Services;
public sealed class ApiException(int estado,string mensaje):Exception(mensaje)
{
 public int Estado {get;}=estado;
}
public sealed class ParqueoApi(HttpClient http,IOptionsSnapshot<ParqueoOpciones> opciones)
{
 public string Fuente(bool consulta=false)=>consulta?opciones.Value.FuenteConsultas:opciones.Value.FuenteMantenimiento;
 async Task<HttpResponseMessage> Enviar(HttpMethod metodo,string ruta,bool consulta,object? datos=null)
 {
  using var req=new HttpRequestMessage(metodo,ruta);
  req.Headers.Add("X-Fuente-Datos",Fuente(consulta));
  if(datos!=null)req.Content=JsonContent.Create(datos);
  HttpResponseMessage respuesta;
  try{respuesta=await http.SendAsync(req);}
  catch(HttpRequestException){throw new ApiException(503,"No se pudo conectar con la API. Inicie Parqueo.Api en el puerto configurado.");}
  catch(TaskCanceledException){throw new ApiException(503,"La API tardó demasiado en responder. Intente de nuevo.");}
  if(respuesta.IsSuccessStatusCode)return respuesta;
  using(respuesta)
  {
   var mensaje=respuesta.StatusCode switch{
    HttpStatusCode.NotFound=>"El registro solicitado no existe.",
    HttpStatusCode.Conflict=>"No se pudo guardar: clave duplicada o registro con ingresos relacionados.",
    HttpStatusCode.BadRequest=>"Revise los datos ingresados.",
    _=>"La API no pudo completar la operación. Revise su conexión y la fuente configurada."
   };
   if(respuesta.StatusCode is HttpStatusCode.Conflict or HttpStatusCode.BadRequest)
   {
    try{using var doc=JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());if(doc.RootElement.TryGetProperty("detail",out var d))mensaje=d.GetString()??mensaje;}catch(JsonException){}
   }
   throw new ApiException((int)respuesta.StatusCode,mensaje);
  }
 }
 async Task<T> Leer<T>(string ruta,bool consulta=false)
 {
  using var respuesta=await Enviar(HttpMethod.Get,ruta,consulta);
  return await respuesta.Content.ReadFromJsonAsync<T>()??throw new ApiException(502,"La API devolvió una respuesta vacía.");
 }
 public Task<List<AutomovilVista>> Todos()=>Leer<List<AutomovilVista>>("api/automoviles");
 public Task<List<AutomovilVista>> PorAnios(int desde,int hasta)=>Leer<List<AutomovilVista>>($"api/automoviles/por-anios?desde={desde}&hasta={hasta}");
 public Task<AutomovilVista> PorId(int id)=>Leer<AutomovilVista>($"api/automoviles/{id}");
 public Task<List<AutomovilVista>> PorTipo(string tipo)=>Leer<List<AutomovilVista>>($"api/automoviles/por-tipo?texto={Uri.EscapeDataString(tipo)}",true);
 public Task<List<IngresoVista>> Ingresos(int id)=>Leer<List<IngresoVista>>($"api/ingresos/por-automovil/{id}",true);
 public async Task Crear(AutomovilFormulario datos){using var r=await Enviar(HttpMethod.Post,"api/automoviles",false,datos);}
 public async Task Actualizar(int id,AutomovilFormulario datos){using var r=await Enviar(HttpMethod.Put,$"api/automoviles/{id}",false,datos);}
 public async Task Borrar(int id){using var r=await Enviar(HttpMethod.Delete,$"api/automoviles/{id}",false);}
}
