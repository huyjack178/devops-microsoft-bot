namespace Fanex.Bot.Skynex.Tests.Services
{
    using System;
    using System.Threading.Tasks;
    using Fanex.Bot.Core.Utilities.Web;
    using Fanex.Bot.Models.UM;
    using Fanex.Bot.Services;
    using Microsoft.Extensions.Configuration;
    using NSubstitute;
    using Xunit;

    public class UMServiceTests
    {
        private readonly IWebClient webClient;
        private readonly IUMService umService;

        public UMServiceTests()
        {
            var config = new ConfigurationBuilder()
               .AddJsonFile($"{AppDomain.CurrentDomain.BaseDirectory}/../../../appsettings.json")
               .Build();
            webClient = Substitute.For<IWebClient>();
            umService = new UMService(webClient, config);
        }

        [Fact]
        public async Task GetUMInformation_Always_GetFromAPI()
        {
            // Arrange
            var umInfo = new UM { IsUM = true };
            webClient.GetJsonAsync<UM>(Arg.Is(new Uri("http://msite.starific.net/V1/api/UM/Information"))).Returns(umInfo);

            // Act
            var actualUmInfo = await umService.GetUMInformation();

            // Assert
            Assert.Equal(umInfo, actualUmInfo);
        }

        [Fact]
        public async Task CheckPageShowUM_PageContentContainsUMKeyword_ReturnTrue()
        {
            // Arrange
            var pageUrl = new Uri("http://google.com");
            webClient.GetContentAsync(Arg.Is(pageUrl)).Returns("<html><title>offline</title><body>offline</body></html>");

            // Act
            var result = await umService.CheckPageShowUM(pageUrl);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task CheckPageShowUM_PageContentDoestNotContainUMKeyword_ReturnTrue()
        {
            // Arrange
            var pageUrl = new Uri("http://google.com");
            webClient.GetContentAsync(Arg.Is(pageUrl)).Returns("<html><title>ok</title><body>ok</body></html>");

            // Act
            var result = await umService.CheckPageShowUM(pageUrl);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task CheckPageShowUM_Exception_ReturnFalse()
        {
            // Arrange
            var pageUrl = new Uri("http://google.com");
            webClient.GetContentAsync(Arg.Is(pageUrl)).Returns("");

            // Act
            var result = await umService.CheckPageShowUM(pageUrl);

            // Assert
            Assert.False(result);
        }
    }
}