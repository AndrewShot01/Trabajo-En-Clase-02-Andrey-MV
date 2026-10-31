using Microsoft.AspNetCore.Mvc;
using Parqueo.Repositorios;
namespace Parqueo.Api.Controllers;
[ApiController,Route("api/ingresos")]
public sealed class IngresosController(IIngresos repo,Navegaciones nav):ControllerBase
{
 [HttpGet] public IActionResult Todos()=>Ok(nav.Ingresos(repo.Todos()));
 [HttpGet("{id:int}")] public IActionResult PorId(int id)=>repo.PorId(id) is {} i?Ok(nav.Ingresos([i]).Single()):NotFound();
 [HttpGet("por-tipo")] public IActionResult Tipo([FromQuery]string texto,[FromQuery]DateTime desde,[FromQuery]DateTime hasta)=>Ok(nav.Ingresos(repo.PorTipo(texto,desde,hasta).Select(x=>x.Ingreso)));
 [HttpGet("por-provincia")] public IActionResult Provincia([FromQuery]string texto,[FromQuery]DateTime desde,[FromQuery]DateTime hasta)=>Ok(nav.Ingresos(repo.PorProvincia(texto,desde,hasta).Select(x=>x.Ingreso)));
 [HttpGet("precio-por-hora/{idParqueo:int}")] public IActionResult Precio(int idParqueo)=>Ok(new{idParqueo,precioPorHora=repo.ObtenerPrecioPorHoraPorParqueo(idParqueo)});
 [HttpPost] public IActionResult Insertar(IngresoSolicitud solicitud){repo.Insertar(solicitud.Entidad());return CreatedAtAction(nameof(PorId),new{id=solicitud.Consecutivo},nav.Ingresos([repo.PorId(solicitud.Consecutivo)!]).Single());}
 [HttpPut("{id:int}")] public IActionResult Actualizar(int id,IngresoSolicitud solicitud){if(id!=solicitud.Consecutivo)return BadRequest("La clave de la URL debe coincidir con el cuerpo.");repo.Actualizar(solicitud.Entidad());return NoContent();}
 [HttpDelete("{id:int}")] public IActionResult Borrar(int id){repo.Borrar(id);return NoContent();}
}
