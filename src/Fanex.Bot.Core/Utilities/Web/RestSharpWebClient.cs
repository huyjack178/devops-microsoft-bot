namespace Fanex.Bot.Core.Utilities.Web
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using RestSharp;

    public class WebClient : IWebClient
    {
        private readonly IRestClient _restClient;

        public WebClient(IRestClient restClient)
        {
            _restClient = restClient;
        }

        public async Task<T> GetJsonAsync<T>(Uri url)
        {
            _restClient.BaseUrl = url;
            var request = new RestRequest(Method.GET);

            var response = await _restClient.ExecuteTaskAsync(request);

            return JsonConvert.DeserializeObject<T>(response.Content);
        }

        public async Task<TOut> PostJsonAsync<TIn, TOut>(Uri url, TIn data)
        {
            var response = await ExecuteAsync(url, data);

            return JsonConvert.DeserializeObject<TOut>(response.Content);
        }

        public async Task<HttpStatusCode> PostJsonAsync<T>(Uri url, T data)
        {
            var response = await ExecuteAsync(url, data);

            return response.StatusCode;
        }

        private async Task<IRestResponse> ExecuteAsync<T>(Uri url, T data)
        {
            _restClient.BaseUrl = url;
            var jsonData = JsonConvert.SerializeObject(data);
            var request = new RestRequest(Method.POST);
            request.AddParameter("application/json", jsonData, ParameterType.RequestBody);

            var response = await _restClient.ExecuteTaskAsync(request);
            return response;
        }
    }
}