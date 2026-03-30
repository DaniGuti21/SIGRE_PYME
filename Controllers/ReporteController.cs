using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SIGRE_PYME.Data;
using SIGRE_PYME.Filters;
using SIGRE_PYME.Models;
using System.Linq;

namespace SIGRE_PYME.Controllers
{
    [SesionActiva]
    [SoloAdmin]
    public class ReporteController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReporteController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard()
        {
            var dashboard = new DashboardViewModel();

            dashboard.TotalVentas = _context.Pedidos.Any() ? _context.Pedidos.Sum(p => p.Total) : 0;

            var productoMasVendido = _context.PedidoDetalles
                .Include(d => d.Producto)
                .GroupBy(d => new { d.ProductoId, d.Producto.Nombre })
                .Select(g => new
                {
                    Nombre = g.Key.Nombre,
                    Cantidad = g.Sum(x => x.Cantidad)
                })
                .OrderByDescending(x => x.Cantidad)
                .FirstOrDefault();

            if (productoMasVendido != null)
            {
                dashboard.ProductoMasVendido = productoMasVendido.Nombre;
                dashboard.CantidadVendida = productoMasVendido.Cantidad;
            }

            dashboard.ProductosStockBajo = _context.Productos
                .Where(p => p.StockActual <= 5)
                .ToList();

            return View(dashboard);
        }

        public IActionResult HistorialProducto(int? productoId)
        {
            ViewBag.Productos = new SelectList(_context.Productos.ToList(), "ProductoId", "Nombre", productoId);

            var movimientos = _context.MovimientosInventario
                .Include(m => m.Producto)
                .AsQueryable();

            if (productoId.HasValue)
            {
                movimientos = movimientos.Where(m => m.ProductoId == productoId.Value);
            }
            else
            {
                movimientos = movimientos.Where(m => false);
            }

            return View(movimientos.OrderByDescending(m => m.Fecha).ToList());
        }
    }
}