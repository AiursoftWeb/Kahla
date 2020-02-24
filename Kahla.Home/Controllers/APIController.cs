using Aiursoft.Handler.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Kahla.Home.Controllers
{
    [LimitPerMin(40)]
    [APIExpHandler]
    [APIModelStateChecker]
    public class APIController : Controller
    {

    }
}
