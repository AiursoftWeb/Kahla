using Aiursoft.AiurProtocol.Exceptions;
using Aiursoft.AiurProtocol.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Aiursoft.Kahla.Server.Attributes;

public class KahlaForceAuth : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        base.OnActionExecuting(context);
        if (context.Controller is not ControllerBase controller)
        {
            throw new InvalidOperationException();
        }

        if (!controller.User.Identity?.IsAuthenticated ?? false)
        {
            throw new AiurServerException(Code.Unauthorized, "You are unauthorized to access this API.");
        }
    }
}