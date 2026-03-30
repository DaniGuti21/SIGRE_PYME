using System.ComponentModel.DataAnnotations;

namespace SIGRE_PYME.Models
{
    public class Cliente
    {
        [Key]
        public int ClienteId { get; set; }

        [Required]
        public string Nombre { get; set; } = "";

        [Required]
        public string Email { get; set; } = "";

        [Required]
        public string Telefono { get; set; } = "";
    }
}