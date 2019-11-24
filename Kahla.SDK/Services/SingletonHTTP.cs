using Aiursoft.Pylon.Interfaces;
using Aiursoft.Pylon.Models;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Kahla.SDK.Services
{
    public class SingletonHTTP : ISingletonDependency
    {
        private readonly HttpClient _client;
        private readonly Regex _regex;

        public SingletonHTTP(
            IHttpClientFactory clientFactory)
        {
            _regex = new Regex("^https://", RegexOptions.Compiled);
            _client = clientFactory.CreateClient();
        }

        public async Task<string> Track(AiurUrl url)
        {
            var tempClient = new HttpClient(new HttpClientHandler()
            {
                AllowAutoRedirect = false
            });
            var request = new HttpRequestMessage(HttpMethod.Get, url.ToString())
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>())
            };
            request.Headers.Add("accept", "application/json");
            var response = await tempClient.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.Redirect)
            {
                return response.Headers.Location.OriginalString;
            }
            else
            {
                throw new WebException(response.ReasonPhrase);
            }
        }

        public async Task<string> Get(AiurUrl url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url.ToString())
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>())
            };

            request.Headers.Add("accept", "application/json");

            var response = await _client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                throw new WebException($"The remote server returned unexpcted status code: {response.StatusCode} - {response.ReasonPhrase}.");
            }
        }

        public async Task<string> Post(AiurUrl url, AiurUrl postDataStr)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url.Address)
            {
                Content = new FormUrlEncodedContent(postDataStr.Params)
            };
            request.Headers.Add("accept", "application/json");
            var response = await _client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                throw new WebException($"The remote server returned unexpcted status code: {response.StatusCode} - {response.ReasonPhrase}.");
            }
        }
    }
}
