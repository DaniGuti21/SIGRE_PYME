using Microsoft.AspNetCore.Mvc;
using SIGRE_PYME.Data;
using SIGRE_PYME.Models;
using SIGRE_PYME.Filters;
using System.Linq;

namespace SIGRE_PYME.Controllers
{
    [SesionActiva]
    public class ProductoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductoController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var productos = _context.Productos.ToList();
            return View(productos);
        }

        [SoloAdmin]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [SoloAdmin]
        public IActionResult Create(Producto producto)
        {
            if (ModelState.IsValid)
            {
                _context.Productos.Add(producto);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(producto);
        }

        [SoloAdmin]
        public IActionResult Edit(int id)
        {
            var producto = _context.Productos.Find(id);

            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }

        [HttpPost]
        [SoloAdmin]
        public IActionResult Edit(Producto producto)
        {
            if (ModelState.IsValid)
            {
                _context.Productos.Update(producto);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(producto);
        }

        [SoloAdmin]
        public IActionResult Delete(int id)
        {
            var producto = _context.Productos.Find(id);

            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }

        [HttpPost, ActionName("Delete")]
        [SoloAdmin]
        public IActionResult DeleteConfirmed(int id)
        {
            var producto = _context.Productos.Find(id);

            if (producto != null)
            {
                _context.Productos.Remove(producto);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public JsonResult Buscar(string texto)
        {
            if (string.IsNullOrEmpty(texto))
            {
                var todos = _context.Productos
                    .Select(p => new
                    {
                        p.ProductoId,
                        p.SKU,
                        p.Nombre,
                        p.Precio,
                        p.StockActual
                    })
                    .ToList();

                return Json(todos);
            }

            var productos = _context.Productos
                .Where(p => p.Nombre.Contains(texto) || p.SKU.Contains(texto))
                .Select(p => new
                {
                    p.ProductoId,
                    p.SKU,
                    p.Nombre,
                    p.Precio,
                    p.StockActual
                })
                .ToList();

            return Json(productos);
        }
    }
}