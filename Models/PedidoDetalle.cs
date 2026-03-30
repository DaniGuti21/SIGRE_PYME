using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIGRE_PYME.Models
{
    [Table("pedidodetalles")]
    public class PedidoDetalle
    {
        [Key]
        public int DetalleId { get; set; }

        public int PedidoId { get; set; }
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecioUnitario { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Subtotal { get; set; }

        public Pedido Pedido { get; set; }
        public Producto Producto { get; set; }
    }
}