using Microsoft.AspNetCore.Mvc;
using SIGRE_PYME.Data;
using SIGRE_PYME.Models;
using System.Linq;

namespace SIGRE_PYME.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsuarioController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🔐 INICIAR SESIÓN
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(Usuario user)
        {
            var usuario = _context.Usuarios
                .FirstOrDefault(u => u.Email == user.Email && u.Contrasena == user.Contrasena);

            if (usuario != null)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Correo o contraseña incorrectos";
            return View();
        }

        // 📝 REGISTRO
        public IActionResult Registro()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Registro(Usuario user)
        {
            _context.Usuarios.Add(user);
            _context.SaveChanges();

            return RedirectToAction("Login");
        }
    }
}