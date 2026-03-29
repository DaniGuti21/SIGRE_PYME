using Microsoft.AspNetCore.Mvc;
using SIGRE_PYME.Data;
using SIGRE_PYME.Filters;
using SIGRE_PYME.Models;
using System;

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
        public IActionResult Contacto(ContactoMensaje contacto)
        {
            if (string.IsNullOrWhiteSpace(contacto.NombreCompleto) ||
                string.IsNullOrWhiteSpace(contacto.Telefono) ||
                string.IsNullOrWhiteSpace(contacto.CorreoElectronico) ||
                string.IsNullOrWhiteSpace(contacto.Mensaje))
            {
                ViewBag.Error = "Debe completar todos los campos.";
                return View(contacto);
            }

            contacto.FechaEnvio = DateTime.Now;

            _context.ContactoMensajes.Add(contacto);
            _context.SaveChanges();

            ViewBag.Mensaje = "Mensaje enviado correctamente.";
            ModelState.Clear();

            return View(new ContactoMensaje());
        }
    }
}