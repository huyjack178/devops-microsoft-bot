﻿namespace Fanex.Bot.Tests.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;
    using System.Threading.Tasks;
    using Fanex.Bot.Dialogs;
    using Fanex.Bot.Dialogs.Impl;
    using Fanex.Bot.Models;
    using Fanex.Bot.Models.Log;
    using Fanex.Bot.Services;
    using Fanex.Bot.Skynex.Models.Log;
    using Fanex.Bot.Tests.Fixtures;
    using Hangfire;
    using Hangfire.Common;
    using Hangfire.States;
    using Microsoft.Bot.Connector;
    using Microsoft.Extensions.Caching.Memory;
    using NSubstitute;
    using Xunit;

    public class LogDialogTests : IClassFixture<BotConversationFixture>
    {
        private readonly BotConversationFixture _conversationFixture;
        private readonly ILogDialog _logDialog;
        private readonly ILogService _logService;
        private readonly IRecurringJobManager _recurringJobManager;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IMemoryCache _memoryCache;

        public LogDialogTests(BotConversationFixture conversationFixture)
        {
            _conversationFixture = conversationFixture;
            _logService = Substitute.For<ILogService>();
            _recurringJobManager = Substitute.For<IRecurringJobManager>();
            _backgroundJobClient = Substitute.For<IBackgroundJobClient>();
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
            _logDialog = new LogDialog(
                _conversationFixture.Configuration,
                _logService,
                _conversationFixture.MockDbContext(),
                _conversationFixture.Conversation,
                _recurringJobManager,
                _backgroundJobClient,
                _memoryCache);
        }

        #region AddCategory

        [Fact]
        public async Task HandleMessageAsync_AddCategory_CategoriesIsEmpty_SendErrorMessage()
        {
            // Arrange
            var message = "log add";

            // Act
            await _logDialog.HandleMessageAsync(_conversationFixture.Activity, message);

            // Assert
            await _conversationFixture
                .Conversation
                .Received()
                .SendAsync(
                    Arg.Is(_conversationFixture.Activity),
                    Arg.Is("You need to add [LogCategory], otherwise, you will not get any log info"));
        }

        [Fact]
        public async Task HandleMessageAsync_AddCategory_IsDisableAddAndNotAdmin_SendErrorMessage()
        {
            // Arrange
            var message = "log add alpha";

            _conversationFixture.Configuration
                .GetSection("LogInfo").GetSection("DisableAddCategories").Value
                .Returns("true");
            _conversationFixture.Activity.Conversation.Returns(new ConversationAccount { Id = "3" });
            _conversationFixture.InitDbContextData();

            // Act
            await _logDialog.HandleMessageAsync(_conversationFixture.Activity, message);

            // Assert
            await _conversationFixture
                .Conversation
                .Received()
                .SendAsync(
                    Arg.Is(_conversationFixture.Activity),
                    Arg.Is("Add log categories is disabled, please contact NexOps."));
        }

        [Fact]
        public async Task HandleMessageAsync_AddCategory_IsDisableAddButAdmin_SendSuccessMessage()
        {
            // Arrange
            var message = "log add alpha";

            _conversationFixture.Configuration
                .GetSection("LogInfo").GetSection("DisableAddCategories").Value
                .Returns("true");
            _conversationFixture.Activity.Conversation.Returns(new ConversationAccount { Id = "2" });
            _conversationFixture.InitDbContextData();

            // Act
            await _logDialog.HandleMessageAsync(_conversationFixture.Activity, message);

            // Assert
            Assert.Equal(
                "alpha;",
                _conversationFixture
                    .BotDbContext
                    .LogInfo
                    .FirstOrDefault(info => info.ConversationId == "2")
                    .LogCategories);
            await _conversationFixture
                .Conversation
                .Received()
                .SendAsync(
                    Arg.Is(_conversationFixture.Activity),
                    Arg.Is($"You will receive log with categories contain **[alpha]**"));
        }

        [Fact]
        public async Task HandleMessageAsync_AddCategory_IsEnableAdd_SendSuccessMessage()
        {
            // Arrange
            var message = "log add alpha";

            _conversationFixture.Configuration
                .GetSection("LogInfo").GetSection("DisableAddCategories").Value
                .Returns("false");
            _conversationFixture.Activity.Conversation.Returns(new ConversationAccount { Id = "5555" });

            // Act
            await _logDialog.HandleMessageAsync(_conversationFixture.Activity, message);

            // Assert
            Assert.Equal(
                "alpha;",
                _conversationFixture
                    .BotDbContext
                    .LogInfo
                    .FirstOrDefault(info => info.ConversationId == "5555")
                    .LogCategories);
            await _conversationFixture
                .Conversation
                .Received()
                .SendAsync(
                    Arg.Is(_conversationFixture.Activity),
                    Arg.Is($"You will receive log with categories contain **[alpha]**"));
        }

        #endregion AddCategory

        #region Remove Category

        [Fact]
        public async Task HandleMessageAsync_RemoveCategory_CategoriesIsEmpty_SendErrorMessage()
        {
            // Arrange
            var message = "log remove";

            // Act
            await _logDialog.HandleMessageAsync(_conversationFixture.Activity, message);

            // Assert
            await _conversationFixture
                .Conversation
                .Received()
                .SendAsync(
                    Arg.Is(_conversationFixture.Activity),
                    Arg.Is("You need to add [LogCategory], otherwise, you will not get any log info"));
        }

        [Fact]
        public async Task HandleMessageAsync_RemoveCategory_NotFoundLogInfo_SendErrorMessage()
        {
            // Arrange
            var message = "log remove alpha";
            _conversationFixture.Activity.Conversation.Returns(new ConversationAccount { Id = "55" });

            // Act
            await _logDialog.HandleMessageAsync(_conversationFixture.Activity, message);

            // Assert
            await _conversationFixture
                .Conversation
                .Received()
                .SendAsync(
                    Arg.Is(_conversationFixture.Activity),
                    Arg.Is("You don't have any log categories data"));
        }

        [Fact]
        public async Task HandleMessageAsync_RemoveCategory_SendSuccessMessage()
        {
            // Arrange
            var message = "log remove nap";
            _conversationFixture.Activity.Conversation.Returns(new ConversationAccount { Id = "10" });
            _conversationFixture.InitDbContextData();

            // Act
            await _logDialog.HandleMessageAsync(_conversationFixture.Activity, message);

            // Assert
            Assert.Equal(
             ";alpha",
             _conversationFixture
                 .BotDbContext
                 .LogInfo
                 .FirstOrDefault(info => info.ConversationId == "10")
                 .LogCategories);
            await _conversationFixture
                .Conversation
                .Received()
                .SendAsync(
                    Arg.Is(_conversationFixture.Activity),
                    Arg.Is($"You will not receive log with categories contain **[nap]**"));
        }

        #endregion Remove Category

        [Fact]
        public async Task HandleMessageAsync_StartLogging_SendSuccessMessage()
        {
            // Arrange
            var message = "log start";
            _conversationFixture.Activity.Conversation.Returns(new ConversationAccount { Id = "5" });
            _memoryCache.Set("5", "1234");

            // Act
            await _logDialog.HandleMessageAsync(_conversationFixture.Activity, message);

            // Assert
            Assert.True(_conversationFixture
                    .BotDbContext
                    .LogInfo
                    .FirstOrDefault(info => info.ConversationId == "5")
                    .IsActive);

            _backgroundJobClient.Received().ChangeState("1234", Arg.Any<DeletedState>(), null);

            _recurringJobManager.Received()
              .AddOrUpdate(
                  Arg.Is("NotifyLogPeriodically"),
                  Arg.Any<Job>(),
                  Cron.Minutely(),
                  Arg.Any<RecurringJobOptions>());

            await _conversationFixture
              .Conversation
              .Received()
              .SendAsync(
                  Arg.Is(_conversationFixture.Activity),
                  Arg.Is($"Log has been started!"));
        }

        [Fact]
        public async Task HandleMessageAsync_StopLogging_WithDefaultStopTime_SendSuccessMessage()
        {
            // Arrange
            var message = "log stop";
            _conversationFixture.Activity.Conversation.Returns(new ConversationAccount { Id = "6" });
            _memoryCache.Set("6", "1234");

            // Act
            await _logDialog.HandleMessageAsync(_conversationFixture.Activity, message);

            // Assert
            Assert.False(_conversationFixture
                    .BotDbContext
                    .LogInfo
                    .FirstOrDefault(info => info.ConversationId == "6")
                    .IsActive);

            // Remove old job
            _backgroundJobClient.Received().ChangeState("1234", Arg.Any<DeletedState>(), null);

            Assert.Equal("", _memoryCache.Get("6"));

            await _conversationFixture
              .Conversation
              .Received()
              .SendAsync(
                  Arg.Is(_conversationFixture.Activity),
                  Arg.Is($"Log has been stopped for 10 minutes"));
        }

        [Fact]
        public async Task HandleMessageAsync_StopLogging_WithStopTime_SendSuccessMessage()
        {
            // Arrange
            var message = "log stop 3h";
            _conversationFixture.Activity.Conversation.Returns(new ConversationAccount { Id = "126" });

            // Act
            await _logDialog.HandleMessageAsync(_conversationFixture.Activity, message);

            // Assert
            Assert.False(_conversationFixture
               .BotDbContext
               .LogInfo
               .FirstOrDefault(info => info.ConversationId == "126")
               .IsActive);

            await _conversationFixture
              .Conversation
              .Received()
              .SendAsync(
                  Arg.Is(_conversationFixture.Activity),
                  Arg.Is($"Log has been stopped for 3 hours"));
        }

        [Fact]
        public async Task HandleMessageAsync_RestartLogging_FoundInfo_SendSuccessMessage()
        {
            // Arrange
            var dbContext = _conversationFixture.MockDbContext();
            dbContext.LogInfo.Add(new LogInfo { ConversationId = "666", LogCategories = "alpha;nap", IsActive = false });
            dbContext.MessageInfo.Add(new MessageInfo { ConversationId = "666" });
            dbContext.SaveChanges();
            _conversationFixture.Activity.Conversation.Returns(new ConversationAccount { Id = "666" });

            // Act
            await _logDialog.RestartNotifyingLog(_conversationFixture.Activity.Conversation.Id);

            // Assert
            Assert.True(_conversationFixture
              .BotDbContext
              .LogInfo
              .FirstOrDefault(info => info.ConversationId == "666")
              .IsActive);

            await _conversationFixture
              .Conversation
              .Received()
              .SendAsync(Arg.Is<MessageInfo>(info => info.Text == "Log has been restarted!"));
        }

        [Fact]
        public async Task GetAndSendLogAsync_HasNoMessageInfo_DontSendMesage()
        {
            // Arrange
            var dbContext = _conversationFixture.MockDbContext();
            dbContext.LogInfo.Add(new LogInfo { ConversationId = "9643534", LogCategories = "alpha;nap", IsActive = true });
            dbContext.SaveChanges();
            _logService.GetErrorLogs().Returns(new List<Log>(){
                new Log {
                    Category = new LogCategory { CategoryName = "alpha" },
                    Machine = new Machine { MachineIP = "machine" },
                    FormattedMessage = "log"
                },
                new Log {
                    Category = new LogCategory { CategoryName = "alpha" },
                    Machine = new Machine { MachineIP = "machine" },
                    FormattedMessage = "log"
                },
                new Log {
                    Category = new LogCategory { CategoryName = "lrf" },
                    Machine = new Machine { MachineIP = "machine" },
                    FormattedMessage = "log_lrf"
                }
            });

            // Act
            await _logDialog.GetAndSendLogAsync();

            // Assert
            var expectedAlphaMessage = "**Category**: alpha\n\nlog\n\n**#Log Id**: 0 **Count**: 0\n\n\n\n====================================\n\n";
            await _conversationFixture.Conversation
                 .DidNotReceive()
                 .SendAsync(Arg.Is<MessageInfo>(info =>
                    info.Text == expectedAlphaMessage &&
                    info.ConversationId == "96435341"));
        }

        [Fact]
        public async Task GetAndSendLogAsync_HasLogInfoData_IsActive_HasLogCategory_SendLogToClient()
        {
            // Arrange
            _conversationFixture.InitDbContextData();
            var dbContext = _conversationFixture.MockDbContext();
            dbContext.LogInfo.Add(new LogInfo { ConversationId = "1", LogCategories = "alpha;nap", IsActive = true });
            dbContext.SaveChanges();
            _logService.GetErrorLogs().Returns(new List<Log>(){
                new Log {
                    Category = new LogCategory { CategoryName = "alpha" },
                    Machine = new Machine { MachineIP = "machine" },
                    FormattedMessage = "log"
                },
                new Log {
                    Category = new LogCategory { CategoryName = "alpha" },
                    Machine = new Machine { MachineIP = "machine" },
                    FormattedMessage = "log"
                },
                new Log {
                    Category = new LogCategory { CategoryName = "lrf" },
                    Machine = new Machine { MachineIP = "machine" },
                    FormattedMessage = "log_lrf"
                }
            });

            // Act
            await _logDialog.GetAndSendLogAsync();

            // Assert
            var expectedAlphaMessage = "**Category**: alpha\n\nlog\n\n**#Log Id**: 0 **Count**: 0\n\n\n\n====================================\n\n";
            await _conversationFixture.Conversation
                 .Received(1)
                 .SendAsync(Arg.Is<MessageInfo>(info =>
                    info.Text == expectedAlphaMessage &&
                    info.ConversationId == "1"));

            var expectedLrfMessage = "**Category**: lrf\n\nlog_lrf\n\n**#Log Id**: 0 **Count**: 0\n\n\n\n====================================\n\n";
            await _conversationFixture.Conversation
                 .DidNotReceive()
                 .SendAsync(Arg.Is<MessageInfo>(info =>
                    info.Text == expectedLrfMessage &&
                    info.ConversationId == "1"));
        }

        [Fact]
        public async Task GetAndSendLogAsync_HasLogInfoData_IsActive_HasLogCategory_HasIgnoreMessage_NotSendLogToClient()
        {
            // Arrange
            _conversationFixture.InitDbContextData();
            var dbContext = _conversationFixture.MockDbContext();
            dbContext.MessageInfo.Add(new MessageInfo { ConversationId = "2342342342311cd1" });
            dbContext.LogInfo.Add(new LogInfo { ConversationId = "2342342342311cd1", LogCategories = "alpha;nap", IsActive = true });
            dbContext.LogIgnoreMessage.Add(new LogIgnoreMessage { Category = "alpha", IgnoreMessage = "thread was being aborted" });
            dbContext.SaveChanges();
            _logService.GetErrorLogs().Returns(new List<Log>(){
                new Log {
                    Category = new LogCategory { CategoryName = "alpha" },
                    Machine = new Machine { MachineIP = "machine" },
                    FormattedMessage = "thread was being aborted"
                },
            });

            // Act
            await _logDialog.GetAndSendLogAsync();

            // Assert
            var expectedAlphaMessage = "**Category**: alpha\n\nthread was being aborted\n\n**#Log Id**: 0 **Count**: 0\n\n\n\n====================================\n\n";
            await _conversationFixture.Conversation
                 .DidNotReceive()
                 .SendAsync(Arg.Is<MessageInfo>(info =>
                    info.Text == expectedAlphaMessage &&
                    info.ConversationId == "2342342342311cd1"));
        }

        [Fact]
        public async Task GetAndSendLogAsync_HasLogInfoData_IsActive_HasLogCategory_HasNoIgnoreMessage_NotSendLogToClient()
        {
            // Arrange
            _conversationFixture.InitDbContextData();
            var dbContext = _conversationFixture.MockDbContext();
            dbContext.MessageInfo.Add(new MessageInfo { ConversationId = "234234231" });
            dbContext.LogInfo.Add(new LogInfo { ConversationId = "234234231", LogCategories = "alpha;nap", IsActive = true });
            dbContext.LogIgnoreMessage.Add(new LogIgnoreMessage { Category = "nap", IgnoreMessage = "thread was being aborted" });
            dbContext.SaveChanges();
            _logService.GetErrorLogs().Returns(new List<Log>(){
                new Log {
                    Category = new LogCategory { CategoryName = "nap" },
                    Machine = new Machine { MachineIP = "machine" },
                    FormattedMessage = "thread was not being aborted"
                },
            });

            // Act
            await _logDialog.GetAndSendLogAsync();

            // Assert
            var expectedAlphaMessage = "**Category**: nap\n\nthread was not being aborted\n\n**#Log Id**: 0 **Count**: 0\n\n\n\n====================================\n\n";
            await _conversationFixture.Conversation
                 .Received(1)
                 .SendAsync(Arg.Is<MessageInfo>(info =>
                    info.Text == expectedAlphaMessage &&
                    info.ConversationId == "234234231"));
        }

        [Theory]
        [InlineData("9", true, "Running")]
        [InlineData("8", false, "Stopped")]
        public async Task HandleMessageAsync_ViewStatus_SendLogInfoMessage(string conversationId, bool isActive, string expectedActiveResult)
        {
            // Arrange
            var message = "log status";
            var dbContext = _conversationFixture.MockDbContext();
            dbContext.LogInfo.Add(new LogInfo { ConversationId = conversationId, LogCategories = "alpha;nap", IsActive = isActive });
            dbContext.SaveChanges();
            _conversationFixture.Activity.Conversation.Returns(new ConversationAccount { Id = conversationId });

            // Act
            await _logDialog.HandleMessageAsync(_conversationFixture.Activity, message);

            // Assert
            await _conversationFixture
              .Conversation
              .Received()
              .SendAsync(
                  Arg.Is(_conversationFixture.Activity),
                  Arg.Is($"Your log status \n\n**Log Categories:** [alpha;nap]\n\n**{expectedActiveResult}**\n\n"));
        }

        [Fact]
        public async Task HandleMessageAsync_LogDetail_GetLogDetailAsync_LogIdIsNull_SendErrorMessage()
        {
            // Arrange
            var message = "log detail";

            // Act
            await _logDialog.HandleMessageAsync(_conversationFixture.Activity, message);

            // Assert
            await _conversationFixture
              .Conversation
              .Received()
              .SendAsync(
                  Arg.Is(_conversationFixture.Activity),
                  Arg.Is("I need [LogId]."));
        }

        [Fact]
        public async Task HandleMessageAsync_LogDetail_GetLogDetailAsync_SendLogDetailMessage()
        {
            // Arrange
            var message = "log detail 23423";
            _logService.GetErrorLogDetail(Arg.Is<long>(23423)).Returns(
                new Log
                {
                    Category = new LogCategory { CategoryName = "alpha" },
                    FormattedMessage = "log",
                });

            // Act
            await _logDialog.HandleMessageAsync(_conversationFixture.Activity, message);

            // Assert
            var expectedLogMesasge = "**Category**: alpha\n\nlog\n\n**#Log Id**: 0 **Count**: 0\n\n\n\n====================================\n\n";
            await _conversationFixture
              .Conversation
              .Received()
              .SendAsync(
                  Arg.Is(_conversationFixture.Activity),
                  Arg.Is(expectedLogMesasge));
        }

        [Fact]
        public async Task HandleMessageAsync_AnyMessage_SendCommandMessage()
        {
            // Arrange
            var message = "log";
            _conversationFixture.Activity.Conversation.Returns(new ConversationAccount { Id = "669" });

            // Act
            await _logDialog.HandleMessageAsync(_conversationFixture.Activity, message);

            // Assert
            await _conversationFixture
                .Conversation
                .Received()
                .SendAsync(Arg.Is(_conversationFixture.Activity), Arg.Is(_conversationFixture.CommandMessage));
        }
    }
}