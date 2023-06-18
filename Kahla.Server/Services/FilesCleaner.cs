using Aiursoft.Probe.SDK.Services.ToProbeServer;
using Aiursoft.Scanner.Abstract;
using Aiursoft.XelNaga.Tools;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Aiursoft.Directory.SDK.Services;

namespace Kahla.Server.Services
{
    public class FilesCleaner : IHostedService, IDisposable, ISingletonDependency
    {
        private readonly ILogger _logger;
        private Timer _timer;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly DirectoryAppTokenService _appsContainer;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public FilesCleaner(
            ILogger<FilesCleaner> logger,
            IServiceScopeFactory scopeFactory,
            DirectoryAppTokenService appsContainer,
            IConfiguration configuration,
            IWebHostEnvironment env)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _appsContainer = appsContainer;
            _configuration = configuration;
            _env = env;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (_env.IsDevelopment() || !EntryExtends.IsProgramEntry())
            {
                _logger.LogInformation("Skip cleaner in development environment");
                return Task.CompletedTask;
            }
            _logger.LogInformation("Timed Background Service is starting");
            _timer = new Timer(DoWork, null, TimeSpan.FromSeconds(5), TimeSpan.FromMinutes(10));
            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            try
            {
                _logger.LogInformation("Cleaner task started!");
                using var scope = _scopeFactory.CreateScope();
                var foldersService = scope.ServiceProvider.GetRequiredService<FoldersService>();
                await AllClean(foldersService);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred");
            }
        }

        public async Task AllClean(FoldersService foldersService)
        {
            try
            {
                var deadline = DateTime.UtcNow - TimeSpan.FromDays(100);
                var publicSite = _configuration["UserFilesSiteName"];
                var accessToken = await _appsContainer.GetAccessTokenAsync();
                var rootFolders = await foldersService.ViewContentAsync(accessToken, publicSite, string.Empty);
                foreach (var conversation in rootFolders.Value.SubFolders)
                {
                    var folders = await foldersService.ViewContentAsync(accessToken, publicSite, conversation.FolderName);
                    foreach (var folder in folders.Value.SubFolders)
                    {
                        try
                        {
                            var parts = folder.FolderName.Split('-');
                            var time = new DateTime(
                                Convert.ToInt32(parts[0]),
                                Convert.ToInt32(parts[1]),
                                Convert.ToInt32(parts[2]));
                            if (time < deadline)
                            {
                                await foldersService.DeleteFolderAsync(accessToken, publicSite, $"{conversation.FolderName}/{folder.FolderName}");
                            }
                        }
                        catch
                        {
                            await foldersService.DeleteFolderAsync(accessToken, publicSite, $"{conversation.FolderName}/{folder.FolderName}");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Failed to delete empty folders");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed Background Service is stopping");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
