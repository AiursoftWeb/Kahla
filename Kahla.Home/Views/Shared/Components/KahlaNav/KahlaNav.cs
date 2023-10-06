using Microsoft.AspNetCore.Mvc;

namespace Kahla.Home.Views.Shared.Components.KahlaNav
{
    public class KahlaNav : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var model = new KahlaNavViewModel();
            return View(model);
        }
    }
}
