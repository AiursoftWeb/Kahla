using Microsoft.Extensions.DependencyInjection;

namespace Kahla.SDK.Abstract
{
    public interface IStartUp
    {
        void ConfigureServices(IServiceCollection services);
        void Configure();
    }
}
