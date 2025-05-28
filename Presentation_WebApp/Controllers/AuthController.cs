using Business.Dtos;
using Business.Services;
using Data.Entities;
using Domain.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Presentation_WebApp.Models;

namespace Presentation_WebApp.Controllers;

public class AuthController(IAuthService authService, UserManager<UserEntity> userManager) : Controller
{
    private readonly IAuthService _authService = authService;
    private readonly UserManager<UserEntity> _userManager = userManager;

    /* ------------------------- Sign In -------------------------*/
    public IActionResult SignIn(string returnUrl = "/")
    {
        ViewBag.ErrorMessage = "";
        ViewBag.ReturnUrl = returnUrl;  
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> SignIn(LoginViewModel model, string returnUrl = "~/")
    {
        ViewBag.ErrorMessage = "";
        ViewBag.ReturnUrl = returnUrl;

        if (!ModelState.IsValid)
       {
           return View(model);
       }

        var signInFormData = model.MapTo<SignInFormData>();
        var result = await _authService.SignInAsync(signInFormData);

        if (result.Succeeded)
            return LocalRedirect(returnUrl);

        ViewBag.ErrorMessage = result.Error;
        return View(model);
    }

    /* ------------------------- Sign Up -------------------------*/

    public IActionResult SignUp()
    {
        ViewBag.ErrorMessage = "";
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> SignUp(SignUpViewModel model)
    {
        ViewBag.ErrorMessage = null;

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            ViewBag.ErrorMessage = string.Join("; ", errors);
            return View(model);
        }

        var signUpFormData = model.MapTo<SignUpFormData>();
        var result = await _authService.SignUpAsync(signUpFormData);

        if (result.Succeeded)
            return RedirectToAction("SignIn", "Auth");

        ViewBag.ErrorMessage = result.Error;
        return View(model); 
    }

    /* ------------------------- Sign Out -------------------------*/

    [Route("logout")]
    public async Task<IActionResult> Logout()
    {
        await _authService.SignOutAsync();
        return LocalRedirect("~/");
    }
}
