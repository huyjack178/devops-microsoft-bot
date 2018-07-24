namespace Fanex.Bot.Client
{
    using System;
    using System.Net;
    using System.Text;
    using Fanex.Bot.Client.Configuration;
    using Fanex.Bot.Client.Models;
    using Fanex.Caching;
    using RestSharp;

    public class BotConnector : IBotConnector
    {
        private const string TokenCachedKey = "TokenCachedKey";
        private readonly IRestClient _webClient;
        private readonly ICacheService _cacheService;

        public BotConnector() : this(null, null)
        {
        }

        protected internal BotConnector(IRestClient webClient, ICacheService cacheService)
        {
            _webClient = webClient ?? new RestClient { Encoding = Encoding.UTF8 };
            _cacheService = new CacheService();
        }

        public string Send(string message, string conversationId)
        {
            var token = GetToken();

            var result = ForwardToBot(message, conversationId, token);

            if (result == (int)HttpStatusCode.Unauthorized)
            {
                token = GetToken();
                result = ForwardToBot(message, conversationId, token);
            }

            return result.ToString();
        }

        private int ForwardToBot(string message, string conversationId, string token)
        {
            _webClient.BaseUrl = BotSettings.BotServiceUrl;

            var request = new RestRequest("/messages/forward", Method.POST);
            request.AddHeader("Authorization", $"Bearer {token}");
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddParameter("message", message);
            request.AddParameter("conversationId", conversationId);

            var result = _webClient.Execute(request);

            return (int)result.StatusCode;
        }

        private string GetToken()
        {
            var token = _cacheService.Get<string>(TokenCachedKey);

            if (string.IsNullOrEmpty(token))
            {
                _webClient.BaseUrl = BotSettings.TokenUrl;

                var request = new RestRequest(Method.POST);
                request.AddParameter("grant_type", "client_credentials");
                request.AddParameter("client_id", BotSettings.ClientId);
                request.AddParameter("client_secret", BotSettings.ClientPassword);
                request.AddParameter("scope", $"{BotSettings.ClientId}/.default");
                request.AddHeader("Content-type", "application/x-www-form-urlencoded");
                var response = _webClient.Execute<Token>(request);

                token = response.Data.AccessToken;

                _cacheService.Set(
                    TokenCachedKey,
                    token,
                    new CacheItemOptions().SetAbsoluteExpiration(
                        TimeSpan.FromSeconds(response.Data.ExpiresIn)));
            }

            return token;
        }
    }
}