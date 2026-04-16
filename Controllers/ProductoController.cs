using Microsoft.AspNetCore.Mvc;
using SIGRE_PYME.Data;
using SIGRE_PYME.Models;
using SIGRE_PYME.Filters;
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
            try
            {
                if (string.IsNullOrWhiteSpace(producto.SKU) ||
                    string.IsNullOrWhiteSpace(producto.Nombre) ||
                    string.IsNullOrWhiteSpace(producto.Categoria))
                {
                    ViewBag.Error = "Debe completar todos los campos obligatorios.";
                    return View(producto);
                }

                if (_context.Productos.Any(p => p.SKU == producto.SKU))
                {
                    ViewBag.Error = "Ya existe un producto con ese SKU.";
                    return View(producto);
                }

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

                TempData["Mensaje"] = "Producto creado correctamente.";
                return RedirectToAction("Index");
            }
            catch
            {
                ViewBag.Error = "Ocurrió un error al guardar el producto.";
                return View(producto);
            }
        }

        [SoloAdmin]
        public IActionResult Edit(int id)
        {
            var producto = _context.Productos.FirstOrDefault(p => p.ProductoId == id);

            if (producto == null)
            {
                TempData["Error"] = "El producto no fue encontrado.";
                return RedirectToAction("Index");
            }

            return View(producto);
        }

        [HttpPost]
        [SoloAdmin]
        public IActionResult Edit(Producto producto, IFormFile? imagenArchivo, string? ImagenActual)
        {
            try
            {
                var productoBD = _context.Productos.FirstOrDefault(p => p.ProductoId == producto.ProductoId);

                if (productoBD == null)
                {
                    TempData["Error"] = "El producto no existe.";
                    return RedirectToAction("Index");
                }

                if (string.IsNullOrWhiteSpace(producto.SKU) ||
                    string.IsNullOrWhiteSpace(producto.Nombre) ||
                    string.IsNullOrWhiteSpace(producto.Categoria))
                {
                    ViewBag.Error = "Debe completar todos los campos obligatorios.";
                    producto.ImagenUrl = ImagenActual ?? "";
                    return View(producto);
                }

                bool skuRepetido = _context.Productos.Any(p => p.SKU == producto.SKU && p.ProductoId != producto.ProductoId);

                if (skuRepetido)
                {
                    ViewBag.Error = "Ya existe otro producto con ese SKU.";
                    producto.ImagenUrl = ImagenActual ?? "";
                    return View(producto);
                }

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
                else
                {
                    productoBD.ImagenUrl = ImagenActual ?? productoBD.ImagenUrl;
                }

                _context.SaveChanges();

                TempData["Mensaje"] = "Producto actualizado correctamente.";
                return RedirectToAction("Index");
            }
            catch
            {
                TempData["Error"] = "Ocurrió un error al actualizar el producto.";
                return RedirectToAction("Index");
            }
        }

        [SoloAdmin]
        public IActionResult Delete(int id)
        {
            var producto = _context.Productos.FirstOrDefault(p => p.ProductoId == id);

            if (producto == null)
            {
                TempData["Error"] = "Producto no encontrado.";
                return RedirectToAction("Index");
            }

            return View(producto);
        }

        [HttpPost]
        [SoloAdmin]
        public IActionResult DeleteConfirmed(int ProductoId)
        {
            var producto = _context.Productos.FirstOrDefault(p => p.ProductoId == ProductoId);

            if (producto == null)
            {
                TempData["Error"] = "Producto no existe.";
                return RedirectToAction("Index");
            }

            bool tieneMovimientos = _context.MovimientosInventario.Any(m => m.ProductoId == ProductoId);
            bool tieneVentas = _context.PedidoDetalles.Any(d => d.ProductoId == ProductoId);

            if (tieneMovimientos || tieneVentas)
            {
                TempData["Error"] = "No se puede eliminar este producto porque tiene movimientos o ventas.";
                return RedirectToAction("Index");
            }

            _context.Productos.Remove(producto);
            _context.SaveChanges();

            TempData["Mensaje"] = "Producto eliminado correctamente.";
            return RedirectToAction("Index");
        }
    }
}