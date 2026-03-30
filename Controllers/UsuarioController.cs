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

        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("UsuarioId") != null)
            {
                return RedirectToAction("Index", "Producto");
            }

            return View();
        }

        [HttpPost]
        public IActionResult Login(Usuario user)
        {
            var usuario = _context.Usuarios
                .FirstOrDefault(u => u.NombreUsuario == user.NombreUsuario && u.Contrasena == user.Contrasena);

            if (usuario != null)
            {
                HttpContext.Session.SetString("UsuarioId", usuario.Id.ToString());
                HttpContext.Session.SetString("NombreUsuario", usuario.NombreUsuario);

                if (usuario.NombreUsuario.Trim().ToLower() == "admin")
                {
                    HttpContext.Session.SetString("EsAdmin", "true");
                }
                else
                {
                    HttpContext.Session.SetString("EsAdmin", "false");
                }

                return RedirectToAction("Index", "Producto");
            }

            ViewBag.Error = "Usuario o contraseña incorrectos";
            return View();
        }

        public IActionResult Registro()
        {
            if (HttpContext.Session.GetString("UsuarioId") != null)
            {
                return RedirectToAction("Index", "Producto");
            }

            return View();
        }

        [HttpPost]
        public IActionResult Registro(Usuario user)
        {
            if (string.IsNullOrEmpty(user.NombreUsuario) || string.IsNullOrEmpty(user.Contrasena))
            {
                ViewBag.Error = "Debe completar todos los campos";
                return View(user);
            }

            var existeUsuario = _context.Usuarios.Any(u => u.NombreUsuario == user.NombreUsuario);

            if (existeUsuario)
            {
                ViewBag.Error = "Ese nombre de usuario ya existe";
                return View(user);
            }

            _context.Usuarios.Add(user);
            _context.SaveChanges();

            return RedirectToAction("Login");
        }

        public IActionResult CerrarSesion()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}