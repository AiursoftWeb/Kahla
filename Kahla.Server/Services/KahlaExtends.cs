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

        public static bool IsVideo(this string filename)
        {
            var AvaliableExtensions = new string[] { "mp4", "webm", "ogg" };
            var ext = Path.GetExtension(filename);
            foreach (var extension in AvaliableExtensions)
            {
                if (ext.Trim('.').ToLower() == extension)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
