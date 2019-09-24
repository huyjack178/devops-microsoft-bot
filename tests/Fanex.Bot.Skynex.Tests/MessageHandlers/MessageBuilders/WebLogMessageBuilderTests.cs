using Fanex.Bot.Core._Shared.Constants;

namespace Fanex.Bot.Skynex.Tests.MessageHandlers.MessageBuilders
{
    using System;
    using System.IO;
    using System.Net;
    using AutoFixture;
    using Log;
    using Xunit;

    public class WebLogMessageBuilderTests
    {
        private readonly IWebLogMessageBuilder webLogMessageBuilder;

        public WebLogMessageBuilderTests()
        {
            webLogMessageBuilder = new WebLogMessageBuilder();
        }

        [Fact]
        public void BuildMessage_IsNotNewLogType_LogLengthIsGreaterThan400_Get400CharLog()
        {
            // Arrange
            var fixture = new Fixture();
            var logMessage = string.Join(string.Empty, fixture.CreateMany<string>(450));
            var log = fixture.Create<Core.Log.Models.Log>();
            log.FormattedMessage = logMessage;

            // Act
            var actualMessage = webLogMessageBuilder.BuildMessage(log);

            // Assert
            var expectedLogMessage = FinalizeMessage(logMessage.Substring(0, 400), log);
            Assert.Equal(expectedLogMessage, actualMessage);
        }

        [Fact]
        public void BuildMessage_IsNotNewLogType_LogLengthIsLessThan400_GetAllLog()
        {
            // Arrange
            var fixture = new Fixture();
            var logMessage = "log";
            var log = fixture.Create<Core.Log.Models.Log>();
            log.FormattedMessage = logMessage;

            // Act
            var actualMessage = webLogMessageBuilder.BuildMessage(log);

            // Assert
            var expectedLogMessage = FinalizeMessage(logMessage, log);
            Assert.Equal(expectedLogMessage, actualMessage);
        }

        [Fact]
        public void BuildMessage_IsNotNewLogType_HasAlphaDomain_HideAlphaDomain()
        {
            // Arrange
            var fixture = new Fixture();
            var log = fixture.Create<Core.Log.Models.Log>();
            log.CategoryName = "alpha";
            log.FormattedMessage = GetNormalLogDataTest();

            // Act
            var actualMessage = webLogMessageBuilder.BuildMessage(log);

            // Assert
            var expecteRequestInfo =
                $"URL: http://alpha.site:8071/ex-main/_MemberInfo/SearchAndCopy/MasterPage.aspx?lv=2 {MessageFormatSignal.NEWLINE}" +
                "REFERRER: http://alpha.site:8071/site-main";
            Assert.Contains(expecteRequestInfo, actualMessage);
        }

        [Fact]
        public void BuildMessage_IsNewLogType_IsNotAlpha_GetRequestInfo()
        {
            // Arrange
            var fixture = new Fixture();
            var log = fixture.Create<Core.Log.Models.Log>();
            log.FormattedMessage = GetNewLogDataTest();

            // Act
            var actualMessage = webLogMessageBuilder.BuildMessage(log);

            // Assert
            var expecteRequestInfo =
                $"{MessageFormatSignal.BOLD_START}Request:{MessageFormatSignal.BOLD_END}" +
                "  http://mb.stakecity.com:8072/site-reports/Statement/";
            Assert.Contains(expecteRequestInfo, actualMessage);
        }

        [Fact]
        public void BuildMessage_IsNewLogType_IsAlpha_GetRequestInfo()
        {
            // Arrange
            var fixture = new Fixture();
            var log = fixture.Create<Core.Log.Models.Log>();
            log.CategoryName = "alpha";
            log.FormattedMessage = GetNewLogDataTest();

            // Act
            var actualMessage = webLogMessageBuilder.BuildMessage(log);

            // Assert
            var expecteRequestInfo =
                $"{MessageFormatSignal.BOLD_START}Request:{MessageFormatSignal.BOLD_END}" +
                $"  http://alpha.site:8072/site-reports/Statement/";
            Assert.Contains(expecteRequestInfo, actualMessage);
        }

