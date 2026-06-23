using Microsoft.AspNetCore.Mvc;

namespace pruebaAsp.Controllers;

public class AuthController : Controller
{
    public IActionResult Login()
    {
        return View();
    }

    public IActionResult Register()
    {
        return View();
    }
}
