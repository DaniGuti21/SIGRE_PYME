using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIGRE_PYME.Models
{
    [Table("Usuarios")]
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Column("Username")]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        [Column("Password")]
        public string Contrasena { get; set; } = string.Empty;

        public int IntentosFallidos { get; set; } = 0;

        public bool Bloqueado { get; set; } = false;

        [Column("RolId")]
        public int RolId { get; set; }
    }
}