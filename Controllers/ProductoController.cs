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
            var listaProductos = _context.Productos.AsQueryable();

            if (!string.IsNullOrEmpty(categoria))
            {
                listaProductos = listaProductos.Where(p => p.Categoria == categoria);
            }

            if (precioMin.HasValue)
            {
                listaProductos = listaProductos.Where(p => p.Precio >= precioMin.Value);
            }

            if (precioMax.HasValue)
            {
                listaProductos = listaProductos.Where(p => p.Precio <= precioMax.Value);
            }

            ViewBag.Categoria = categoria;
            ViewBag.PrecioMin = precioMin;
            ViewBag.PrecioMax = precioMax;

            return View(listaProductos.ToList());
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
                    string carpeta = Path.Combine(_webHostEnvironment.WebRootPath, "img", "productos");

                    if (!Directory.Exists(carpeta))
                    {
                        Directory.CreateDirectory(carpeta);
                    }

                    string nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(imagenArchivo.FileName);
                    string ruta = Path.Combine(carpeta, nombreArchivo);

                    using (var stream = new FileStream(ruta, FileMode.Create))
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
            var productoBD = _context.Productos.Find(producto.ProductoId);

            if (productoBD == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                productoBD.SKU = producto.SKU;
                productoBD.Nombre = producto.Nombre;
                productoBD.Categoria = producto.Categoria;
                productoBD.Precio = producto.Precio;
                productoBD.StockActual = producto.StockActual;

                if (imagenArchivo != null && imagenArchivo.Length > 0)
                {
                    string carpeta = Path.Combine(_webHostEnvironment.WebRootPath, "img", "productos");

                    if (!Directory.Exists(carpeta))
                    {
                        Directory.CreateDirectory(carpeta);
                    }

                    string nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(imagenArchivo.FileName);
                    string ruta = Path.Combine(carpeta, nombreArchivo);

                    using (var stream = new FileStream(ruta, FileMode.Create))
                    {
                        imagenArchivo.CopyTo(stream);
                    }

                    productoBD.ImagenUrl = nombreArchivo;
                }

                _context.Productos.Update(productoBD);
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

            bool tieneMovimientos = _context.MovimientosInventario.Any(m => m.ProductoId == id);

            if (tieneMovimientos)
            {
                TempData["Error"] = "No se puede eliminar este producto porque tiene movimientos registrados.";
                return RedirectToAction("Index");
            }

            _context.Productos.Remove(producto);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}