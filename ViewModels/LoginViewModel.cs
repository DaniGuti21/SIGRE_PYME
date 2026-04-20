using System.ComponentModel.DataAnnotations;

namespace SIGRE_PYME.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Contrasena { get; set; } = string.Empty;
    }
}