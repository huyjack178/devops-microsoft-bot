using System;
using System.Net;
using Fanex.Bot.Client.Configuration;
using Fanex.Bot.Client.Models;
using Fanex.Caching;
using NSubstitute;
using RestSharp;
using Xunit;

namespace Fanex.Bot.Client.Tests
{
    public class BotConnectorTests
    {
        private const string TokenCachedKey = "TokenCachedKey";
        private readonly IRestClient _restClient;
        private readonly ICacheService _cacheService;

        public BotConnectorTests()
        {
            _restClient = Substitute.For<IRestClient>();
            _cacheService = new CacheService();
            BotClientManager.UseConfig(new BotSettings(new Uri("http://www.google.com"), "12345", "234234"));
        }

        [Fact]
        public void GetToken_CacheHasNoData_SetCache()
        {
            // Arrange
            _cacheService.Remove(TokenCachedKey);
            RestRequest request = GetRequestToken();
            var response = new RestResponse<Token>
            {
                Data = new Token { AccessToken = "!23", ExpiresIn = 100 },
                StatusCode = HttpStatusCode.OK
            };

            _restClient.Execute<Token>(request).Returns(response);

            var botConnector = new BotConnector(_restClient, _cacheService);

            // Act
            var token = botConnector.GetToken();

            // Assert
            Assert.Equal("!23", token);
            Assert.Equal("!23", _cacheService.Get<string>(TokenCachedKey));
        }

        [Fact]
        public void GetToken_CacheHasData_GetFromCache()
        {
            // Arrange
            _cacheService.Set(TokenCachedKey, "!23");
            var request = GetRequestToken();
            var botConnector = new BotConnector(_restClient, _cacheService);

            // Act
            var token = botConnector.GetToken();

            // Assert
            _restClient.DidNotReceive().Execute<Token>(request);
            Assert.Equal("!23", token);
        }

        private static RestRequest GetRequestToken()
        {
            return Arg.Is<RestRequest>(req =>
                   req.Parameters[0].Name == "grant_type" && req.Parameters[0].Value.ToString() == "client_credentials" &&
                   req.Parameters[1].Name == "client_id" && req.Parameters[1].Value.ToString() == "12345" &&
                   req.Parameters[2].Name == "client_secret" && req.Parameters[2].Value.ToString() == "234234" &&
                   req.Parameters[3].Name == "scope" && req.Parameters[3].Value.ToString() == "12345/.default");
        }
    }
}