using Microsoft.AspNetCore.Mvc;
using SIGRE_PYME.Data;
using SIGRE_PYME.Models;
using SIGRE_PYME.Filters;
using System.Linq;

namespace SIGRE_PYME.Controllers
{
    [SesionActiva]
    [SoloAdmin]
    public class ClienteController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClienteController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var clientes = _context.Clientes.ToList();
            return View(clientes);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Cliente cliente)
        {
            if (ModelState.IsValid)
            {
                _context.Clientes.Add(cliente);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(cliente);
        }
    }
}