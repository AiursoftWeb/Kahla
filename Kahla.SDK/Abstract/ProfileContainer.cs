using Aiursoft.Scanner.Interfaces;
using Kahla.SDK.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kahla.SDK.Abstract
{
    public class ProfileContainer<T>  where T : BotBase
    {
        public KahlaUser Profile { get; set; }
    }
}
