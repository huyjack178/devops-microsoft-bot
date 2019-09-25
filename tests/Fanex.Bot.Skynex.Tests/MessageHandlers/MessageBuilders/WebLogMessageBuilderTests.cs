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
                $"URL: http://alpha.site:8071/ex-main/_MemberInfo/SearchAndCopy/MasterPage.aspx?lv=2 {MessageFormatSymbol.NEWLINE}" +
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
                $"{MessageFormatSymbol.BOLD_START}Request:{MessageFormatSymbol.BOLD_END}" +
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
                $"{MessageFormatSymbol.BOLD_START}Request:{MessageFormatSymbol.BOLD_END}" +
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
                $"{MessageFormatSymbol.BOLD_START}Browser:{MessageFormatSymbol.BOLD_END}" +
                $"  Firefox 60.0 (Beta: False) {MessageFormatSymbol.NEWLINE} MobileDeviceModel: Unknown";
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
            var expecteDbInfo = $"{MessageFormatSymbol.BOLD_START}Server:{MessageFormatSymbol.BOLD_END}" +
                $" {log.MachineName} ({log.MachineIP})" +
                $"{MessageFormatSymbol.NEWLINE}{MessageFormatSymbol.BOLD_START}Database:{MessageFormatSymbol.BOLD_END}{MessageFormatSymbol.NEWLINE}" +
                $"Server: 10.40.40.100 {MessageFormatSymbol.NEWLINE}" +
                $"DbName: DBACC.bodb02.BODBDownlineNet {MessageFormatSymbol.NEWLINE}" +
                $"UserID: bodbDownlineNet {MessageFormatSymbol.NEWLINE}" +
                $"SpName: Acc_StatementSelTransfer {MessageFormatSymbol.NEWLINE}" +
                $"Parameters: @winlostdate=6/17/2018 12:00:00 AM;@custid=26570707 {MessageFormatSymbol.NEWLINE}" +
                $"Line: 0 {MessageFormatSymbol.NEWLINE}" +
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
                $"{MessageFormatSymbol.BOLD_START}Custom Info:{MessageFormatSymbol.BOLD_END} {MessageFormatSymbol.NEWLINE}" +
                $"CustId: 26570707 {MessageFormatSymbol.NEWLINE}" +
                $"CustName: IX1388 {MessageFormatSymbol.NEWLINE}" +
                $"SubAccountName: IX1388SUB03 {MessageFormatSymbol.NEWLINE}" +
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
            var expecteExceptionInfo = $"{MessageFormatSymbol.BOLD_START}Exception:{MessageFormatSymbol.BOLD_END} {MessageFormatSymbol.NEWLINE}" +
                $"Source: Fanex.Data.CrossCutting.WrappingException {MessageFormatSymbol.NEWLINE}" +
                $"Type: Fanex.Data.CrossCutting.WrappingException.ObjectDbException {MessageFormatSymbol.NEWLINE}" +
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
                   $"{MessageFormatSymbol.BOLD_START}Session Info:{MessageFormatSymbol.BOLD_END}{MessageFormatSymbol.NEWLINE}" +
                   $"Username: starix69 {MessageFormatSymbol.NEWLINE}" +
                   $"AccUserName: starix69 {MessageFormatSymbol.NEWLINE}" +
                   $"CustRoleId: 4 {MessageFormatSymbol.NEWLINE}" +
                   $"custid: 27671895 {MessageFormatSymbol.NEWLINE}" +
                   $"CustUname: supertestPRO2 {MessageFormatSymbol.NEWLINE}" +
                   $"MemberID: 0 {MessageFormatSymbol.NEWLINE}" +
                   $"MemberUserName: {MessageFormatSymbol.NEWLINE}" +
                   $"AgentID: 0 {MessageFormatSymbol.NEWLINE}" +
                   $"AgentUserName: {MessageFormatSymbol.NEWLINE}" +
                   $"MasterID: 0 {MessageFormatSymbol.NEWLINE}" +
                   $"MasterUserName: {MessageFormatSymbol.NEWLINE}" +
                   $"SuperID: 27671895 {MessageFormatSymbol.NEWLINE}" +
                   $"SusperUserName: supertestPRO2 {MessageFormatSymbol.NEWLINE}" +
                   $"IsSyncCSCurrentCust: False {MessageFormatSymbol.NEWLINE}" +
                   $"IsInternal: False {MessageFormatSymbol.NEWLINE}" +
                   $"sitename: BEST-ODDS {MessageFormatSymbol.NEWLINE}";
            Assert.Contains(expectedSessionInfo, actualMessage);
        }

        private string FinalizeMessage(string message, Core.Log.Models.Log log)
        {
            var returnMessage = message
                    .Replace("\r", string.Empty)
                    .Replace("\t", string.Empty)
                    .Replace("Timestamp", $"{MessageFormatSymbol.BOLD_START}Timestamp{MessageFormatSymbol.BOLD_END}")
                    .Replace("Message", $"{MessageFormatSymbol.BOLD_START}Message{MessageFormatSymbol.BOLD_END}");

            return $"{MessageFormatSymbol.BOLD_START}Category{MessageFormatSymbol.BOLD_END}: {log.CategoryName}{MessageFormatSymbol.NEWLINE}" +
                    $"{WebUtility.HtmlDecode(returnMessage)}{MessageFormatSymbol.NEWLINE}" +
                    $"{MessageFormatSymbol.BOLD_START}#Log Id{MessageFormatSymbol.BOLD_END}: {log.LogId} " +
                    $"{MessageFormatSymbol.BOLD_START}Count{MessageFormatSymbol.BOLD_END}: " +
                    $"{log.NumMessage}{MessageFormatSymbol.DOUBLE_NEWLINE}{MessageFormatSymbol.DIVIDER}";
        }

        private string GetNewLogDataTest()
            => File.ReadAllText($"{AppDomain.CurrentDomain.BaseDirectory}/../../../Data/Log.txt");

        private string GetNormalLogDataTest()
            => File.ReadAllText($"{AppDomain.CurrentDomain.BaseDirectory}/../../../Data/NormalLog.txt");
    }
}