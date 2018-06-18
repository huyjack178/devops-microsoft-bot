namespace Fanex.Bot.Utilitites
{
    using System;
    using System.Threading.Tasks;

    public interface IWebClient : IDisposable
    {
#pragma warning disable S3994 // URI Parameters should not be strings

        Task<T> GetAsync<T>(string url);

        Task<TOut> PostAsync<TIn, TOut>(string url, TIn content);

#pragma warning restore S3994 // URI Parameters should not be strings
    }
}