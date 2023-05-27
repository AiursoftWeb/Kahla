using Aiursoft.Handler.Models;
using Aiursoft.Scanner;
using Aiursoft.XelNaga.Tools;
using Kahla.SDK.Models.ApiViewModels;
using Kahla.SDK.Services;
using Kahla.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using static Aiursoft.WebTools.Extends;

namespace Aiursoft.Archon.Tests
{
    [TestClass]
    public class BasicServerTests
    {
        private readonly string _endpointUrl;
        private readonly int _port;
        private IHost _server;
        private HttpClient _http;
        private ServiceCollection _services;
        private ServiceProvider _serviceProvider;

        public BasicServerTests()
        {
            _port = Network.GetAvailablePort();
            _endpointUrl = $"http://localhost:{_port}";
        }

        [TestInitialize]
        public async Task CreateServer()
        {
            _server = App<Startup>(port: _port);
            await _server.StartAsync();

            _http = new HttpClient();
            _services = new ServiceCollection();
            _services.AddHttpClient();
            _services.AddLibraryDependencies();
            _serviceProvider = _services.BuildServiceProvider();
            var kahlaLocation = _serviceProvider.GetRequiredService<KahlaLocation>();
            await kahlaLocation.UseKahlaServerAsync(_endpointUrl);
        }

        [TestCleanup]
        public async Task CleanServer()
        {
            if (_server != null)
            {
                await _server.StopAsync();
                _server.Dispose();
            }
        }

        [TestMethod]
        public async Task GetHome()
        {
            var response = await _http.GetAsync(_endpointUrl);
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            Assert.AreEqual("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());

            var content = await response.Content.ReadAsStringAsync();
            var contentObject = JsonConvert.DeserializeObject<IndexViewModel>(content);
            Assert.AreEqual(contentObject.Code, ErrorType.Success);
            Assert.AreEqual(ErrorType.Success, contentObject.Code);
        }

        [TestMethod]
        public async Task CallEmptySDK()
        {
            var home = _serviceProvider.GetRequiredService<HomeService>();
            var result = await home.IndexAsync(_endpointUrl);
            Assert.AreEqual(ErrorType.Success, result.Code);

        }

        [TestMethod]
        public async Task UnauthorizedCall()
        {
            var auth = _serviceProvider.GetRequiredService<AuthService>();
            var status = await auth.SignInStatusAsync();
            Assert.AreEqual(false, status.Value);

            try
            {
                await auth.MeAsync();
                Assert.Fail("Unauthorized call should not success.");
            }
            catch (WebException e)
            {
                Assert.IsTrue(e.Message.Contains("Unauthorized"));
            }
        }

        [TestMethod]
        public async Task OAuthTest()
        {
            var auth = _serviceProvider.GetRequiredService<AuthService>();
            var redirect = await auth.OAuthAsync();
            Assert.IsTrue(redirect.Contains(@"https://directory.aiursoft.com/oauth/authorize"));
        }
    }
}
