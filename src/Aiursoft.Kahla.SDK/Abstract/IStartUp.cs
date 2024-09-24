using Microsoft.Extensions.DependencyInjection;

namespace Aiursoft.Kahla.SDK.Abstract
{
    public interface IStartUp
    {
        void ConfigureServices(IServiceCollection services);
        void Configure();
    }
}
