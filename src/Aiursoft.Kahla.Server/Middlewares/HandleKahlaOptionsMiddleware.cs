
namespace Aiursoft.Kahla.Server.Middlewares
{
    public class HandleKahlaOptionsMiddleware(
        RequestDelegate next)
    {

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Method == "OPTIONS")
            {
                context.Response.StatusCode = 204;
                return;
            }
            await next.Invoke(context);
        }
    }
}
