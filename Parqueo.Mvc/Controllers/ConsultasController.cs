using Microsoft.AspNetCore.Mvc;
using Parqueo.Mvc.Models;
using Parqueo.Mvc.Services;
namespace Parqueo.Mvc.Controllers;

public sealed class ConsultasController(ParqueoApi api) : Controller
{
    public static readonly string[] Tipos = ["sedán", "4x4", "moto"];
    [HttpGet]
    public async Task<IActionResult> Index(string tipo = "sedán")
    {
        var modelo = new MaestroDetalle { Tipo = tipo, Fuente = api.Fuente(true) };
        if (!Tipos.Contains(tipo)) ModelState.AddModelError("", "Seleccione un tipo de automóvil válido.");
        if (ModelState.IsValid) try { modelo.Automoviles = await api.PorTipo(tipo); } catch (ApiException e) { ModelState.AddModelError("", e.Message); }
        return View(modelo);
    }
    [HttpGet]
    public async Task<IActionResult> Ingresos(int id)
    {
        if (id <= 0) return BadRequest(new { mensaje = "Identificador inválido." });
        try { return Json(new { fuente = api.Fuente(true), ingresos = await api.Ingresos(id) }); }
        catch (ApiException e) { return StatusCode(e.Estado, new { mensaje = e.Message }); }
    }
}
