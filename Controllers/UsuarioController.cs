using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SIGRE_PYME.Data;
using SIGRE_PYME.Helpers;
using SIGRE_PYME.Models;
using SIGRE_PYME.ViewModels;

namespace SIGRE_PYME.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<Usuario> _passwordHasher;

        public UsuarioController(ApplicationDbContext context, IPasswordHasher<Usuario> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("UsuarioId") != null)
            {
                return RedirectToAction("Index", "Producto");
            }

            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Debe completar usuario y contraseña.";
                return View(model);
            }

            var usuario = _context.Usuarios.FirstOrDefault(u => u.NombreUsuario == model.NombreUsuario);

            if (usuario == null)
            {
                ViewBag.Error = "Usuario o contraseña incorrectos.";
                return View(model);
            }

            if (usuario.Bloqueado)
            {
                ViewBag.Error = "El usuario está bloqueado por intentos fallidos. Contacte al administrador.";
                return View(model);
            }

            var resultado = _passwordHasher.VerifyHashedPassword(usuario, usuario.Contrasena, model.Contrasena);

            if (resultado == PasswordVerificationResult.Failed)
            {
                usuario.IntentosFallidos++;

                if (usuario.IntentosFallidos >= 3)
                {
                    usuario.Bloqueado = true;
                }

                _context.SaveChanges();

                ViewBag.Error = usuario.Bloqueado
                    ? "Usuario bloqueado por exceder 3 intentos fallidos."
                    : $"Usuario o contraseña incorrectos. Intentos: {usuario.IntentosFallidos}/3";

                return View(model);
            }

            usuario.IntentosFallidos = 0;
            _context.SaveChanges();

            var nombreRol = RolesSistema.ObtenerNombre(usuario.RolId);

            HttpContext.Session.SetString("UsuarioId", usuario.Id.ToString());
            HttpContext.Session.SetString("NombreUsuario", usuario.NombreUsuario);
            HttpContext.Session.SetString("Rol", nombreRol);
            HttpContext.Session.SetString("EsAdmin", (usuario.RolId == RolesSistema.AdminId).ToString().ToLower());

            return RedirectToAction("Index", "Producto");
        }

        [HttpGet]
        public IActionResult Registro()
        {
            if (HttpContext.Session.GetString("UsuarioId") != null)
            {
                return RedirectToAction("Index", "Producto");
            }

            CargarRoles();
            return View(new RegistroUsuarioViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Registro(RegistroUsuarioViewModel model)
        {
            CargarRoles();

            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Revise los datos del formulario.";
                return View(model);
            }

            if (!RolesSistema.EsValido(model.RolId))
            {
                ViewBag.Error = "Rol inválido.";
                return View(model);
            }

            var existeUsuario = _context.Usuarios.Any(u => u.NombreUsuario == model.NombreUsuario);
            if (existeUsuario)
            {
                ViewBag.Error = "Ese nombre de usuario ya existe.";
                return View(model);
            }

            var nuevoUsuario = new Usuario
            {
                NombreUsuario = model.NombreUsuario.Trim(),
                RolId = model.RolId,
                IntentosFallidos = 0,
                Bloqueado = false
            };

            nuevoUsuario.Contrasena = _passwordHasher.HashPassword(nuevoUsuario, model.Contrasena);

            _context.Usuarios.Add(nuevoUsuario);
            _context.SaveChanges();

            TempData["Mensaje"] = "Usuario registrado correctamente.";
            return RedirectToAction("Login");
        }

        public IActionResult CerrarSesion()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        public IActionResult AccesoDenegado()
        {
            return View();
        }

        private void CargarRoles()
        {
            ViewBag.Roles = RolesSistema.Roles
                .Select(r => new SelectListItem
                {
                    Value = r.Key.ToString(),
                    Text = r.Value
                })
                .ToList();
        }
    }
}