using Microsoft.AspNetCore.Mvc;

namespace pruebaAsp.Controllers;

public class UserController : Controller
{
    public IActionResult Dashboard() => View();
    public IActionResult MyReservations() => View();
    public IActionResult Favorites() => View();
}