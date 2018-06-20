namespace Fanex.Bot.Utilitites
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    public interface IWebClient : IDisposable
    {
#pragma warning disable S3994 // URI Parameters should not be strings
        HttpClient Client { get; }

        Task<T> GetAsync<T>(string url);

        Task<TOut> PostAsync<TOut>(string url, HttpContent content);

        Task<string> SendAsync(HttpRequestMessage request);

        Task<TOut> SendAsync<TOut>(HttpRequestMessage request);

#pragma warning restore S3994 // URI Parameters should not be strings
    }
}