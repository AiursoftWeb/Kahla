using Aiursoft.Scanner;
using Aiursoft.XelNaga.Tools;
using AngleSharp.Html.Dom;
using Kahla.Home;
using Kahla.SDK.Services;
using Kahla.Tests.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using static Aiursoft.WebTools.Extends;

namespace Kahla.Tests
{
    [TestClass]
    public class BasicHomeTests
    {
        private readonly string _endpointUrl;
        private readonly int _port;
        private IHost _server;
        private HttpClient _http;

        public BasicHomeTests()
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
            Assert.AreEqual("text/html; charset=utf-8", response.Content.Headers.ContentType.ToString());
            var doc = await HtmlHelpers.GetDocumentAsync(response);
            var p = (IHtmlElement)doc.QuerySelector("p.col-md-8");
            Assert.AreEqual("As a cross-platform business messaging app, Kahla can encrypt communications at any location, on any device. And it is completely open source and free.", p.InnerHtml.Trim());
        }
    }
}
