using Microsoft.AspNetCore.Mvc;

namespace Presentation_WebApp.Controllers;

public class AuthController : Controller
{
    [Route("Sign-Up")]
    public IActionResult SignUp()
    {
        return View();
    }

    [Route("Sign-In")]
    public IActionResult SignIn()
    {
        return View();
    }

    public new IActionResult SignOut()
    {
        return RedirectToAction("Index", "Home");
    }
}
