namespace Fanex.Bot.Client
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Fanex.Bot.Client.Models;
    using Fanex.Bot.Utilitites;

    public class BotConnector
    {
        private readonly BotSettings _botConfig;
        private readonly IWebClient _webClient;

        public BotConnector(
            BotSettings botConfig = null,
            IWebClient webClient = null)
        {
            _botConfig = botConfig ?? BotSettings.Settings;
            _webClient = webClient ?? new WebClient();
        }

        public async Task<string> SendAsync(string message, string conversationId)
        {
            var token = await GetTokenAsync();
            var result = await ForwardToBot(message, conversationId, token);

            return result;
        }

        private async Task<string> ForwardToBot(string message, string conversationId, Token token)
        {
            var requestData = new List<KeyValuePair<string, string>> {
                new KeyValuePair<string, string>("message", message),
                new KeyValuePair<string, string>("conversationId", conversationId),
            };

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_botConfig.BotServiceUrl}/messages/forward");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
            request.Content = new FormUrlEncodedContent(requestData);

            var result = await _webClient.SendAsync(request);
            return result;
        }

        private async Task<Token> GetTokenAsync()
        {
            var requestData = new List<KeyValuePair<string, string>> {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", _botConfig.ClientId),
                new KeyValuePair<string, string>("client_secret", _botConfig.ClientPassword),
                new KeyValuePair<string, string>("scope", $"{_botConfig.ClientId}/.default")
            };
            var request = new HttpRequestMessage(HttpMethod.Post, _botConfig.TokenUrl);
            request.Content = new FormUrlEncodedContent(requestData);

            var token = await _webClient.SendAsync<Token>(request);

            return token;
        }
    }
}