using Microsoft.AspNetCore.Mvc;
namespace Parqueo.Mvc.Controllers;
public class HomeController:Controller
{
 public IActionResult Index()=>RedirectToAction("Index","Automoviles");
 public IActionResult Error(){Response.StatusCode=500;ViewData["Mensaje"]="No fue posible completar la solicitud. Intente nuevamente.";return View("~/Views/Automoviles/Fallo.cshtml");}
}
