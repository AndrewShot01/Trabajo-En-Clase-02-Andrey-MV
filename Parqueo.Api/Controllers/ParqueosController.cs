using Microsoft.AspNetCore.Mvc;
using Parqueo.Repositorios;
namespace Parqueo.Api.Controllers;
[ApiController,Route("api/parqueos")]
public sealed class ParqueosController(IParqueos repo,Navegaciones nav):ControllerBase
{
 [HttpGet] public IActionResult Todos()=>Ok(nav.Parqueos(repo.Todos()));
 [HttpGet("{id:int}")] public IActionResult PorId(int id)=>repo.PorId(id) is {} p?Ok(nav.Parqueos([p]).Single()):NotFound();
 [HttpGet("por-provincia")] public IActionResult Provincia([FromQuery]string texto)=>Ok(nav.Parqueos(repo.PorProvincia(texto)));
 [HttpGet("por-nombre")] public IActionResult Nombre([FromQuery]string texto)=>Ok(nav.Parqueos(repo.PorNombre(texto)));
 [HttpGet("por-precios")] public IActionResult Precios([FromQuery]decimal desde,[FromQuery]decimal hasta)=>Ok(nav.Parqueos(repo.PorPrecios(desde,hasta)));
 [HttpPost] public IActionResult Insertar(ParqueoSolicitud solicitud){repo.Insertar(solicitud.Entidad());return CreatedAtAction(nameof(PorId),new{id=solicitud.Id},nav.Parqueos([repo.PorId(solicitud.Id)!]).Single());}
 [HttpPut("{id:int}")] public IActionResult Actualizar(int id,ParqueoSolicitud solicitud){if(id!=solicitud.Id)return BadRequest("La clave de la URL debe coincidir con el cuerpo.");repo.Actualizar(solicitud.Entidad());return NoContent();}
 [HttpDelete("{id:int}")] public IActionResult Borrar(int id){repo.Borrar(id);return NoContent();}
}
