using Microsoft.AspNetCore.Mvc;

namespace EcosistemaRentas.Controllers
{
    public class PropertyController : Controller
    {
        // GET: /Property
        public IActionResult Index(string city)
        {
            ViewBag.City = city; // Pasar la ciudad desde la URL a la vista si existe
            return View();
        }

        // GET: /Property/Details/5
        public IActionResult Details(int id)
        {
            ViewBag.PropertyId = id;
            return View();
        }

        // GET: /Property/Create (Solo para OWNERS, la validación final se hace en JS/API)
        public IActionResult Create()
        {
            return View();
        }
    }
}