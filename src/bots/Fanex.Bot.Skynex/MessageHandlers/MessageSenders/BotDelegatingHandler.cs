namespace Fanex.Bot.Skynex.MessageHandlers.MessageSenders
{
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    public class BotDelegatingHandler : DelegatingHandler
    {
        private readonly string _token;

        public BotDelegatingHandler(string token)
        {
            _token = token;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
            return base.SendAsync(request, cancellationToken);
        }
    }
}