using Microsoft.AspNetCore.Mvc;
using SIGRE_PYME.Data;
using SIGRE_PYME.Filters;
using SIGRE_PYME.Helpers;
using SIGRE_PYME.Models;

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

        public IActionResult Index(string? categoria, decimal? precioMin, decimal? precioMax, string? texto)
        {
            var productos = ConstruirConsulta(categoria, precioMin, precioMax, texto)
                .OrderBy(p => p.Nombre)
                .ToList();

            ViewBag.Categoria = categoria;
            ViewBag.PrecioMin = precioMin;
            ViewBag.PrecioMax = precioMax;
            ViewBag.Texto = texto;

            return View(productos);
        }

        [HttpGet]
        public IActionResult BuscarAjax(string? texto, string? categoria, decimal? precioMin, decimal? precioMax)
        {
            var productos = ConstruirConsulta(categoria, precioMin, precioMax, texto)
                .OrderBy(p => p.Nombre)
                .Select(p => new
                {
                    p.ProductoId,
                    p.SKU,
                    p.Nombre,
                    p.Categoria,
                    Precio = p.Precio.ToString("0.00"),
                    p.StockActual,
                    p.ImagenUrl
                })
                .ToList();

            var rol = HttpContext.Session.GetString("Rol") ?? string.Empty;
            var puedeEditar = rol == RolesSistema.Admin || rol == RolesSistema.Gerente || rol == RolesSistema.Vendedor;
            var puedeComprar = rol == RolesSistema.Cliente;

            return Json(new
            {
                productos,
                puedeEditar,
                puedeComprar
            });
        }

        [AutorizarRoles(RolesSistema.Admin, RolesSistema.Gerente, RolesSistema.Vendedor)]
        public IActionResult Create()
        {
            return View(new Producto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AutorizarRoles(RolesSistema.Admin, RolesSistema.Gerente, RolesSistema.Vendedor)]
        public IActionResult Create(Producto producto, IFormFile? imagenArchivo)
        {
            if (_context.Productos.Any(p => p.SKU == producto.SKU))
            {
                ModelState.AddModelError(nameof(producto.SKU), "Ya existe un producto con ese SKU.");
            }

            if (!ModelState.IsValid)
            {
                return View(producto);
            }

            producto.SKU = producto.SKU.Trim();
            producto.Nombre = producto.Nombre.Trim();
            producto.Categoria = producto.Categoria.Trim();
            producto.ImagenUrl = GuardarImagen(imagenArchivo, producto.ImagenUrl);

            _context.Productos.Add(producto);
            _context.SaveChanges();

            TempData["Mensaje"] = "Producto creado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [AutorizarRoles(RolesSistema.Admin, RolesSistema.Gerente, RolesSistema.Vendedor)]
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
        [ValidateAntiForgeryToken]
        [AutorizarRoles(RolesSistema.Admin, RolesSistema.Gerente, RolesSistema.Vendedor)]
        public IActionResult Edit(Producto producto, IFormFile? imagenArchivo)
        {
            var productoBD = _context.Productos.Find(producto.ProductoId);
            if (productoBD == null)
            {
                return NotFound();
            }

            if (_context.Productos.Any(p => p.ProductoId != producto.ProductoId && p.SKU == producto.SKU))
            {
                ModelState.AddModelError(nameof(producto.SKU), "Ya existe otro producto con ese SKU.");
            }

            if (!ModelState.IsValid)
            {
                return View(producto);
            }

            productoBD.SKU = producto.SKU.Trim();
            productoBD.Nombre = producto.Nombre.Trim();
            productoBD.Categoria = producto.Categoria.Trim();
            productoBD.Precio = producto.Precio;
            productoBD.StockActual = producto.StockActual;
            productoBD.ImagenUrl = GuardarImagen(imagenArchivo, productoBD.ImagenUrl);

            _context.Productos.Update(productoBD);
            _context.SaveChanges();

            TempData["Mensaje"] = "Producto actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [AutorizarRoles(RolesSistema.Admin, RolesSistema.Gerente)]
        public IActionResult Delete(int id)
        {
            var producto = _context.Productos.Find(id);
            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AutorizarRoles(RolesSistema.Admin)]
        public IActionResult DeleteConfirmed(int productoId)
        {
            var producto = _context.Productos.Find(productoId);
            if (producto == null)
            {
                TempData["Error"] = "El producto ya no existe.";
                return RedirectToAction(nameof(Index));
            }

            var tieneMovimientos = _context.MovimientosInventario.Any(m => m.ProductoId == productoId);
            var tienePedidos = _context.PedidoDetalles.Any(d => d.ProductoId == productoId);

            if (tieneMovimientos || tienePedidos)
            {
                TempData["Error"] = "No se puede eliminar porque el producto tiene movimientos o ventas asociadas.";
                return RedirectToAction(nameof(Index));
            }

            _context.Productos.Remove(producto);
            _context.SaveChanges();

            TempData["Mensaje"] = "Producto eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        private IQueryable<Producto> ConstruirConsulta(string? categoria, decimal? precioMin, decimal? precioMax, string? texto)
        {
            var query = _context.Productos.AsQueryable();

            if (!string.IsNullOrWhiteSpace(categoria))
            {
                query = query.Where(p => p.Categoria == categoria);
            }

            if (precioMin.HasValue)
            {
                query = query.Where(p => p.Precio >= precioMin.Value);
            }

            if (precioMax.HasValue)
            {
                query = query.Where(p => p.Precio <= precioMax.Value);
            }

            if (!string.IsNullOrWhiteSpace(texto))
            {
                texto = texto.Trim();
                query = query.Where(p => p.Nombre.Contains(texto) || p.SKU.Contains(texto) || p.Categoria.Contains(texto));
            }

            return query;
        }

        private string GuardarImagen(IFormFile? imagenArchivo, string? imagenActual)
        {
            if (imagenArchivo == null || imagenArchivo.Length == 0)
            {
                return imagenActual ?? string.Empty;
            }

            var carpeta = Path.Combine(_webHostEnvironment.WebRootPath, "img", "productos");
            if (!Directory.Exists(carpeta))
            {
                Directory.CreateDirectory(carpeta);
            }

            var extension = Path.GetExtension(imagenArchivo.FileName).ToLowerInvariant();
            var permitidas = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            if (!permitidas.Contains(extension))
            {
                return imagenActual ?? string.Empty;
            }

            var nombreArchivo = $"{Guid.NewGuid()}{extension}";
            var ruta = Path.Combine(carpeta, nombreArchivo);

            using var stream = new FileStream(ruta, FileMode.Create);
            imagenArchivo.CopyTo(stream);

            return nombreArchivo;
        }
    }
}