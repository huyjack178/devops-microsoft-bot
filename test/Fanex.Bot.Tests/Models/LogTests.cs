namespace Fanex.Bot.Tests.Models
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using AutoFixture;
    using Fanex.Bot.Models.Log;
    using Xunit;

    public class LogTests
    {
        [Fact]
        public void Message_IsNotNewLogType_LogLengthIsGreaterThan400_Get400CharLog()
        {
            // Arrange
            var fixture = new Fixture();
            var logMessage = string.Join(string.Empty, fixture.CreateMany<string>(400));
            var log = fixture.Create<Log>();

            // Act
            log.FormattedMessage = logMessage;

            // Assert
            var expectedLogMessage = FormatLog(log, logMessage.Substring(0, 400));
            Assert.Equal(expectedLogMessage, log.Message);
        }

        [Fact]
        public void Message_IsNotNewLogType_LogLengthIsLessThan400_GetAllLog()
        {
            // Arrange
            var fixture = new Fixture();
            var logMessage = "log";
            var log = fixture.Create<Log>();

            // Act
            log.FormattedMessage = logMessage;

            // Assert
            var expectedLogMessage = FormatLog(log, logMessage);
            Assert.Equal(expectedLogMessage, log.Message);
        }

        [Fact]
        public void Message_IsNewLogType_GetRequestInfo()
        {
            // Arrange
            var fixture = new Fixture();
            var log = fixture.Create<Log>();

            // Act
            log.FormattedMessage = GetLogDataTest();

            // Assert
            var expecteRequestInfo = $"**Request:**" +
                $"  http://mb.stakecity.com:8072/site-reports/Statement/";
            Assert.Contains(expecteRequestInfo, log.Message);
        }

        [Fact]
        public void Message_IsNewLogType_GetBrowserInfo()
        {
            // Arrange
            var fixture = new Fixture();
            var log = fixture.Create<Log>();

            // Act
            log.FormattedMessage = GetLogDataTest();

            // Assert
            var expecteRequestInfo = $"**Browser:**" +
                $"  Firefox 60.0 (Beta: False) \n\n MobileDeviceModel: Unknown";
            Assert.Contains(expecteRequestInfo, log.Message);
        }

        [Fact]
        public void Message_IsNewLogType_GetServerAndDatabaseInfo()
        {
            // Arrange
            var fixture = new Fixture();
            var log = fixture.Create<Log>();

            // Act
            log.FormattedMessage = GetLogDataTest();

            // Assert
            var expecteRequestInfo = $"**Server:**" +
                $" {log.Machine.MachineName} ({log.Machine.MachineIP})" +
                $"\n\n**Database:**\n\n" +
                $"Server: 10.40.40.100 \n\n" +
                $"DbName: DBACC.bodb02.BODBDownlineNet \n\n" +
                $"UserID: bodbDownlineNet \n\n" +
                $"SpName: Acc_StatementSelTransfer ";
            Assert.Contains(expecteRequestInfo, log.Message);
        }

        [Fact]
        public void Message_IsNewLogType_GetExceptionInfo()
        {
            // Arrange
            var fixture = new Fixture();
            var log = fixture.Create<Log>();

            // Act
            log.FormattedMessage = GetLogDataTest();

            // Assert
            var expecteRequestInfo = $"**Exception:** \n\n \n\n" +
                $"Source: Fanex.Data.CrossCutting.WrappingException \n\n" +
                $"Type: Fanex.Data.CrossCutting.WrappingException.ObjectDbException \n\n" +
                $"TargetSite: System.Collections.Generic.IEnumerable`1[TReturn] Query[TReturn](System.Object, System.Data.Common.DbTransaction, Boolean, System.Nullable`1[System.Int32]) ";
            Assert.Contains(expecteRequestInfo, log.Message);
        }

        private string FormatLog(Log log, string message)
        {
            var returnMessage = message.Replace("\r", "\n").Replace("\t", string.Empty)
                      .Replace("Timestamp", "**Timestamp**")
                      .Replace("Message", "**Message**")
                      .Replace("REQUEST INFO", "**REQUEST INFO**")
                      .Replace("BROWSER INFO", "**BROWSER INFO**")
                      .Replace("SERVER INFO", "**SERVER INFO**")
                      .Replace("DATABASE INFO", "**DATABASE INFO**")
                      .Replace("EXCEPTION INFO", "**EXCEPTION INFO**")
                      .Replace("REQUEST HEADERS", "**REQUEST HEADERS**")
                      .Replace("SESSION INFO", "**SESSION INFO**");

            return $"**Category**: {log.Category.CategoryName}\n\n" +
                    $"{returnMessage}\n\n" +
                    $"**#Log Id**: {log.LogId} " +
                    $"**Count**: {log.NumMessage}\n\n\n\n" +
                    $"====================================\n\n";
        }

        private string GetLogDataTest()
            => File.ReadAllText($"{AppDomain.CurrentDomain.BaseDirectory}/../../../Data/Log.txt");
    }
}