namespace Fanex.Bot.Utilitites
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    public interface IWebClient : IDisposable
    {
        Task<HttpResponseMessage> GetAsync(string url);

        Task<TOut> PostAsync<TIn, TOut>(string url, TIn content);
    }
}