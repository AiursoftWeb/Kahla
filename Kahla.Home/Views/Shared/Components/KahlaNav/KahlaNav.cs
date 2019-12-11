using Microsoft.AspNetCore.Mvc;

namespace Kahla.Home.Views.Shared.Components.KahlaNav
{
    public class KahlaNav : ViewComponent
    {
        public IViewComponentResult Invoke(bool isProduction)
        {
            var model = new KahlaNavViewModel
            {
                IsProduction = isProduction
            };
            return View(model);
        }
    }
}
