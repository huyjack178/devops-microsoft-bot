namespace Fanex.Bot.Skynex.Tests.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Fanex.Bot.Models;
    using Fanex.Bot.Models.Log;
    using Fanex.Bot.Services;
    using Fanex.Bot.Skynex.Dialogs;
    using Fanex.Bot.Skynex.MessageHandlers.MessageBuilders;
    using Fanex.Bot.Skynex.Tests.Fixtures;
    using Hangfire;
    using Hangfire.Common;
    using Microsoft.Bot.Connector;
    using NSubstitute;
    using Xunit;

    public class DBLogDialogTests : IClassFixture<BotConversationFixture>
    {
        private readonly BotConversationFixture conversationFixture;
        private readonly BotDbContext subBotDbContext;
        private readonly ILogService subLogService;
        private readonly IRecurringJobManager subRecurringJobManager;
        private readonly IDBLogMessageBuilder subDBLogMessageBuilder;
        private readonly IDBLogDialog dbLogDialog;

        public DBLogDialogTests(BotConversationFixture conversationFixture)
        {
            this.conversationFixture = conversationFixture;
            subBotDbContext = Substitute.For<BotDbContext>();
            subLogService = Substitute.For<ILogService>();
            subRecurringJobManager = Substitute.For<IRecurringJobManager>();
            subDBLogMessageBuilder = new DBLogMessageBuilder();

            dbLogDialog = new DBLogDialog(
                subBotDbContext,
                conversationFixture.Conversation,
                subLogService,
                subRecurringJobManager,
                subDBLogMessageBuilder);
        }

        [Fact]
        public async Task HandleMessage_StartCommand_StartNotifyDBLog()
        {
            // Arrange
            var command = "dblog start";

            // Act
            await dbLogDialog.HandleMessage(conversationFixture.Activity, command);

            // Assert

            subRecurringJobManager.Received()
              .AddOrUpdate(
                  Arg.Is("NotifyDbLogPeriodically"),
                  Arg.Any<Job>(),
                  Cron.Minutely(),
                  Arg.Any<RecurringJobOptions>());

            await conversationFixture
              .Conversation
              .Received()
              .ReplyAsync(
                  Arg.Is(conversationFixture.Activity),
                  Arg.Is("DBLog has been started!"));
        }

        [Fact]
        public async Task GetAndSendLogAsync_DBLogsIsNullOrEmptyList_Return()
        {
            // Arrange
            var dblogs = new List<DBLog>();
            subLogService.GetDBLogs().Returns(dblogs);

            // Act
            await dbLogDialog.GetAndSendLogAsync();

            // Assert
            await conversationFixture.Conversation.DidNotReceive().SendAsync(Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task GetAndSendLogAsync_HasDbLogsData_NotSimpleMessage_SendLog()
        {
            // Arrange
            var dblogs = new List<DBLog>
            {
                new DBLog {
                    SkypeGroupId = "123456",
                    MsgInfo = "Log message",
                    NotificationId = 1,
                    Title = "Message Log",
                    ServerName = "Server 1",
                    LogDate = new DateTime(2018, 01, 01)
                }
            };
            subLogService.GetDBLogs().Returns(dblogs);
            var logMessage = "{{BeginBold}}Server:{{EndBold}} Server 1{{NewLine}}{{BeginBold}}Title:{{EndBold}} Message Log{{NewLine}}{{BeginBold}}DateTime:{{EndBold}} 1/1/2018 12:00:00 AM{{DoubleNewLine}}Log message{{NewLine}}{{BreakLine}}";
            conversationFixture.Conversation.SendAsync("123456", logMessage).Returns(Result.CreateSuccessfulResult());

            // Act
            await dbLogDialog.GetAndSendLogAsync();

            // Assert
            await subLogService.Received().AckDBLog(Arg.Is<int[]>(ids => ids.ToList().Contains(1)));
        }

        [Fact]
        public async Task GetAndSendLogAsync_HasDbLogsData_SimpleMessage_SendLog()
        {
            // Arrange
            var dblogs = new List<DBLog>
            {
                new DBLog {
                    SkypeGroupId = "123456",
                    MsgInfo = "Log message",
                    NotificationId = 1,
                    Title = "Message Log",
                    ServerName = "Server 1",
                    LogDate = new DateTime(2018, 01, 01),
                    IsSimple = true
                }
            };
            subLogService.GetDBLogs().Returns(dblogs);
            var logMessage = "Log message{{NewLine}}{{BreakLine}}";
            conversationFixture.Conversation.SendAsync("123456", logMessage).Returns(Result.CreateSuccessfulResult());

            // Act
            await dbLogDialog.GetAndSendLogAsync();

            // Assert
            await subLogService.Received().AckDBLog(Arg.Is<int[]>(ids => ids.ToList().Contains(1)));
        }
    }
}