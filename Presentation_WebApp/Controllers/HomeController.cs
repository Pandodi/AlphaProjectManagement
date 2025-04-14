using Microsoft.AspNetCore.Mvc;

namespace Presentation_WebApp.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
