namespace Fanex.Bot.Core.Utilities.Web
{
    using System;
    using System.Net;
    using System.Threading.Tasks;

    public interface IWebClient
    {
        Task<T> GetJsonAsync<T>(Uri url);

        Task<TOut> PostJsonAsync<TIn, TOut>(Uri url, TIn data);

        Task<HttpStatusCode> PostJsonAsync<T>(Uri url, T data);
    }
}