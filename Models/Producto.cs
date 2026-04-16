using System.ComponentModel.DataAnnotations;

namespace SIGRE_PYME.Models
{
    public class Producto
    {
        public int ProductoId { get; set; }

        public string SKU { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Categoria { get; set; } = "";
        public decimal Precio { get; set; }
        public int StockActual { get; set; }
        public string ImagenUrl { get; set; } = "";
    }
}