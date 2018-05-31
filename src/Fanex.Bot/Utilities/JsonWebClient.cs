namespace Fanex.Bot.Utilitites
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    public class JsonWebClient : IWebClient
    {
        private const string MimeType = "application/json";
        private readonly HttpClient _client = new HttpClient();

        public JsonWebClient(Uri baseAddress)
        {
            _client.BaseAddress = baseAddress;
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MimeType));
        }

#pragma warning disable S3994 // URI Parameters should not be strings
#pragma warning disable S4005 // "System.Uri" arguments should be used instead of strings

        public async Task<HttpResponseMessage> GetAsync(string url)
        {
            CheckArgument(url);

            return await _client.GetAsync(url);
        }

        private static void CheckArgument(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException(nameof(url));
            }
        }

        public async Task<TOut> PostAsync<TIn, TOut>(string url, TIn content)
        {
            var httpContent = new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, MimeType);
            var response = await _client.PostAsync(url, httpContent);

            return JsonConvert.DeserializeObject<TOut>(response.Content.ReadAsStringAsync().Result);
        }

#pragma warning restore S4005 // "System.Uri" arguments should be used instead of strings
#pragma warning restore S3994 // URI Parameters should not be strings

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
}