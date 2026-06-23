using Microsoft.AspNetCore.Mvc;

namespace EcosistemaRentas.Controllers
{
    public class OwnerController : Controller
    {
        public IActionResult Dashboard() => View();
    }
}