using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kahla.Home.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult Code404()
        {
            return View();
        }
        public IActionResult ServerException()
        {
            return View();
        }
    }
}
