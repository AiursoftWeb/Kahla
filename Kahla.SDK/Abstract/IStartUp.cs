using Kahla.SDK.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kahla.SDK.Abstract
{
    public interface IStartUp
    {
        void ConfigureServices(IServiceCollection services);
        void Configure(SettingsService settings);
    }
}
