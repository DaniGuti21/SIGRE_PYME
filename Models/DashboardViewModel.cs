using System.Collections.Generic;

namespace SIGRE_PYME.Models
{
    public class DashboardViewModel
    {
        public decimal TotalVentas { get; set; }
        public string ProductoMasVendido { get; set; } = "";
        public int CantidadVendida { get; set; }
        public List<Producto> ProductosStockBajo { get; set; } = new List<Producto>();
    }
}