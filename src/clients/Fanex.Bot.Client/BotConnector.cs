namespace Fanex.Bot.Client
{
    using System;
    using System.Net;
    using System.Text;
    using Fanex.Bot.Client.Configuration;
    using Fanex.Caching;
    using RestSharp;

    public class BotConnector : IBotConnector
    {
        private const string TokenCachedKey = "TokenCachedKey";
        private readonly IRestClient restClient;
        private readonly ICacheService cacheService;

        public BotConnector() : this(null, null)
        {
        }

        protected internal BotConnector(IRestClient restClient, ICacheService cacheService)
        {
            this.restClient = restClient ?? new RestClient { Encoding = Encoding.UTF8, Timeout = 10000 };
            this.cacheService = cacheService ?? new CacheService();
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

        internal int ForwardToBot(string message, string conversationId, string token)
        {
            restClient.BaseUrl = BotSettings.BotServiceUrl;

            var request = new RestRequest("/messages/forward", Method.POST);
            request.AddHeader("Authorization", $"Bearer {token}");
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddParameter("message", message);
            request.AddParameter("conversationId", conversationId);

            var response = restClient.Execute(request);
            TimeoutCheck(response);
            return (int)response.StatusCode;
        }

        internal string GetToken()
        {
            var token = cacheService.Get<string>(TokenCachedKey);

            if (string.IsNullOrEmpty(token))
            {
                restClient.BaseUrl = BotSettings.TokenUrl;

                var request = new RestRequest(Method.GET);
                request.AddQueryParameter("clientId", BotSettings.ClientId);
                request.AddQueryParameter("clientPassword", BotSettings.ClientPassword);
                var response = restClient.Execute(request);
                TimeoutCheck(response);
                token = response.Content.Replace("\"", string.Empty);

                cacheService.Set(
                    TokenCachedKey,
                    token,
                    new CacheItemOptions().SetAbsoluteExpiration(
                        TimeSpan.FromDays(1)));
            }

            return token;
        }

        private static void TimeoutCheck(IRestResponse response)
        {
            if (response.StatusCode == 0)
            {
                throw new TimeoutException("The request timed out!");
            }
        }
    }
}