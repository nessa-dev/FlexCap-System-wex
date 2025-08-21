// Dentro de Controllers/Base/BaseController.cs

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FlexCap.Web.Controllers
{
    public abstract class BaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Obtém o nome da Action atual
            var actionName = context.RouteData.Values["action"]?.ToString();

            // Determina o perfil com base na Action
            if (actionName == "Rh")
            {
                ViewData["Profile"] = "Rh";
            }
            else if (actionName == "Manager")
            {
                ViewData["Profile"] = "Manager";
            }
            else if (actionName == "Colaborador")
            {
                ViewData["Profile"] = "Colaborador";
            }

            base.OnActionExecuting(context);
        }
    }
}