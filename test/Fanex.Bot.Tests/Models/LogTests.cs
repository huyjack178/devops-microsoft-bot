namespace Fanex.Bot.Tests.Models
{
    using System;
    using System.IO;
    using AutoFixture;
    using Fanex.Bot.Skynex.Models.Log;
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
        public void Message_IsNewLogType_IsNotAlpha_GetRequestInfo()
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
        public void Message_IsNewLogType_IsAlpha_GetRequestInfo()
        {
            // Arrange
            var fixture = new Fixture();
            var log = fixture.Create<Log>();
            log.CategoryName = "alpha";

            // Act
            log.FormattedMessage = GetLogDataTest();

            // Assert
            var expecteRequestInfo = $"**Request:**" +
                $" http://alpha.site/site-reports/Statement/";
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
            var expecteDbInfo = $"**Server:**" +
                $" {log.MachineName} ({log.MachineIP})" +
                $"\n\n**Database:**\n\n" +
                $"Server: 10.40.40.100 \n\n" +
                $"DbName: DBACC.bodb02.BODBDownlineNet \n\n" +
                $"UserID: bodbDownlineNet \n\n" +
                $"SpName: Acc_StatementSelTransfer \n\n" +
                $"Parameters: @winlostdate=6/17/2018 12:00:00 AM;@custid=26570707 \n\n" +
                $"Line: 0 \n\n" +
                $"CommandTimeout: 120";
            Assert.Contains(expecteDbInfo, log.Message);
        }

        [Fact]
        public void Message_IsNewLogType_GetCustomInfo()
        {
            // Arrange
            var fixture = new Fixture();
            var log = fixture.Create<Log>();

            // Act
            log.FormattedMessage = GetLogDataTest();

            // Assert
            var expectedCustomInfo =
                $"**Custom Info:** \n\n " +
                $"\n\nCustId: 26570707 \n\n" +
                $"CustName: IX1388 \n\n" +
                $"SubAccountName: IX1388SUB03 \n\n" +
                $"AdminName: ";
            Assert.Contains(expectedCustomInfo, log.Message);
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
            var expecteExceptionInfo = $"**Exception:** \n\n \n\n" +
                $"Source: Fanex.Data.CrossCutting.WrappingException \n\n" +
                $"Type: Fanex.Data.CrossCutting.WrappingException.ObjectDbException \n\n" +
                $"TargetSite: System.Collections.Generic.IEnumerable`1[TReturn] Query[TReturn](System.Object, System.Data.Common.DbTransaction, Boolean, System.Nullable`1[System.Int32]) ";
            Assert.Contains(expecteExceptionInfo, log.Message);
        }

        [Fact]
        public void Message_IsNewLogType_GetSessionInfo()
        {
            // Arrange
            var fixture = new Fixture();
            var log = fixture.Create<Log>();

            // Act
            log.FormattedMessage = GetLogDataTest();

            // Assert
            var expectedSessionInfo =
                   "**Session Info:**\n\nUsername: starix69 \n\n\n" +
                   "AccUserName: starix69 \n\n\n" +
                   "CustRoleId: 4 \n\n\n" +
                   "custid: 27671895 \n\n\n" +
                   "CustUname: supertestPRO2 \n\n\n" +
                   "MemberID: 0 \n\n\nMemberUserName: \n\n\n" +
                   "AgentID: 0 \n\n\n" +
                   "AgentUserName: \n\n\n" +
                   "MasterID: 0 \n\n\n" +
                   "MasterUserName: \n\n\n" +
                   "SuperID: 27671895 \n\n\n" +
                   "SusperUserName: supertestPRO2 \n\n\n" +
                   "IsSyncCSCurrentCust: False \n\n\n" +
                   "IsInternal: False \n\n\n" +
                   "sitename: BEST-ODDS \n\n\n";
            Assert.Contains(expectedSessionInfo, log.Message);
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

            return $"**Category**: {log.CategoryName}\n\n" +
                    $"{returnMessage}\n\n" +
                    $"**#Log Id**: {log.LogId} " +
                    $"**Count**: {log.NumMessage}\n\n\n\n" +
                    $"====================================\n\n";
        }

        private string GetLogDataTest()
            => File.ReadAllText($"{AppDomain.CurrentDomain.BaseDirectory}/../../../Data/Log.txt");
    }
}