using System.Net;
using System.Xml.Serialization;
using Aiursoft.CSTools.Tools;
using Aiursoft.Scanner.Abstractions;

namespace Kahla.SDK.Services
{
    public class PersistentHttpClient : ISingletonDependency
    {
        private readonly HttpClient _client;
        private readonly CookieContainer _cookieContainer;

        public PersistentHttpClient()
        {
            _cookieContainer = Load();
            _client = new HttpClient(new HttpClientHandler()
            {
                AllowAutoRedirect = false,
                CookieContainer = _cookieContainer,
                UseCookies = true
            });
        }

        public async Task<string> Track(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>())
            };
            request.Headers.Add("accept", "application/json");
            var response = await _client.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.Redirect)
            {
                Save(_cookieContainer);
                return response.Headers.Location?.OriginalString;
            }
            else
            {
                throw new WebException(response.ReasonPhrase);
            }
        }

        public async Task<string> PostWithFile(string url, Stream fileStream, string fileName)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new MultipartFormDataContent
                {
                    { new StreamContent(fileStream), "file", fileName }
                }
            };


            request.Headers.Add("accept", "application/json");

            var response = await _client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                throw new WebException($"The remote server returned unexpected status code: {response.StatusCode} - {response.ReasonPhrase}.");
            }
        }

        private static void Save(CookieContainer cookieContainer)
        {
            using MemoryStream stream = new MemoryStream();
            new XmlSerializer(typeof(CookieContainer)).Serialize(stream, cookieContainer);
            var bytes = new byte[stream.Length];
            stream.Position = 0;
            _ = stream.Read(bytes, 0, bytes.Length);
            var base64 = bytes.BytesToBase64();
            File.WriteAllText("cookies.dat", base64);
        }

        private static CookieContainer Load()
        {
            try
            {
                var base64 = File.ReadAllText("cookies.dat");
                var bytes = base64.Base64ToBytes();
                using MemoryStream stream = new MemoryStream(bytes);
                return (CookieContainer)new XmlSerializer(typeof(CookieContainer)).Deserialize(stream);
            }
            catch
            {
                return new CookieContainer();
            }
        }
    }
}
