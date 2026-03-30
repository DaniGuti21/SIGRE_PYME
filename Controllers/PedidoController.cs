using Microsoft.AspNetCore.Mvc;
using SIGRE_PYME.Data;
using SIGRE_PYME.Filters;
using SIGRE_PYME.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace SIGRE_PYME.Controllers
{
    [SesionActiva]
    public class PedidoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PedidoController(ApplicationDbContext context)
        {
            _context = context;
        }

        private List<CarritoItem> ObtenerCarrito()
        {
            var carritoJson = HttpContext.Session.GetString("Carrito");

            if (string.IsNullOrEmpty(carritoJson))
            {
                return new List<CarritoItem>();
            }

            return JsonSerializer.Deserialize<List<CarritoItem>>(carritoJson) ?? new List<CarritoItem>();
        }

        private void GuardarCarrito(List<CarritoItem> carrito)
        {
            HttpContext.Session.SetString("Carrito", JsonSerializer.Serialize(carrito));
        }

        [HttpPost]
        public IActionResult AgregarAlCarrito(int productoId)
        {
            var producto = _context.Productos.Find(productoId);

            if (producto == null)
            {
                return RedirectToAction("Index", "Producto");
            }

            if (producto.StockActual <= 0)
            {
                TempData["Error"] = "Ese producto no tiene stock disponible.";
                return RedirectToAction("Index", "Producto");
            }

            var carrito = ObtenerCarrito();
            var itemExistente = carrito.FirstOrDefault(c => c.ProductoId == productoId);

            if (itemExistente != null)
            {
                if (itemExistente.Cantidad < producto.StockActual)
                {
                    itemExistente.Cantidad++;
                }
            }
            else
            {
                carrito.Add(new CarritoItem
                {
                    ProductoId = producto.ProductoId,
                    Nombre = producto.Nombre,
                    ImagenUrl = producto.ImagenUrl,
                    Precio = producto.Precio,
                    Cantidad = 1
                });
            }

            GuardarCarrito(carrito);

            return RedirectToAction("Carrito");
        }

        public IActionResult Carrito()
        {
            var carrito = ObtenerCarrito();
            return View(carrito);
        }

        [HttpPost]
        public IActionResult ActualizarCantidad(int productoId, int cantidad)
        {
            var carrito = ObtenerCarrito();
            var item = carrito.FirstOrDefault(c => c.ProductoId == productoId);
            var producto = _context.Productos.Find(productoId);

            if (item != null && producto != null)
            {
                if (cantidad < 1)
                {
                    cantidad = 1;
                }

                if (cantidad > producto.StockActual)
                {
                    cantidad = producto.StockActual;
                }

                item.Cantidad = cantidad;
            }

            GuardarCarrito(carrito);
            return RedirectToAction("Carrito");
        }

        [HttpPost]
        public IActionResult QuitarDelCarrito(int productoId)
        {
            var carrito = ObtenerCarrito();
            var item = carrito.FirstOrDefault(c => c.ProductoId == productoId);

            if (item != null)
            {
                carrito.Remove(item);
            }

            GuardarCarrito(carrito);
            return RedirectToAction("Carrito");
        }

        [HttpPost]
        public IActionResult ConfirmarPedido()
        {
            var carrito = ObtenerCarrito();

            if (carrito.Count == 0)
            {
                TempData["Error"] = "El carrito está vacío.";
                return RedirectToAction("Carrito");
            }

            var usuarioIdTexto = HttpContext.Session.GetString("UsuarioId");

            if (string.IsNullOrEmpty(usuarioIdTexto))
            {
                return RedirectToAction("Login", "Usuario");
            }

            int usuarioId = int.Parse(usuarioIdTexto);

            foreach (var item in carrito)
            {
                var productoValidacion = _context.Productos.Find(item.ProductoId);

                if (productoValidacion == null)
                {
                    TempData["Error"] = "Uno de los productos ya no existe.";
                    return RedirectToAction("Carrito");
                }

                if (productoValidacion.StockActual < item.Cantidad)
                {
                    TempData["Error"] = "No hay suficiente stock para " + productoValidacion.Nombre;
                    return RedirectToAction("Carrito");
                }
            }

            decimal total = carrito.Sum(x => x.Precio * x.Cantidad);

            var pedido = new Pedido
            {
                UsuarioId = usuarioId,
                Fecha = DateTime.Now,
                Total = total,
                Estado = "Confirmado"
            };

            _context.Pedidos.Add(pedido);
            _context.SaveChanges();

            var contenido = new StringBuilder();
            contenido.AppendLine("FACTURA SIGRE_PYME");
            contenido.AppendLine("====================================");
            contenido.AppendLine("Pedido #: " + pedido.PedidoId);
            contenido.AppendLine("Fecha: " + pedido.Fecha);
            contenido.AppendLine("Usuario ID: " + usuarioId);
            contenido.AppendLine("====================================");
            contenido.AppendLine("");

            foreach (var item in carrito)
            {
                var producto = _context.Productos.Find(item.ProductoId);

                var detalle = new PedidoDetalle
                {
                    PedidoId = pedido.PedidoId,
                    ProductoId = item.ProductoId,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = item.Precio,
                    Subtotal = item.Precio * item.Cantidad
                };

                _context.PedidoDetalles.Add(detalle);

                producto.StockActual -= item.Cantidad;

                var movimiento = new MovimientoInventario
                {
                    ProductoId = item.ProductoId,
                    UsuarioId = usuarioId,
                    TipoMovimiento = "Salida",
                    Cantidad = item.Cantidad,
                    Fecha = DateTime.Now
                };

                _context.MovimientosInventario.Add(movimiento);

                contenido.AppendLine("Producto: " + item.Nombre);
                contenido.AppendLine("Cantidad: " + item.Cantidad);
                contenido.AppendLine("Precio unitario: " + item.Precio.ToString("0.00"));
                contenido.AppendLine("Subtotal: " + (item.Precio * item.Cantidad).ToString("0.00"));
                contenido.AppendLine("------------------------------------");
            }

            _context.SaveChanges();

            contenido.AppendLine("TOTAL: " + total.ToString("0.00"));
            contenido.AppendLine("====================================");
            contenido.AppendLine("Gracias por su compra.");

            HttpContext.Session.SetString("FacturaTexto", contenido.ToString());
            HttpContext.Session.SetString("FacturaNombre", "Factura_Pedido_" + pedido.PedidoId + ".txt");

            HttpContext.Session.Remove("Carrito");

            return RedirectToAction("CompraExitosa");
        }

        public IActionResult CompraExitosa()
        {
            return View();
        }

        public IActionResult VerFactura()
        {
            var facturaTexto = HttpContext.Session.GetString("FacturaTexto");

            if (string.IsNullOrEmpty(facturaTexto))
            {
                return RedirectToAction("Carrito");
            }

            ViewBag.Factura = facturaTexto;
            return View();
        }

        public IActionResult DescargarFactura()
        {
            var facturaTexto = HttpContext.Session.GetString("FacturaTexto");
            var facturaNombre = HttpContext.Session.GetString("FacturaNombre");

            if (string.IsNullOrEmpty(facturaTexto) || string.IsNullOrEmpty(facturaNombre))
            {
                return RedirectToAction("Carrito");
            }

            var bytes = Encoding.UTF8.GetBytes(facturaTexto);
            return File(bytes, "text/plain", facturaNombre);
        }
    }
}