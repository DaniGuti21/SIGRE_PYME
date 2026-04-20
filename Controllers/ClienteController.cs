using Microsoft.AspNetCore.Mvc;
using SIGRE_PYME.Data;
using SIGRE_PYME.Filters;
using SIGRE_PYME.Helpers;
using SIGRE_PYME.Models;

namespace SIGRE_PYME.Controllers
{
    [SesionActiva]
    [AutorizarRoles(RolesSistema.Admin, RolesSistema.Gerente, RolesSistema.Vendedor)]
    public class ClienteController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClienteController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View(_context.Clientes.OrderBy(c => c.Nombre).ToList());
        }

        public IActionResult Create()
        {
            return View(new Cliente());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Cliente cliente)
        {
            if (!ModelState.IsValid)
            {
                return View(cliente);
            }

            _context.Clientes.Add(cliente);
            _context.SaveChanges();
            TempData["Mensaje"] = "Cliente registrado correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}