using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIGRE_PYME.Models
{
    [Table("pedidos")]
    public class Pedido
    {
        [Key]
        public int PedidoId { get; set; }

        public int UsuarioId { get; set; }

        public DateTime Fecha { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Total { get; set; }

        public string Estado { get; set; } = "";

        public Usuario Usuario { get; set; }
        public List<PedidoDetalle> PedidoDetalles { get; set; } = new List<PedidoDetalle>();
    }
}