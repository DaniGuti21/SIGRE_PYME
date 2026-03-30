using System;
using System.ComponentModel.DataAnnotations;

namespace SIGRE_PYME.Models
{
    public class MovimientoInventario
    {
        [Key]
        public int MovimientoId { get; set; }

        public int ProductoId { get; set; }
        public int UsuarioId { get; set; }
        public string TipoMovimiento { get; set; }
        public int Cantidad { get; set; }
        public DateTime Fecha { get; set; }

        public Producto Producto { get; set; }
        public Usuario Usuario { get; set; }
    }
}