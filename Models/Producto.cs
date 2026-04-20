using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIGRE_PYME.Models
{
    public class Producto
    {
        [Key]
        public int ProductoId { get; set; }

        [Required(ErrorMessage = "El SKU es obligatorio.")]
        [StringLength(50)]
        public string SKU { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(120)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La categoría es obligatoria.")]
        [StringLength(60)]
        public string Categoria { get; set; } = string.Empty;

        [Required(ErrorMessage = "El precio es obligatorio.")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, 999999999, ErrorMessage = "El precio debe ser mayor a 0.")]
        public decimal Precio { get; set; }

        [Required(ErrorMessage = "El stock es obligatorio.")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
        public int StockActual { get; set; }

        [StringLength(255)]
        public string ImagenUrl { get; set; } = string.Empty;
    }
}