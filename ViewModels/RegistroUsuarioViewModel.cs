using System.ComponentModel.DataAnnotations;
using SIGRE_PYME.Helpers;

namespace SIGRE_PYME.ViewModels
{
    public class RegistroUsuarioViewModel
    {
        [Required]
        [StringLength(100)]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        public string Contrasena { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Contrasena), ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmarContrasena { get; set; } = string.Empty;

        [Required]
        [Range(1, 5, ErrorMessage = "Debe seleccionar un rol válido.")]
        public int RolId { get; set; } = RolesSistema.ClienteId;
    }
}