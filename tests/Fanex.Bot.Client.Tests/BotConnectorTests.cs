namespace Fanex.Bot.Client.Tests
{
    using System;
    using System.Net;
    using Fanex.Bot.Client.Configuration;
    using Fanex.Bot.Client.Models;
    using Fanex.Caching;
    using NSubstitute;
    using RestSharp;
    using Xunit;

    public class BotConnectorTests
    {
        private const string TokenCachedKey = "TokenCachedKey";
        private const string Token = "12sdfsdf";
        private const string ConversationId = "12345";
        private readonly IRestClient restClient;
        private readonly ICacheService cacheService;

        public BotConnectorTests()
        {
            restClient = Substitute.For<IRestClient>();
            cacheService = new CacheService();
            BotClientManager.UseConfig(new BotSettings(new Uri("http://www.google.com"), "12345", "234234"));
            cacheService.Remove(TokenCachedKey);
        }

        [Fact]
        public void Send_ResultIsOk_ReturnResult()
        {
            // Arrange
            var tokenResponse = new RestResponse
            {
                Content = Token,
                StatusCode = HttpStatusCode.OK
            };
            var forwardToBotResponse = new RestResponse
            {
                StatusCode = HttpStatusCode.OK
            };
            restClient.Execute(Arg.Any<IRestRequest>()).Returns(tokenResponse);
            restClient.Execute(Arg.Any<IRestRequest>()).Returns(forwardToBotResponse);
            var botConnector = new BotConnector(restClient, cacheService);

            // Act
            var result = botConnector.Send("message", ConversationId);

            // Assert
            Assert.Equal("200", result);
        }

        [Fact]
        public void Send_ResultIsUnauthorized_RefreshToken()
        {
            // Arrange
            var tokenResponse = new RestResponse
            {
                Content = Token,
                StatusCode = HttpStatusCode.OK
            };
            var forwardToBotResponse = new RestResponse
            {
                StatusCode = HttpStatusCode.Unauthorized
            };
            restClient.Execute(Arg.Any<IRestRequest>()).Returns(tokenResponse);
            restClient.Execute(Arg.Any<IRestRequest>()).Returns(forwardToBotResponse);
            var botConnector = new BotConnector(restClient, cacheService);

            // Act
            botConnector.Send("message", ConversationId);

            // Assert
            restClient.Received(2).Execute(Arg.Is<RestRequest>(req => req.Resource == "/token"));
            restClient.Received(2).Execute(Arg.Is<RestRequest>(req => req.Resource == "/messages/forward"));
        }

        [Fact]
        public void ForwardToBot_NotTimeout_ExecuteCorrectRequest()
        {
            var request = GetRequestForwardToBot();

            var response = new RestResponse
            {
                StatusCode = HttpStatusCode.OK
            };

            restClient.Execute(request).Returns(response);
            var botConnector = new BotConnector(restClient, cacheService);

            // Act
            var result = botConnector.ForwardToBot("message", ConversationId, Token, Enums.MessageType.Markdown);

            // Assert
            Assert.Equal(200, result);
        }

        [Fact]
        public void ForwardToBot_Timeout_ThrowTimeoutException()
        {
            // Arrange
            var response = new RestResponse
            {
                StatusCode = 0
            };

            restClient.Execute(Arg.Any<IRestRequest>()).Returns(response);
            var botConnector = new BotConnector(restClient, cacheService);

            // Act & Assert
            Assert.Throws<TimeoutException>(() => botConnector.ForwardToBot("message", "132123", "1231231", Enums.MessageType.Markdown));
        }

        [Fact]
        public void GetToken_CacheHasNoData_SetCache()
        {
            // Arrange
            RestRequest request = GetRequestToken();
            var response = new RestResponse<Token>
            {
                Content = Token,
                StatusCode = HttpStatusCode.OK
            };

            restClient.Execute(request).Returns(response);
            var botConnector = new BotConnector(restClient, cacheService);

            // Act
            var token = botConnector.GetToken();

            // Assert
            Assert.Equal(Token, token);
            Assert.Equal(Token, cacheService.Get<string>(TokenCachedKey));
        }

        [Fact]
        public void GetToken_CacheHasData_GetFromCache()
        {
            // Arrange
            cacheService.Set(TokenCachedKey, Token);
            var request = GetRequestToken();
            var botConnector = new BotConnector(restClient, cacheService);

            // Act
            var token = botConnector.GetToken();

            // Assert
            restClient.DidNotReceive().Execute<Token>(request);
            Assert.Equal(Token, token);
        }

        [Fact]
        public void GetToken_Timeout_ThrowTimeoutException()
        {
            // Arrange
            var response = new RestResponse
            {
                StatusCode = 0
            };

            restClient.Execute(Arg.Any<IRestRequest>()).Returns(response);
            var botConnector = new BotConnector(restClient, cacheService);

            // Act & Assert
            Assert.Throws<TimeoutException>(() => botConnector.GetToken());
        }

        private static RestRequest GetRequestToken() =>
            Arg.Is<RestRequest>(req =>
                req.Resource == "/token" &&
                req.Method == Method.GET &&
                req.Parameters[0].Name == "clientId" && req.Parameters[0].Value.ToString() == "12345" &&
                req.Parameters[1].Name == "clientPassword" && req.Parameters[1].Value.ToString() == "234234");

        private static IRestRequest GetRequestForwardToBot()
            => Arg.Is<RestRequest>(req =>
                req.Resource == "/messages/forward" &&
                req.Method == Method.POST &&
                req.Parameters[0].Name == "Authorization" && req.Parameters[0].Value.ToString() == $"Bearer {Token}" &&
                req.Parameters[1].Name == "content-type" && req.Parameters[1].Value.ToString() == "application/x-www-form-urlencoded" &&
                req.Parameters[2].Name == "message" && req.Parameters[2].Value.ToString() == "message" &&
                req.Parameters[3].Name == "conversationId" && req.Parameters[3].Value.ToString() == ConversationId);
    }
}