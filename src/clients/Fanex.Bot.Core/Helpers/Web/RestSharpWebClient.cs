namespace Fanex.Bot.Core.Utilities.Web
{
    using System;
    using System.Net;
    using System.Net.Cache;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using RestSharp;

    public class RestSharpWebClient : IWebClient
    {
        private readonly IRestClient _restClient;

        public RestSharpWebClient(IRestClient restClient)
        {
            _restClient = restClient;
            _restClient.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
            _restClient.AddDefaultHeader("Cache-Control", "no-cache");
        }

#pragma warning disable S3216 // "ConfigureAwait(false)" should be used

        public async Task<T> GetJsonAsync<T>(Uri url)
        {
            var response = await ExecuteGetAsync(url);

            return JsonConvert.DeserializeObject<T>(response.Content);
        }

        public async Task<string> GetContentAsync(Uri url)
        {
            var response = await ExecuteGetAsync(url);

            return response.Content;
        }

        public async Task<TOut> PostJsonAsync<TIn, TOut>(Uri url, TIn data)
        {
            var response = await ExecutePostAsync(url, data);

            return JsonConvert.DeserializeObject<TOut>(response.Content);
        }

        public async Task<HttpStatusCode> PostJsonAsync<T>(Uri url, T data)
        {
            var response = await ExecutePostAsync(url, data);

            return response.StatusCode;
        }

        private async Task<IRestResponse> ExecuteGetAsync(Uri url)
        {
            _restClient.BaseUrl = url;
            var request = new RestRequest(Method.GET);

            var response = await _restClient.ExecuteTaskAsync(request);
            TimeoutCheck(request, response);

            return response;
        }

        private async Task<IRestResponse> ExecutePostAsync<T>(Uri url, T data)
        {
            _restClient.BaseUrl = url;
            var jsonData = JsonConvert.SerializeObject(data);
            var request = new RestRequest(Method.POST);
            request.AddParameter("application/json", jsonData, ParameterType.RequestBody);

            var response = await _restClient.ExecuteTaskAsync(request);
            TimeoutCheck(request, response);

            return response;
        }

        private static void TimeoutCheck(IRestRequest request, IRestResponse response)
        {
            if (response.StatusCode == 0)
            {
                throw new TimeoutException($"The request timed out! {request.Parameters[0].Value}");
            }
        }

#pragma warning restore S3216 // "ConfigureAwait(false)" should be used
    }
}