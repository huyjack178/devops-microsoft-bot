namespace Fanex.Bot.Skynex.Tests.Services
{
    using System.Threading.Tasks;
    using Fanex.Bot.Services;
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
        public Task GetToken_StateUnderTest_ExpectedBehavior()
        {
            return Task.CompletedTask;
        }
    }
}