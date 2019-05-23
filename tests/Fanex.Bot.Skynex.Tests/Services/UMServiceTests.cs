namespace Fanex.Bot.Skynex.Tests.Services
{
    using System;
    using System.Collections.Generic;
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
        private readonly IUnderMaintenanceService umService;

        public UMServiceTests()
        {
            var config = new ConfigurationBuilder()
               .AddJsonFile($"{AppDomain.CurrentDomain.BaseDirectory}/../../../appsettings.json")
               .Build();
            webClient = Substitute.For<IWebClient>();
            umService = new UnderMaintenanceService(webClient, config);
        }

        [Fact]
        public async Task GetScheduledInfo_Always_GetFromAPI()
        {
            //  Arrange
            var umInfo = new Dictionary<int, UM> { { 1, new UM { IsUnderMaintenanceTime = true } } };

            webClient
                .GetJsonAsync<Dictionary<int, UM>>(Arg.Is(new Uri("http://log.com/UnderMaintenance/ScheduledInfo")))
                .Returns(umInfo);

            //  Act
            var scheduleInfo = await umService.GetScheduledInfo();

            // Assert
            Assert.Equal(umInfo, scheduleInfo);
        }

        [Fact]
        public async Task GetActualInfo_Always_GetFromAPI()
        {
            //  Arrange
            var umInfo = new Dictionary<int, UM> { { 1, new UM { IsUnderMaintenanceTime = true } } };

            webClient
                .GetJsonAsync<Dictionary<int, UM>>(Arg.Is(new Uri("http://log.com/UnderMaintenance/ActualInfo")))
                .Returns(umInfo);

            //  Act
            var actualInfo = await umService.GetActualInfo();

            // Assert
            Assert.Equal(umInfo, actualInfo);
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