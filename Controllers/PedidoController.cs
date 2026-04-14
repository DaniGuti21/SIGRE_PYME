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

            return JsonSerializer.Deserialize<List<CarritoItem>>(carritoJson);
        }

        private void GuardarCarrito(List<CarritoItem> carrito)
        {
            var carritoJson = JsonSerializer.Serialize(carrito);
            HttpContext.Session.SetString("Carrito", carritoJson);
        }

        [HttpPost]
        public IActionResult AgregarAlCarrito(int productoId)
        {
            var producto = _context.Productos.Find(productoId);

            if (producto == null)
            {
                TempData["Error"] = "Producto no encontrado.";
                return RedirectToAction("Index", "Producto");
            }

            if (producto.StockActual <= 0)
            {
                TempData["Error"] = "No hay stock disponible para este producto.";
                return RedirectToAction("Index", "Producto");
            }

            var carrito = ObtenerCarrito();
            var item = carrito.FirstOrDefault(x => x.ProductoId == productoId);

            if (item != null)
            {
                if (item.Cantidad < producto.StockActual)
                {
                    item.Cantidad = item.Cantidad + 1;
                }
            }
            else
            {
                CarritoItem nuevo = new CarritoItem();
                nuevo.ProductoId = producto.ProductoId;
                nuevo.Nombre = producto.Nombre;
                nuevo.ImagenUrl = producto.ImagenUrl;
                nuevo.Precio = producto.Precio;
                nuevo.Cantidad = 1;

                carrito.Add(nuevo);
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
            var item = carrito.FirstOrDefault(x => x.ProductoId == productoId);
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
            var item = carrito.FirstOrDefault(x => x.ProductoId == productoId);

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
                var producto = _context.Productos.Find(item.ProductoId);

                if (producto == null)
                {
                    TempData["Error"] = "Uno de los productos no existe.";
                    return RedirectToAction("Carrito");
                }

                if (producto.StockActual < item.Cantidad)
                {
                    TempData["Error"] = "No hay suficiente stock para " + producto.Nombre;
                    return RedirectToAction("Carrito");
                }
            }

            try
            {
                decimal total = 0;

                foreach (var item in carrito)
                {
                    total = total + (item.Precio * item.Cantidad);
                }

                Pedido pedido = new Pedido();
                pedido.UsuarioId = usuarioId;
                pedido.Fecha = DateTime.Now;
                pedido.Total = total;
                pedido.Estado = "Confirmado";

                _context.Pedidos.Add(pedido);
                _context.SaveChanges();

                StringBuilder factura = new StringBuilder();
                factura.AppendLine("FACTURA");
                factura.AppendLine("Pedido: " + pedido.PedidoId);
                factura.AppendLine("Fecha: " + pedido.Fecha);
                factura.AppendLine("--------------------------------");

                foreach (var item in carrito)
                {
                    var producto = _context.Productos.Find(item.ProductoId);

                    PedidoDetalle detalle = new PedidoDetalle();
                    detalle.PedidoId = pedido.PedidoId;
                    detalle.ProductoId = item.ProductoId;
                    detalle.Cantidad = item.Cantidad;
                    detalle.PrecioUnitario = item.Precio;
                    detalle.Subtotal = item.Precio * item.Cantidad;

                    _context.PedidoDetalles.Add(detalle);

                    producto.StockActual = producto.StockActual - item.Cantidad;

                    MovimientoInventario movimiento = new MovimientoInventario();
                    movimiento.ProductoId = item.ProductoId;
                    movimiento.UsuarioId = usuarioId;
                    movimiento.TipoMovimiento = "Salida";
                    movimiento.Cantidad = item.Cantidad;
                    movimiento.Fecha = DateTime.Now;

                    _context.MovimientosInventario.Add(movimiento);

                    factura.AppendLine("Producto: " + item.Nombre);
                    factura.AppendLine("Cantidad: " + item.Cantidad);
                    factura.AppendLine("Precio: " + item.Precio.ToString("0.00"));
                    factura.AppendLine("Subtotal: " + detalle.Subtotal.ToString("0.00"));
                    factura.AppendLine("--------------------------------");
                }

                _context.SaveChanges();

                factura.AppendLine("Total: " + total.ToString("0.00"));
                factura.AppendLine("Gracias por su compra.");

                HttpContext.Session.SetString("FacturaTexto", factura.ToString());
                HttpContext.Session.SetString("FacturaNombre", "Factura_Pedido_" + pedido.PedidoId + ".txt");

                HttpContext.Session.Remove("Carrito");

                return RedirectToAction("CompraExitosa");
            }
            catch
            {
                TempData["Error"] = "Ocurrió un error al confirmar el pedido.";
                return RedirectToAction("Carrito");
            }
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

            byte[] bytes = Encoding.UTF8.GetBytes(facturaTexto);
            return File(bytes, "text/plain", facturaNombre);
        }
    }
}