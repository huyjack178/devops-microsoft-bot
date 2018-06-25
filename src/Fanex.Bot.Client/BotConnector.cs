namespace Fanex.Bot.Client
{
    using System.Text;
    using Fanex.Bot.Client.Configuration;
    using Fanex.Bot.Client.Models;
    using RestSharp;

    public class BotConnector : IBotConnector
    {
        private readonly IRestClient _webClient;

        public BotConnector() : this(null)
        {
        }

        protected internal BotConnector(IRestClient webClient)
        {
            _webClient = webClient ?? new RestClient { Encoding = Encoding.UTF8 };
        }

        public string Send(string message, string conversationId)
        {
            var token = GetToken();
            var result = ForwardToBot(message, conversationId, token);

            return result;
        }

        private string ForwardToBot(string message, string conversationId, Token token)
        {
            _webClient.BaseUrl = BotSettings.BotServiceUrl;

            var request = new RestRequest("/messages/forward", Method.POST);
            request.AddHeader("Authorization", $"Bearer {token.AccessToken}");
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddParameter("message", message);
            request.AddParameter("conversationId", conversationId);

            var result = _webClient.Execute(request);

            return result.Content;
        }

        private Token GetToken()
        {
            _webClient.BaseUrl = BotSettings.TokenUrl;

            var request = new RestRequest(Method.POST);
            request.AddParameter("grant_type", "client_credentials");
            request.AddParameter("client_id", BotSettings.ClientId);
            request.AddParameter("client_secret", BotSettings.ClientPassword);
            request.AddParameter("scope", $"{BotSettings.ClientId}/.default");

            request.AddHeader("Content-type", "application/x-www-form-urlencoded");

            var response = _webClient.Execute<Token>(request);

            return response.Data;
        }
    }
}