using Microsoft.AspNetCore.Mvc;
using SIGRE_PYME.Data;
using SIGRE_PYME.Filters;
using SIGRE_PYME.Models;

namespace SIGRE_PYME.Controllers
{
    [SesionActiva]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult AcercaDe()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Contacto()
        {
            return View(new ContactoMensaje());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Contacto(ContactoMensaje contacto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Complete correctamente todos los campos del formulario.";
                return View(contacto);
            }

            contacto.NombreCompleto = contacto.NombreCompleto.Trim();
            contacto.Telefono = contacto.Telefono.Trim();
            contacto.CorreoElectronico = contacto.CorreoElectronico.Trim();
            contacto.Mensaje = contacto.Mensaje.Trim();
            contacto.FechaEnvio = DateTime.Now;

            _context.ContactoMensajes.Add(contacto);
            _context.SaveChanges();

            TempData["Mensaje"] = "Mensaje enviado correctamente.";
            return RedirectToAction(nameof(Contacto));
        }
    }
}