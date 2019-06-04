using Aiursoft.Pylon.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kahla.Server.Models.ApiViewModels
{
    public class MineViewModel : AiurProtocol
    {
        public List<KahlaUser> Users { get; internal set; }
        public List<SearchedGroup> Groups { get; internal set; }
    }
}
