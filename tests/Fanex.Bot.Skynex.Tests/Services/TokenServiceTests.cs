using Fanex.Bot.Core.Bot.Models;
using Fanex.Bot.Core.Bot.Services;

namespace Fanex.Bot.Skynex.Tests.Services
{
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using NSubstitute;
    using RestSharp;
    using Xunit;

    public class TokenServiceTests
    {
        private readonly IConfiguration subConfiguration;
        private readonly IRestClient subRestClient;
        private readonly ITokenService tokenService;

        public TokenServiceTests()
        {
            subConfiguration = Substitute.For<IConfiguration>();
            subRestClient = Substitute.For<IRestClient>();
            tokenService = new TokenService(subConfiguration, subRestClient);
        }

        [Fact]
        public async Task GetToken_PasswordIsValid_GetToken()
        {
            // Arrange
            var clientId = "1";
            var clientPassword = "Bult8TMrOurQs1JtkvbvCBjJypuSmNO8";
            subConfiguration.GetSection("TokenUrl").Value.Returns("http://token.com");
            subConfiguration.GetSection("MicrosoftAppId").Value.Returns("12312312");
            subConfiguration.GetSection("MicrosoftAppPassword").Value.Returns("234234");
            var expectedToken = new Token { AccessToken = "123234324" };
            var responseData = new RestResponse<Token> { Data = expectedToken };
            subRestClient.ExecuteTaskAsync<Token>(Arg.Any<RestRequest>()).Returns(responseData);

            // Act
            var token = await tokenService.GetToken(clientId, clientPassword);

            // Assert
            Assert.Equal(expectedToken.AccessToken, token);
            Assert.Equal("http://token.com/", subRestClient.BaseUrl.ToString());
            await subRestClient.Received().ExecuteTaskAsync<Token>(Arg.Is<RestRequest>(r => r.Method == Method.POST));
            await subRestClient.Received().ExecuteTaskAsync<Token>(
                Arg.Is<RestRequest>(r => r.Parameters[0].Name == "grant_type" && r.Parameters[0].Value.ToString() == "client_credentials"));
            await subRestClient.Received().ExecuteTaskAsync<Token>(
                Arg.Is<RestRequest>(r => r.Parameters[1].Name == "client_id" && r.Parameters[1].Value.ToString() == "12312312"));
            await subRestClient.Received().ExecuteTaskAsync<Token>(
                Arg.Is<RestRequest>(r => r.Parameters[2].Name == "client_secret" && r.Parameters[2].Value.ToString() == "234234"));
            await subRestClient.Received().ExecuteTaskAsync<Token>(
                Arg.Is<RestRequest>(r => r.Parameters[3].Name == "scope" && r.Parameters[3].Value.ToString() == "12312312/.default"));
            await subRestClient.Received().ExecuteTaskAsync<Token>(
              Arg.Is<RestRequest>(r => r.Parameters[4].Name == "Content-type" && r.Parameters[4].Value.ToString() == "application/x-www-form-urlencoded"));
        }

        [Fact]
        public async Task GetToken_PasswordIsNotValid_ReturnNull()
        {
            // Arrange
            var clientId = "1";
            var clientPassword = "3423423";

            // Act
            var token = await tokenService.GetToken(clientId, clientPassword);

            // Assert
            Assert.Null(token);
        }
    }
}