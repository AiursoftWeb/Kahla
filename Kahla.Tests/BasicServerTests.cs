using Aiursoft.Handler.Exceptions;
using Aiursoft.Handler.Models;
using Aiursoft.Scanner;
using Kahla.SDK.Abstract;
using Kahla.SDK.Models.ApiViewModels;
using Kahla.SDK.Services;
using Kahla.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using static Aiursoft.WebTools.Extends;

namespace Aiursoft.Archon.Tests
{
    [TestClass]
    public class BasicServerTests
    {
        private readonly string _endpointUrl = $"http://localhost:{_port}";
        private const int _port = 15999;
        private IHost _server;
        private HttpClient _http;
        private ServiceCollection _services;
        private ServiceProvider _serviceProvider;

        [TestInitialize]
        public async Task CreateServer()
        {
            _server = App<Startup>(port: _port);
            _http = new HttpClient();
            _services = new ServiceCollection();
            _services.AddHttpClient();
            await _server.StartAsync();
            _services.AddHttpClient();
            _services.AddLibraryDependencies();
            _serviceProvider = _services.BuildServiceProvider();
        }

        [TestCleanup]
        public async Task CleanServer()
        {
            await _server.StopAsync();
            _server.Dispose();
        }

        [TestMethod]
        public async Task GetHome()
        {
            var response = await _http.GetAsync(_endpointUrl);
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            Assert.AreEqual("application/json; charset=utf-8", response.Content.Headers.ContentType.ToString());

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
    }
}
