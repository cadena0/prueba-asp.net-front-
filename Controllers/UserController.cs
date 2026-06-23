using Microsoft.AspNetCore.Mvc;

namespace EcosistemaRentas.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Dashboard() => View();
        public IActionResult MyReservations() => View();
    }
}