using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SIGRE_PYME.Models
{
    public class Pedido
    {
        [Key]
        public int PedidoId { get; set; }

        public int UsuarioId { get; set; }

        public DateTime Fecha { get; set; }

        public decimal Total { get; set; }

        public string Estado { get; set; }

        public Usuario Usuario { get; set; }

        public List<PedidoDetalle> PedidoDetalles { get; set; }
    }
}