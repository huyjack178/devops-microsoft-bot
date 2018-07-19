namespace Fanex.Bot.Skynex.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fanex.Bot.Core.Utilities.Web;
    using Fanex.Bot.Skynex.Models.UM;
    using Fanex.Bot.Skynex.Services;
    using Microsoft.Extensions.Configuration;
    using NSubstitute;
    using Xunit;

    public class UMServiceTests
    {
        private readonly IWebClient _webClient;
        private readonly IUMService _umService;

        public UMServiceTests()
        {
            var config = new ConfigurationBuilder()
               .AddJsonFile($"{AppDomain.CurrentDomain.BaseDirectory}/../../../appsettings.json")
               .Build();
            _webClient = Substitute.For<IWebClient>();
            _umService = new UMService(_webClient, config);
        }

        [Fact]
        public async Task GetUMInformation_Always_GetFromAPI()
        {
            // Arrange
            var umInfo = new UM { IsUM = true };
            _webClient.GetJsonAsync<UM>(Arg.Is(new Uri("http://msite.starific.net/V1//Bot/UMInformation"))).Returns(umInfo);

            // Act
            var actualUmInfo = await _umService.GetUMInformation();

            // Assert
            Assert.Equal(umInfo, actualUmInfo);
        }

        [Fact]
        public async Task CheckPageShowUM_PageContentContainsUMKeyword_ReturnTrue()
        {
            // Arrange
            var pageUrl = new Uri("http://google.com");
            _webClient.GetContentAsync(Arg.Is(pageUrl)).Returns("<html><title>offline</title><body>offline</body></html>");

            // Act
            var result = await _umService.CheckPageShowUM(pageUrl);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task CheckPageShowUM_PageContentDoestNotContainUMKeyword_ReturnTrue()
        {
            // Arrange
            var pageUrl = new Uri("http://google.com");
            _webClient.GetContentAsync(Arg.Is(pageUrl)).Returns("<html><title>ok</title><body>ok</body></html>");

            // Act
            var result = await _umService.CheckPageShowUM(pageUrl);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task CheckPageShowUM_Exception_ReturnFalse()
        {
            // Arrange
            var pageUrl = new Uri("http://google.com");
            _webClient.GetContentAsync(Arg.Is(pageUrl)).Returns("");

            // Act
            var result = await _umService.CheckPageShowUM(pageUrl);

            // Assert
            Assert.False(result);
        }
    }
}