namespace Fanex.Bot.Common.Helpers.Web
{
    using System;
    using System.Net;
    using System.Threading.Tasks;

    public interface IWebClient
    {
        Task<T> GetJsonAsync<T>(Uri url);

        Task<string> GetContentAsync(Uri url);

        Task<TOut> PostJsonAsync<TIn, TOut>(Uri url, TIn data);

        Task<HttpStatusCode> PostJsonAsync<T>(Uri url, T data);
    }
}