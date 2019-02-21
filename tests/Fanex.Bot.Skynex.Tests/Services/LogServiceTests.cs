namespace Fanex.Bot.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net;
    using System.Threading.Tasks;
    using Fanex.Bot.Core.Utilities.Web;
    using Fanex.Bot.Models;
    using Fanex.Bot.Models.Log;
    using Fanex.Bot.Services;
    using Microsoft.Extensions.Configuration;
    using NSubstitute;
    using Xunit;

    public class LogServiceTests
    {
        private readonly IWebClient webClient;
        private readonly ILogService logService;
        private readonly IConfiguration configuration;

        public LogServiceTests()
        {
            configuration = new ConfigurationBuilder()
              .AddJsonFile($"{AppDomain.CurrentDomain.BaseDirectory}/../../../appsettings.json")
              .Build();
            webClient = Substitute.For<IWebClient>();
            logService = new LogService(webClient, configuration);
        }

        [Fact]
        public async Task GetErrorLogAsync_HasErrorLog_ReturnErrorLogList()
        {
            // Arrange
            var fromDate = new DateTime(2018, 01, 01);
            var toDate = new DateTime(2018, 01, 02);
            var isProduction = true;
            var expectedLogList = new List<Log>{
                new Log { LogId = 1 },
                new Log { LogId = 2 }
            };

            webClient.PostJsonAsync<GetLogFormData, IEnumerable<Log>>(
                new Uri("http://log.com/Log/List"),
                Arg.Is<GetLogFormData>(data =>
                    data.From == fromDate.ToString(CultureInfo.InvariantCulture) &&
                    data.To == toDate.ToString(CultureInfo.InvariantCulture) &&
                    data.Severity == "Error" &&
                    data.Size == 10 &&
                    data.Page == 0 &&
                    data.ToGMT == 7 &&
                    data.CategoryId == 0 &&
                    data.MachineId == 0 &&
                    data.IsProduction == isProduction))
                .Returns(expectedLogList);

            // Act
            var actualLogList = await logService.GetErrorLogs(fromDate, toDate, isProduction);

            // Assert
            Assert.Equal(expectedLogList, actualLogList);
        }

        [Fact]
        public async Task GetErrorLogAsync_NoErrorLog_ReturnEmptyList()
        {
            // Arrange
            var fromDate = new DateTime(2018, 01, 01);
            var toDate = new DateTime(2018, 01, 02);
            const bool isProduction = true;
            var expectedLogList = new List<Log>();

            webClient.PostJsonAsync<GetLogFormData, IEnumerable<Log>>(
                new Uri("http://log.com/Log/List"), Arg.Any<GetLogFormData>())
                .Returns(expectedLogList);

            // Act
            var actualLogList = await logService.GetErrorLogs(fromDate, toDate, isProduction);

            // Assert
            Assert.Equal(expectedLogList, actualLogList);
        }

        [Fact]
        public async Task GetErrorLogDetail_Always_GetFromApi()
        {
            // Arrange
            const int logId = 1;
            var expectedLog = new Log { LogId = 1 };
            webClient.GetJsonAsync<Log>(new Uri("http://log.com/Log" + $"?logId={logId}")).Returns(expectedLog);

            // Act
            var actualLog = await logService.GetErrorLogDetail(logId);

            // Assert
            Assert.Equal(expectedLog, actualLog);
        }

        [Fact]
        public async Task GetDBLogAsync_HasErrorLog_ReturnErrorLogList()
        {
            // Arrange
            var expectedLogList = new List<DBLog>{
                new DBLog { NotificationId = 1 },
                new DBLog { NotificationId = 2 }
            };

            webClient
                .PostJsonAsync<string, IEnumerable<DBLog>>(new Uri("http://log.com/DbLog/List"), string.Empty)
                .Returns(expectedLogList);

            // Act
            var actualLogList = await logService.GetDBLogs();

            // Assert
            Assert.Equal(expectedLogList, actualLogList);
        }

        [Fact]
        public async Task AckDBLogAsync_StatusIsOk_ReturnSuccessfulResult()
        {
            // Arrange
            var notificationIds = new[] { 1, 2, 3 };

            webClient
                .PostJsonAsync(new Uri("http://log.com/DbLog/Ack"), notificationIds)
                .Returns(HttpStatusCode.OK);

            // Act
            var result = await logService.AckDBLog(notificationIds);

            // Assert
            Assert.True(result.IsOk);
        }

        [Fact]
        public async Task AckDBLogAsync_StatusIsNotOk_ReturnFailedResult()
        {
            // Arrange
            var notificationIds = new[] { 1, 2, 3 };

            webClient
                .PostJsonAsync(new Uri("http://log.com/DbLog/Ack"), notificationIds)
                .Returns(HttpStatusCode.InternalServerError);

            // Act
            var result = await logService.AckDBLog(notificationIds);

            // Assert
            Assert.False(result.IsOk);
        }
    }
}