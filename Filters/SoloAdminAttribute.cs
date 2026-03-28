using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SIGRE_PYME.Filters
{
    public class SoloAdminAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var esAdmin = context.HttpContext.Session.GetString("EsAdmin");

            if (esAdmin != "true")
            {
                context.Result = new ContentResult
                {
                    Content = "Acceso denegado. Solo el administrador puede realizar esta acción."
                };
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}