﻿namespace Fanex.Bot.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading.Tasks;
    using Fanex.Bot.Models.Log;
    using Fanex.Bot.Services;
    using Fanex.Bot.Utilitites;
    using NSubstitute;
    using Xunit;

    public class LogServiceTests
    {
        private readonly IWebClient _webClient;
        private readonly ILogService _logService;

        public LogServiceTests()
        {
            _webClient = Substitute.For<IWebClient>();
            _logService = new LogService(_webClient);
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

            _webClient.PostAsync<GetLogFormData, IEnumerable<Log>>(
                "PublicLog/Logs",
                Arg.Is<GetLogFormData>(data =>
                    data.From == fromDate.AddHours(7).ToString(CultureInfo.InvariantCulture) &&
                    data.To == toDate.AddHours(7).ToString(CultureInfo.InvariantCulture) &&
                    data.Severity == "Error" &&
                    data.Size == 5 &&
                    data.Page == 0 &&
                    data.ToGMT == 7 &&
                    data.CategoryId == 0 &&
                    data.MachineId == 0 &&
                    data.IsProduction == isProduction))
                .Returns(expectedLogList);

            // Act
            var actualLogList = await _logService.GetErrorLogs(fromDate, toDate, isProduction);

            // Assert
            Assert.Equal(expectedLogList, actualLogList);
        }

        [Fact]
        public async Task GetErrorLogAsync_NoErrorLog_ReturnEmptyList()
        {
            // Arrange
            var fromDate = new DateTime(2018, 01, 01);
            var toDate = new DateTime(2018, 01, 02);
            var isProduction = true;
            var expectedLogList = new List<Log>();

            _webClient.PostAsync<GetLogFormData, IEnumerable<Log>>(
                "PublicLog/Logs", Arg.Any<GetLogFormData>())
                .Returns(expectedLogList);

            // Act
            var actualLogList = await _logService.GetErrorLogs(fromDate, toDate, isProduction);

            // Assert
            Assert.Equal(expectedLogList, actualLogList);
        }

        [Fact]
        public async Task GetErrorLogDetail_Always_GetFromApi()
        {
            // Arrange
            var logId = 1;
            var expectedLog = new Log { LogId = 1 };
            _webClient.GetAsync<Log>($"PublicLog/Log?logId={logId}").Returns(expectedLog);

            // Act
            var actualLog = await _logService.GetErrorLogDetail(logId);

            // Assert
            Assert.Equal(expectedLog, actualLog);
        }
    }
}