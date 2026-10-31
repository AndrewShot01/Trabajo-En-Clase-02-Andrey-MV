using Microsoft.AspNetCore.Mvc;
using Parqueo.Mvc.Models;
using Parqueo.Mvc.Services;
namespace Parqueo.Mvc.Controllers;

public sealed class AutomovilesController(ParqueoApi api) : Controller
{
    void Fuente() => ViewBag.Fuente = api.Fuente();
    [HttpGet]
    public async Task<IActionResult> Index(ListadoAutomoviles modelo)
    {
        modelo.Fuente = api.Fuente();
        if (!modelo.Desde.HasValue || !modelo.Hasta.HasValue) ModelState.AddModelError("", "Ingrese los dos límites del intervalo.");
        if (modelo.Desde > modelo.Hasta) ModelState.AddModelError("", "El año inicial no puede ser mayor que el final.");
        if (ModelState.IsValid) try { modelo.Automoviles = await api.PorAnios(modelo.Desde!.Value, modelo.Hasta!.Value); } catch (ApiException e) { ModelState.AddModelError("", e.Message); }
        return View(modelo);
    }
    [HttpGet]
    public async Task<IActionResult> Crear()
    {
        Fuente(); var modelo = new AutomovilFormulario { Anio = DateTime.Today.Year };
        try { var todos = await api.Todos(); var max = todos.Select(x => x.Id).DefaultIfEmpty(0).Max(); modelo.Id = max < int.MaxValue ? max + 1 : 0; }
        catch (ApiException e) { ModelState.AddModelError("", e.Message); }
        return View(modelo);
    }
    [HttpPost]
    public async Task<IActionResult> Crear(AutomovilFormulario modelo)
    {
        Fuente(); if (ModelState.IsValid) try { await api.Crear(modelo); TempData["Exito"] = "Automóvil creado correctamente."; return RedirectToAction(nameof(Index)); } catch (ApiException e) { ModelState.AddModelError("", e.Message); }
        return View(modelo);
    }
    [HttpGet]
    public async Task<IActionResult> Detalles(int id)
    {
        Fuente(); try { return View(await api.PorId(id)); } catch (ApiException e) { return Fallo(e); }
    }
    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        Fuente(); try { var a = await api.PorId(id); return View(new AutomovilFormulario { Id = a.Id, Color = a.Color, Anio = a.Anio, Fabricante = a.Fabricante, Tipo = a.Tipo }); } catch (ApiException e) { return Fallo(e); }
    }
    [HttpPost]
    public async Task<IActionResult> Editar([FromRoute] int id, AutomovilFormulario modelo)
    {
        Fuente(); if (id != modelo.Id) ModelState.AddModelError("", "El identificador del formulario no coincide con la URL.");
        if (ModelState.IsValid) try { await api.Actualizar(id, modelo); TempData["Exito"] = "Cambios guardados correctamente."; return RedirectToAction(nameof(Index)); } catch (ApiException e) { ModelState.AddModelError("", e.Message); }
        return View(modelo);
    }
    [HttpGet]
    public async Task<IActionResult> Eliminar(int id)
    {
        Fuente(); try { return View(await api.PorId(id)); } catch (ApiException e) { return Fallo(e); }
    }
    [HttpPost, ActionName("Eliminar")]
    public async Task<IActionResult> ConfirmarEliminar([FromRoute] int id)
    {
        Fuente(); try { await api.Borrar(id); TempData["Exito"] = "Automóvil eliminado correctamente."; return RedirectToAction(nameof(Index)); }
        catch (ApiException e) { if (e.Estado == 404) return Fallo(e); ModelState.AddModelError("", e.Message); try { return View(await api.PorId(id)); } catch (ApiException fallo) { return Fallo(fallo); } }
    }
    IActionResult Fallo(ApiException e) { Response.StatusCode = e.Estado; ViewData["Mensaje"] = e.Message; return View("Fallo"); }
}
