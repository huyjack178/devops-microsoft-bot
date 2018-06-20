namespace Fanex.Bot.Utilitites
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

#pragma warning disable S3994 // URI Parameters should not be strings
#pragma warning disable S4005 // "System.Uri" arguments should be used instead of strings

    public class WebClient : IWebClient
    {
        private readonly HttpClient _client = new HttpClient();

        public HttpClient Client => _client;

        public async Task<T> GetAsync<T>(string url)
        {
            CheckArgument(url);

            var response = await _client.GetAsync(url);

            return JsonConvert.DeserializeObject<T>(response.Content.ReadAsStringAsync().Result);
        }

        private static void CheckArgument(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException(nameof(url));
            }
        }

        public async Task<TOut> PostAsync<TOut>(
            string url,
            HttpContent content)
        {
            var response = await _client.PostAsync(url, content);

            return JsonConvert.DeserializeObject<TOut>(response.Content.ReadAsStringAsync().Result);
        }

        public async Task<TOut> SendAsync<TOut>(HttpRequestMessage request)
        {
            var response = _client.SendAsync(request).Result;
            var result = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<TOut>(result);
        }

        public async Task<string> SendAsync(HttpRequestMessage request)
        {
            var response = _client.SendAsync(request).Result;
            var result = await response.Content.ReadAsStringAsync();

            return result;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_client != null)
            {
                _client.Dispose();
            }
        }
    }

#pragma warning restore S4005 // "System.Uri" arguments should be used instead of strings
#pragma warning restore S3994 // URI Parameters should not be strings
}