namespace Aiursoft.Kahla.Server.Middlewares;

public class AngularMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        await next(context);
        if (context.Response.StatusCode == 404 && context.Request.Method == "GET")
        {
            var origPath = context.Request.Path;
            context.Request.Path = "/index.html";
            await next(context);
            context.Request.Path = origPath; // For correct logging and middleware compatibility
        }
    }
}
