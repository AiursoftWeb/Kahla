using Aiursoft.Pylon.Interfaces;
using Aiursoft.Pylon.Models;
using Aiursoft.Pylon.Services;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace Kahla.SDK.Services
{
    public class SingletonHTTP : ISingletonDependency
    {
        private readonly HttpClient _client;
        private readonly CookieContainer _cookieContainer;

        public SingletonHTTP()
        {
            _cookieContainer = Load();
            _client = new HttpClient(new HttpClientHandler()
            {
                AllowAutoRedirect = false,
                CookieContainer = _cookieContainer,
                UseCookies = true
            });
        }

        public async Task<string> Track(AiurUrl url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url.ToString())
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>())
            };
            request.Headers.Add("accept", "application/json");
            var response = await _client.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.Redirect)
            {
                Save(_cookieContainer);
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
                Save(_cookieContainer);
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
                Save(_cookieContainer);
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                throw new WebException($"The remote server returned unexpcted status code: {response.StatusCode} - {response.ReasonPhrase}.");
            }
        }

        public void Save(CookieContainer cookieContainer)
        {
            using MemoryStream stream = new MemoryStream();
            new BinaryFormatter().Serialize(stream, cookieContainer);
            var bytes = new byte[stream.Length];
            stream.Position = 0;
            stream.Read(bytes, 0, bytes.Length);
            var base64 = bytes.BytesToBase64();
            File.WriteAllText("cookies.dat", base64);
        }

        public CookieContainer Load()
        {
            try
            {
                var base64 = File.ReadAllText("cookies.dat");
                var bytes = base64.Base64ToBytes();
                using MemoryStream stream = new MemoryStream(bytes);
                return (CookieContainer)new BinaryFormatter().Deserialize(stream);
            }
            catch
            {
                return new CookieContainer();
            }
        }
    }
}
