using Aiursoft.XelNaga.Interfaces;
using Kahla.Bot.Services.BingModels;
using Newtonsoft.Json;
using RestSharp;
using System.Collections.Generic;

namespace Kahla.Bot.Services
{
    public class BingTranslator : IScopedDependency
    {
        private static string _apiKey;
        private readonly BotLogger _logger;

        public BingTranslator(
            BotLogger logger)
        {
            _logger = logger;
        }

        public void Init(string apiKey)
        {
            _apiKey = apiKey;
        }

        private string CallTranslateAPI(string inputJson, string targetLanguage)
        {
            var apiAddress = $"https://api.cognitive.microsofttranslator.com/translate?api-version=3.0&to={targetLanguage}";
            var client = new RestClient(apiAddress);
            var request = new RestRequest(Method.POST);
            request
                .AddHeader("Ocp-Apim-Subscription-Key", _apiKey)
                .AddHeader("Content-Type", "application/json")
                .AddParameter("undefined", inputJson, ParameterType.RequestBody);

            var json = client.Execute(request).Content;
            return json;
        }

        public string CallTranslate(string input, string targetLanguage)
        {
            var inputSource = new List<Translation>
            {
                new Translation { Text = input }
            };
            var bingResponse = CallTranslateAPI(JsonConvert.SerializeObject(inputSource), targetLanguage);
            var result = JsonConvert.DeserializeObject<List<BingResponse>>(bingResponse);
            _logger.LogInfo($"Called Bing translate API.");
            return result[0].Translations[0].Text;
        }
    }
}
