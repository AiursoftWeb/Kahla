using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.IO;

namespace Kahla.Server.Services
{
    public static class KahlaExtends
    {
        public static JsonResult AiurJson(this Controller controller, object obj)
        {
            return controller.Json(obj, new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                }
            });
        }
    }
}