        [Fact]
        public void BuildMessage_IsNewLogType_GetBrowserInfo()
        {
            // Arrange
            var fixture = new Fixture();
            var log = fixture.Create<Core.Log.Models.Log>();
            log.FormattedMessage = GetNewLogDataTest();

            // Act
            var actualMessage = webLogMessageBuilder.BuildMessage(log);

            // Assert
            var expecteRequestInfo =
                $"{MessageFormatSignal.BOLD_START}Browser:{MessageFormatSignal.BOLD_END}" +
                $"  Firefox 60.0 (Beta: False) {MessageFormatSignal.NEWLINE} MobileDeviceModel: Unknown";
            Assert.Contains(expecteRequestInfo, actualMessage);
        }

        [Fact]
        public void BuildMessage_IsNewLogType_GetServerAndDatabaseInfo()
        {
            // Arrange
            var fixture = new Fixture();
            var log = fixture.Create<Core.Log.Models.Log>();
            log.FormattedMessage = GetNewLogDataTest();

            // Act
            var actualMessage = webLogMessageBuilder.BuildMessage(log);

            // Assert
            var expecteDbInfo = $"{MessageFormatSignal.BOLD_START}Server:{MessageFormatSignal.BOLD_END}" +
                $" {log.MachineName} ({log.MachineIP})" +
                $"{MessageFormatSignal.NEWLINE}{MessageFormatSignal.BOLD_START}Database:{MessageFormatSignal.BOLD_END}{MessageFormatSignal.NEWLINE}" +
                $"Server: 10.40.40.100 {MessageFormatSignal.NEWLINE}" +
                $"DbName: DBACC.bodb02.BODBDownlineNet {MessageFormatSignal.NEWLINE}" +
                $"UserID: bodbDownlineNet {MessageFormatSignal.NEWLINE}" +
                $"SpName: Acc_StatementSelTransfer {MessageFormatSignal.NEWLINE}" +
                $"Parameters: @winlostdate=6/17/2018 12:00:00 AM;@custid=26570707 {MessageFormatSignal.NEWLINE}" +
                $"Line: 0 {MessageFormatSignal.NEWLINE}" +
                $"CommandTimeout: 120";
            Assert.Contains(expecteDbInfo, actualMessage);
        }

        [Fact]
        public void BuildMessage_IsNewLogType_GetCustomInfo()
        {
            // Arrange
            var fixture = new Fixture();
            var log = fixture.Create<Core.Log.Models.Log>();
            log.FormattedMessage = GetNewLogDataTest();

            // Act
            var actualMessage = webLogMessageBuilder.BuildMessage(log);

            // Assert
            var expectedCustomInfo =
                $"{MessageFormatSignal.BOLD_START}Custom Info:{MessageFormatSignal.BOLD_END} {MessageFormatSignal.NEWLINE}" +
                $"CustId: 26570707 {MessageFormatSignal.NEWLINE}" +
                $"CustName: IX1388 {MessageFormatSignal.NEWLINE}" +
                $"SubAccountName: IX1388SUB03 {MessageFormatSignal.NEWLINE}" +
                $"AdminName: ";
            Assert.Contains(expectedCustomInfo, actualMessage);
        }

        [Fact]
        public void BuildMessage_IsNewLogType_GetExceptionInfo()
        {
            // Arrange
            var fixture = new Fixture();
            var log = fixture.Create<Core.Log.Models.Log>();
            log.FormattedMessage = GetNewLogDataTest();

            // Act
            var actualMessage = webLogMessageBuilder.BuildMessage(log);

            // Assert
            var expecteExceptionInfo = $"{MessageFormatSignal.BOLD_START}Exception:{MessageFormatSignal.BOLD_END} {MessageFormatSignal.NEWLINE}" +
                $"Source: Fanex.Data.CrossCutting.WrappingException {MessageFormatSignal.NEWLINE}" +
                $"Type: Fanex.Data.CrossCutting.WrappingException.ObjectDbException {MessageFormatSignal.NEWLINE}" +
                $"TargetSite: System.Collections.Generic.IEnumerable`1[TReturn] Query[TReturn](System.Object, System.Data.Common.DbTransaction, Boolean, System.Nullable`1[System.Int32]) ";
            Assert.Contains(expecteExceptionInfo, actualMessage);
        }

