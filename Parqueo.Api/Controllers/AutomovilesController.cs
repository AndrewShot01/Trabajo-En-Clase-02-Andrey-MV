using Microsoft.AspNetCore.Mvc;
using Parqueo.Repositorios;
namespace Parqueo.Api.Controllers;
[ApiController,Route("api/automoviles")]
public sealed class AutomovilesController(IAutomoviles repo,Navegaciones nav):ControllerBase
{
 [HttpGet] public IActionResult Todos()=>Ok(nav.Automoviles(repo.Todos()));
 [HttpGet("{id:int}")] public IActionResult PorId(int id)=>repo.PorId(id) is {} a?Ok(nav.Automoviles([a]).Single()):NotFound();
 [HttpGet("por-color")] public IActionResult Color([FromQuery]string texto)=>Ok(nav.Automoviles(repo.PorColor(texto)));
 [HttpGet("por-anios")] public IActionResult Anios([FromQuery]int desde,[FromQuery]int hasta)=>Ok(nav.Automoviles(repo.PorAnios(desde,hasta)));
 [HttpGet("por-fabricante")] public IActionResult Fabricante([FromQuery]string texto)=>Ok(nav.Automoviles(repo.PorFabricante(texto)));
 [HttpGet("por-tipo")] public IActionResult Tipo([FromQuery]string texto)=>Ok(nav.Automoviles(repo.PorTipo(texto)));
 [HttpPost] public IActionResult Insertar(AutomovilSolicitud solicitud){repo.Insertar(solicitud.Entidad());return CreatedAtAction(nameof(PorId),new{id=solicitud.Id},nav.Automoviles([repo.PorId(solicitud.Id)!]).Single());}
 [HttpPut("{id:int}")] public IActionResult Actualizar(int id,AutomovilSolicitud solicitud){if(id!=solicitud.Id)return BadRequest("La clave de la URL debe coincidir con el cuerpo.");repo.Actualizar(solicitud.Entidad());return NoContent();}
 [HttpDelete("{id:int}")] public IActionResult Borrar(int id){repo.Borrar(id);return NoContent();}
}
