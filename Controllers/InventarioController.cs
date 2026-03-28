using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SIGRE_PYME.Data;
using SIGRE_PYME.Models;
using SIGRE_PYME.Filters;
using System;
using System.Linq;

namespace SIGRE_PYME.Controllers
{
    [SesionActiva]
    [SoloAdmin]
    public class InventarioController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InventarioController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var movimientos = _context.MovimientosInventario.ToList();
            return View(movimientos);
        }
        public IActionResult Entrada()
        {
            ViewBag.Productos = new SelectList(_context.Productos.ToList(), "ProductoId", "Nombre");
            return View();
        }

        [HttpPost]
        public IActionResult Entrada(MovimientoInventario movimiento)
        {
            var producto = _context.Productos.Find(movimiento.ProductoId);

            if (producto == null)
            {
                ViewBag.Error = "El producto no existe";
                ViewBag.Productos = new SelectList(_context.Productos.ToList(), "ProductoId", "Nombre");
                return View(movimiento);
            }

            var usuarioId = HttpContext.Session.GetString("UsuarioId");

            if (usuarioId == null)
            {
                return RedirectToAction("Login", "Usuario");
            }

            producto.StockActual += movimiento.Cantidad;

            movimiento.TipoMovimiento = "Entrada";
            movimiento.Fecha = DateTime.Now;
            movimiento.UsuarioId = int.Parse(usuarioId);

            _context.MovimientosInventario.Add(movimiento);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Salida()
        {
            ViewBag.Productos = new SelectList(_context.Productos.ToList(), "ProductoId", "Nombre");
            return View();
        }

        [HttpPost]
        public IActionResult Salida(MovimientoInventario movimiento)
        {
            var producto = _context.Productos.Find(movimiento.ProductoId);

            if (producto == null)
            {
                ViewBag.Error = "El producto no existe";
                ViewBag.Productos = new SelectList(_context.Productos.ToList(), "ProductoId", "Nombre");
                return View(movimiento);
            }
            var usuarioId = HttpContext.Session.GetString("UsuarioId");

            if (usuarioId == null)
            {
                return RedirectToAction("Login", "Usuario");
            }

            if (producto.StockActual < movimiento.Cantidad)
            {
                ViewBag.Error = "No hay suficiente stock";
                ViewBag.Productos = new SelectList(_context.Productos.ToList(), "ProductoId", "Nombre");
                return View(movimiento);
            }

            producto.StockActual -= movimiento.Cantidad;

            movimiento.TipoMovimiento = "Salida";
            movimiento.Fecha = DateTime.Now;
            movimiento.UsuarioId = int.Parse(usuarioId);

            _context.MovimientosInventario.Add(movimiento);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}