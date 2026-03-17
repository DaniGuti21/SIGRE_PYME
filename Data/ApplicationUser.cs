using Microsoft.AspNetCore.Identity;

namespace SIGRE_PYME.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Nombre { get; set; } = "";
    }
}