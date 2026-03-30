using Microsoft.AspNetCore.Mvc;
using SIGRE_PYME.Data;
using SIGRE_PYME.Models;
using SIGRE_PYME.Filters;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;

namespace SIGRE_PYME.Controllers
{
    [SesionActiva]
    public class ProductoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductoController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index(string categoria, decimal? precioMin, decimal? precioMax)
        {
            var productos = _context.Productos.AsQueryable();

            if (!string.IsNullOrEmpty(categoria))
            {
                productos = productos.Where(p => p.Categoria == categoria);
            }

            if (precioMin.HasValue)
            {
                productos = productos.Where(p => p.Precio >= precioMin.Value);
            }

            if (precioMax.HasValue)
            {
                productos = productos.Where(p => p.Precio <= precioMax.Value);
            }

            ViewBag.Categoria = categoria;
            ViewBag.PrecioMin = precioMin;
            ViewBag.PrecioMax = precioMax;

            return View(productos.ToList());
        }

        [SoloAdmin]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [SoloAdmin]
        public IActionResult Create(Producto producto, IFormFile imagenArchivo)
        {
            if (ModelState.IsValid)
            {
                if (imagenArchivo != null && imagenArchivo.Length > 0)
                {
                    string carpetaDestino = Path.Combine(_webHostEnvironment.WebRootPath, "img", "productos");

                    if (!Directory.Exists(carpetaDestino))
                    {
                        Directory.CreateDirectory(carpetaDestino);
                    }

                    string nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(imagenArchivo.FileName);
                    string rutaCompleta = Path.Combine(carpetaDestino, nombreArchivo);

                    using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                    {
                        imagenArchivo.CopyTo(stream);
                    }

                    producto.ImagenUrl = nombreArchivo;
                }
                else
                {
                    producto.ImagenUrl = "";
                }

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
        public IActionResult Edit(Producto producto, IFormFile imagenArchivo)
        {
            var productoExistente = _context.Productos.Find(producto.ProductoId);

            if (productoExistente == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                productoExistente.SKU = producto.SKU;
                productoExistente.Nombre = producto.Nombre;
                productoExistente.Categoria = producto.Categoria;
                productoExistente.Precio = producto.Precio;
                productoExistente.StockActual = producto.StockActual;

                if (imagenArchivo != null && imagenArchivo.Length > 0)
                {
                    string carpetaDestino = Path.Combine(_webHostEnvironment.WebRootPath, "img", "productos");

                    if (!Directory.Exists(carpetaDestino))
                    {
                        Directory.CreateDirectory(carpetaDestino);
                    }

                    string nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(imagenArchivo.FileName);
                    string rutaCompleta = Path.Combine(carpetaDestino, nombreArchivo);

                    using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                    {
                        imagenArchivo.CopyTo(stream);
                    }

                    productoExistente.ImagenUrl = nombreArchivo;
                }

                _context.Update(productoExistente);
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

            if (producto == null)
            {
                return RedirectToAction("Index");
            }

            if (_context.MovimientosInventario.Any(m => m.ProductoId == id))
            {
                TempData["Error"] = "No se puede eliminar este producto porque tiene movimientos de inventario asociados.";
                return RedirectToAction("Index");
            }

            _context.Productos.Remove(producto);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}