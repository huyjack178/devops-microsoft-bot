using AutoFixture;
using Fanex.Bot.Models.Log;
using Fanex.Bot.Skynex.MessageHandlers.MessageBuilders;
using NSubstitute;
using System;
using System.IO;
using System.Net;
using Xunit;

namespace Fanex.Bot.Skynex.Tests.MessageHandlers.MessageBuilders
{
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
            var log = fixture.Create<Log>();
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
            var log = fixture.Create<Log>();
            log.FormattedMessage = logMessage;

            // Act
            var actualMessage = webLogMessageBuilder.BuildMessage(log);

            // Assert
            var expectedLogMessage = FinalizeMessage(logMessage, log);
            Assert.Equal(expectedLogMessage, actualMessage);
        }

        [Fact]
        public void BuildMessage_IsNewLogType_IsNotAlpha_GetRequestInfo()
        {
            // Arrange
            var fixture = new Fixture();
            var log = fixture.Create<Log>();
            log.FormattedMessage = GetLogDataTest();

            // Act
            var actualMessage = webLogMessageBuilder.BuildMessage(log);

            // Assert
            var expecteRequestInfo =
                $"{MessageFormatSignal.BeginBold}Request:{MessageFormatSignal.EndBold}" +
                "  http://mb.stakecity.com:8072/site-reports/Statement/";
            Assert.Contains(expecteRequestInfo, actualMessage);
        }

        [Fact]
        public void BuildMessage_IsNewLogType_IsAlpha_GetRequestInfo()
        {
            // Arrange
            var fixture = new Fixture();
            var log = fixture.Create<Log>();
            log.CategoryName = "alpha";
            log.FormattedMessage = GetLogDataTest();

            // Act
            var actualMessage = webLogMessageBuilder.BuildMessage(log);

            // Assert
            var expecteRequestInfo =
                $"{MessageFormatSignal.BeginBold}Request:{MessageFormatSignal.EndBold}" +
                $" http://alpha.site/site-reports/Statement/";
            Assert.Contains(expecteRequestInfo, actualMessage);
        }

        [Fact]
        public void BuildMessage_IsNewLogType_GetBrowserInfo()
        {
            // Arrange
            var fixture = new Fixture();
            var log = fixture.Create<Log>();
            log.FormattedMessage = GetLogDataTest();

            // Act
            var actualMessage = webLogMessageBuilder.BuildMessage(log);

            // Assert
            var expecteRequestInfo =
                $"{MessageFormatSignal.BeginBold}Browser:{MessageFormatSignal.EndBold}" +
                $"  Firefox 60.0 (Beta: False) {MessageFormatSignal.NewLine} MobileDeviceModel: Unknown";
            Assert.Contains(expecteRequestInfo, actualMessage);
        }

        [Fact]
        public void BuildMessage_IsNewLogType_GetServerAndDatabaseInfo()
        {
            // Arrange
            var fixture = new Fixture();
            var log = fixture.Create<Log>();
            log.FormattedMessage = GetLogDataTest();

            // Act
            var actualMessage = webLogMessageBuilder.BuildMessage(log);

            // Assert
            var expecteDbInfo = $"{MessageFormatSignal.BeginBold}Server:{MessageFormatSignal.EndBold}" +
                $" {log.MachineName} ({log.MachineIP})" +
                $"{MessageFormatSignal.NewLine}{MessageFormatSignal.BeginBold}Database:{MessageFormatSignal.EndBold}{MessageFormatSignal.NewLine}" +
                $"Server: 10.40.40.100 {MessageFormatSignal.NewLine}" +
                $"DbName: DBACC.bodb02.BODBDownlineNet {MessageFormatSignal.NewLine}" +
                $"UserID: bodbDownlineNet {MessageFormatSignal.NewLine}" +
                $"SpName: Acc_StatementSelTransfer {MessageFormatSignal.NewLine}" +
                $"Parameters: @winlostdate=6/17/2018 12:00:00 AM;@custid=26570707 {MessageFormatSignal.NewLine}" +
                $"Line: 0 {MessageFormatSignal.NewLine}" +
                $"CommandTimeout: 120";
            Assert.Contains(expecteDbInfo, actualMessage);
        }

        [Fact]
        public void BuildMessage_IsNewLogType_GetCustomInfo()
        {
            // Arrange
            var fixture = new Fixture();
            var log = fixture.Create<Log>();
            log.FormattedMessage = GetLogDataTest();

            // Act
            var actualMessage = webLogMessageBuilder.BuildMessage(log);

            // Assert
            var expectedCustomInfo =
                $"{MessageFormatSignal.BeginBold}Custom Info:{MessageFormatSignal.EndBold} {MessageFormatSignal.NewLine}" +
                $"CustId: 26570707 {MessageFormatSignal.NewLine}" +
                $"CustName: IX1388 {MessageFormatSignal.NewLine}" +
                $"SubAccountName: IX1388SUB03 {MessageFormatSignal.NewLine}" +
                $"AdminName: ";
            Assert.Contains(expectedCustomInfo, actualMessage);
        }

        [Fact]
        public void BuildMessage_IsNewLogType_GetExceptionInfo()
        {
            // Arrange
            var fixture = new Fixture();
            var log = fixture.Create<Log>();
            log.FormattedMessage = GetLogDataTest();

            // Act
            var actualMessage = webLogMessageBuilder.BuildMessage(log);

            // Assert
            var expecteExceptionInfo = $"{MessageFormatSignal.BeginBold}Exception:{MessageFormatSignal.EndBold} {MessageFormatSignal.NewLine}" +
                $"Source: Fanex.Data.CrossCutting.WrappingException {MessageFormatSignal.NewLine}" +
                $"Type: Fanex.Data.CrossCutting.WrappingException.ObjectDbException {MessageFormatSignal.NewLine}" +
                $"TargetSite: System.Collections.Generic.IEnumerable`1[TReturn] Query[TReturn](System.Object, System.Data.Common.DbTransaction, Boolean, System.Nullable`1[System.Int32]) ";
            Assert.Contains(expecteExceptionInfo, actualMessage);
        }

        [Fact]
        public void Message_IsNewLogType_GetSessionInfo()
        {
            // Arrange
            var fixture = new Fixture();
            var log = fixture.Create<Log>();
            log.FormattedMessage = GetLogDataTest();

            // Act
            var actualMessage = webLogMessageBuilder.BuildMessage(log);

            // Assert
            var expectedSessionInfo =
                   $"{MessageFormatSignal.BeginBold}Session Info:{MessageFormatSignal.EndBold}{MessageFormatSignal.NewLine}" +
                   $"Username: starix69 {MessageFormatSignal.NewLine}" +
                   $"AccUserName: starix69 {MessageFormatSignal.NewLine}" +
                   $"CustRoleId: 4 {MessageFormatSignal.NewLine}" +
                   $"custid: 27671895 {MessageFormatSignal.NewLine}" +
                   $"CustUname: supertestPRO2 {MessageFormatSignal.NewLine}" +
                   $"MemberID: 0 {MessageFormatSignal.NewLine}" +
                   $"MemberUserName: {MessageFormatSignal.NewLine}" +
                   $"AgentID: 0 {MessageFormatSignal.NewLine}" +
                   $"AgentUserName: {MessageFormatSignal.NewLine}" +
                   $"MasterID: 0 {MessageFormatSignal.NewLine}" +
                   $"MasterUserName: {MessageFormatSignal.NewLine}" +
                   $"SuperID: 27671895 {MessageFormatSignal.NewLine}" +
                   $"SusperUserName: supertestPRO2 {MessageFormatSignal.NewLine}" +
                   $"IsSyncCSCurrentCust: False {MessageFormatSignal.NewLine}" +
                   $"IsInternal: False {MessageFormatSignal.NewLine}" +
                   $"sitename: BEST-ODDS {MessageFormatSignal.NewLine}";
            Assert.Contains(expectedSessionInfo, actualMessage);
        }

        private string FinalizeMessage(string message, Log log)
        {
            var returnMessage = message
                    .Replace("\r", string.Empty)
                    .Replace("\t", string.Empty)
                    .Replace("Timestamp", $"{MessageFormatSignal.BeginBold}Timestamp{MessageFormatSignal.EndBold}")
                    .Replace("Message", $"{MessageFormatSignal.BeginBold}Message{MessageFormatSignal.EndBold}");

            return $"{MessageFormatSignal.BeginBold}Category{MessageFormatSignal.EndBold}: {log.CategoryName}{MessageFormatSignal.NewLine}" +
                    $"{WebUtility.HtmlDecode(returnMessage)}{MessageFormatSignal.NewLine}" +
                    $"{MessageFormatSignal.BeginBold}#Log Id{MessageFormatSignal.EndBold}: {log.LogId} " +
                    $"{MessageFormatSignal.BeginBold}Count{MessageFormatSignal.EndBold}: " +
                    $"{log.NumMessage}{MessageFormatSignal.DoubleNewLine}{MessageFormatSignal.BreakLine}";
        }

        private string GetLogDataTest()
            => File.ReadAllText($"{AppDomain.CurrentDomain.BaseDirectory}/../../../Data/Log.txt");
    }
}