using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SIGRE_PYME.Filters
{
    public class SesionActivaAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var usuarioId = context.HttpContext.Session.GetString("UsuarioId");

            if (string.IsNullOrEmpty(usuarioId))
            {
                context.Result = new RedirectToActionResult("Login", "Usuario", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}