namespace Fanex.Bot.Service.Tests.Controllers
{
    using System.Threading.Tasks;
    using Fanex.Bot.Services;
    using Fanex.Bot.Skynex.Controllers;
    using NSubstitute;
    using Xunit;

    public class TokenControllerTests
    {
        private readonly ITokenService subTokenService;
        private readonly TokenController tokenController;

        public TokenControllerTests()
        {
            subTokenService = Substitute.For<ITokenService>();
            tokenController = new TokenController(subTokenService);
        }

        [Fact]
        public async Task Get_TokenIsNotNull_ReturnToken()
        {
            // Arrange
            var clientId = "1";
            var clientPassword = "23234";
            subTokenService.GetToken(clientId, clientPassword).Returns("234234");

            // Act
            var token = await tokenController.Get(clientId, clientPassword);

            // Assert
            Assert.Equal("234234", token);
        }

        [Fact]
        public async Task Get_TokenIsNotNull_ReturnErrorMessage()
        {
            // Arrange
            var clientId = "1";
            var clientPassword = "23234";
            subTokenService.GetToken(clientId, clientPassword).Returns("");

            // Act
            var token = await tokenController.Get(clientId, clientPassword);

            // Assert
            Assert.Equal("Invalid client information", token);
        }
    }
}