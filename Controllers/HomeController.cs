using Microsoft.AspNetCore.Mvc;
using SIGRE_PYME.Filters;

namespace SIGRE_PYME.Controllers
{
    [SesionActiva]
    public class HomeController : Controller
    {
        public IActionResult AcercaDe()
        {
            return View();
        }

        public IActionResult Contacto()
        {
            return View();
        }
    }
}