        [Fact]
        public void Message_IsNewLogType_GetSessionInfo()
        {
            // Arrange
            var fixture = new Fixture();
            var log = fixture.Create<Core.Log.Models.Log>();
            log.FormattedMessage = GetNewLogDataTest();

            // Act
            var actualMessage = webLogMessageBuilder.BuildMessage(log);

            // Assert
            var expectedSessionInfo =
                   $"{MessageFormatSignal.BOLD_START}Session Info:{MessageFormatSignal.BOLD_END}{MessageFormatSignal.NEWLINE}" +
                   $"Username: starix69 {MessageFormatSignal.NEWLINE}" +
                   $"AccUserName: starix69 {MessageFormatSignal.NEWLINE}" +
                   $"CustRoleId: 4 {MessageFormatSignal.NEWLINE}" +
                   $"custid: 27671895 {MessageFormatSignal.NEWLINE}" +
                   $"CustUname: supertestPRO2 {MessageFormatSignal.NEWLINE}" +
                   $"MemberID: 0 {MessageFormatSignal.NEWLINE}" +
                   $"MemberUserName: {MessageFormatSignal.NEWLINE}" +
                   $"AgentID: 0 {MessageFormatSignal.NEWLINE}" +
                   $"AgentUserName: {MessageFormatSignal.NEWLINE}" +
                   $"MasterID: 0 {MessageFormatSignal.NEWLINE}" +
                   $"MasterUserName: {MessageFormatSignal.NEWLINE}" +
                   $"SuperID: 27671895 {MessageFormatSignal.NEWLINE}" +
                   $"SusperUserName: supertestPRO2 {MessageFormatSignal.NEWLINE}" +
                   $"IsSyncCSCurrentCust: False {MessageFormatSignal.NEWLINE}" +
                   $"IsInternal: False {MessageFormatSignal.NEWLINE}" +
                   $"sitename: BEST-ODDS {MessageFormatSignal.NEWLINE}";
            Assert.Contains(expectedSessionInfo, actualMessage);
        }

        private string FinalizeMessage(string message, Core.Log.Models.Log log)
        {
            var returnMessage = message
                    .Replace("\r", string.Empty)
                    .Replace("\t", string.Empty)
                    .Replace("Timestamp", $"{MessageFormatSignal.BOLD_START}Timestamp{MessageFormatSignal.BOLD_END}")
                    .Replace("Message", $"{MessageFormatSignal.BOLD_START}Message{MessageFormatSignal.BOLD_END}");

            return $"{MessageFormatSignal.BOLD_START}Category{MessageFormatSignal.BOLD_END}: {log.CategoryName}{MessageFormatSignal.NEWLINE}" +
                    $"{WebUtility.HtmlDecode(returnMessage)}{MessageFormatSignal.NEWLINE}" +
                    $"{MessageFormatSignal.BOLD_START}#Log Id{MessageFormatSignal.BOLD_END}: {log.LogId} " +
                    $"{MessageFormatSignal.BOLD_START}Count{MessageFormatSignal.BOLD_END}: " +
                    $"{log.NumMessage}{MessageFormatSignal.DOUBLE_NEWLINE}{MessageFormatSignal.DIVIDER}";
        }

        private string GetNewLogDataTest()
            => File.ReadAllText($"{AppDomain.CurrentDomain.BaseDirectory}/../../../Data/Log.txt");

        private string GetNormalLogDataTest()
            => File.ReadAllText($"{AppDomain.CurrentDomain.BaseDirectory}/../../../Data/NormalLog.txt");
    }
}