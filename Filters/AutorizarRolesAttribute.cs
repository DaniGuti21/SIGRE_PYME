using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SIGRE_PYME.Filters
{
    public class AutorizarRolesAttribute : ActionFilterAttribute
    {
        private readonly string[] _rolesPermitidos;

        public AutorizarRolesAttribute(params string[] rolesPermitidos)
        {
            _rolesPermitidos = rolesPermitidos ?? [];
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var usuarioId = context.HttpContext.Session.GetString("UsuarioId");
            var rol = context.HttpContext.Session.GetString("Rol");

            if (string.IsNullOrWhiteSpace(usuarioId))
            {
                context.Result = new RedirectToActionResult("Login", "Usuario", null);
                return;
            }

            if (_rolesPermitidos.Length > 0 && !_rolesPermitidos.Contains(rol, StringComparer.OrdinalIgnoreCase))
            {
                context.Result = new RedirectToActionResult("AccesoDenegado", "Usuario", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}