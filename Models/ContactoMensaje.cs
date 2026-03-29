using System;
using System.ComponentModel.DataAnnotations;

namespace SIGRE_PYME.Models
{
    public class ContactoMensaje
    {
        [Key]
        public int ContactoMensajeId { get; set; }

        [Required]
        public string NombreCompleto { get; set; } = "";

        [Required]
        public string Telefono { get; set; } = "";

        [Required]
        public string CorreoElectronico { get; set; } = "";

        [Required]
        public string Mensaje { get; set; } = "";

        public DateTime FechaEnvio { get; set; }
    }
}