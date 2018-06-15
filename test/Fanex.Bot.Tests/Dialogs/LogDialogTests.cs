namespace Fanex.Bot.Tests.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fanex.Bot.Dialogs;
    using Fanex.Bot.Dialogs.Impl;
    using Fanex.Bot.Models;
    using Fanex.Bot.Models.Log;
    using Fanex.Bot.Services;
    using Fanex.Bot.Tests.Fixtures;
    using Hangfire;
    using Hangfire.Common;
    using Microsoft.Bot.Connector;
    using NSubstitute;
    using Xunit;

    public class LogDialogTests : IClassFixture<BotConversationFixture>
    {
        private readonly BotConversationFixture _conversationFixture;
        private readonly ILogDialog _logDialog;
        private readonly ILogService _logService;
        private readonly IRecurringJobManager _recurringJobManager;

        public LogDialogTests(BotConversationFixture conversationFixture)
        {
            _conversationFixture = conversationFixture;
            _logService = Substitute.For<ILogService>();
            _recurringJobManager = Substitute.For<IRecurringJobManager>();
            _logDialog = new LogDialog(
                _conversationFixture.Configuration,
                _logService,
                _conversationFixture.MockDbContext(),
                _conversationFixture.Conversation,
                _recurringJobManager);
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
            _conversationFixture.Activity.Conversation.Returns(new ConversationAccount { Id = "5" });

            // Act
            await _logDialog.HandleMessageAsync(_conversationFixture.Activity, message);

            // Assert
            Assert.Equal(
                "alpha;",
                _conversationFixture
                    .BotDbContext
                    .LogInfo
                    .FirstOrDefault(info => info.ConversationId == "5")
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

            // Act
            await _logDialog.HandleMessageAsync(_conversationFixture.Activity, message);

            // Assert
            Assert.True(_conversationFixture
                    .BotDbContext
                    .LogInfo
                    .FirstOrDefault(info => info.ConversationId == "5")
                    .IsActive);

            Assert.True(_conversationFixture
                    .BotDbContext
                    .MessageInfo
                    .Any(info => info.ConversationId == "5"));

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
                  Arg.Is($"Log will be sent to you soon!"));
        }

        [Fact]
        public async Task HandleMessageAsync_StopLogging_SendSuccessMessage()
        {
            // Arrange
            var message = "log stop";
            _conversationFixture.Activity.Conversation.Returns(new ConversationAccount { Id = "6" });

            // Act
            await _logDialog.HandleMessageAsync(_conversationFixture.Activity, message);

            // Assert
            Assert.False(_conversationFixture
                    .BotDbContext
                    .LogInfo
                    .FirstOrDefault(info => info.ConversationId == "6")
                    .IsActive);

            await _conversationFixture
              .Conversation
              .Received()
              .SendAsync(
                  Arg.Is(_conversationFixture.Activity),
                  Arg.Is($"Log will not be sent to you more!"));
        }

        [Fact]
        public async Task GetAndSendLogAsync_HasNoMessageInfo_DontSendMesage()
        {
            // Arrange
            var dbContext = _conversationFixture.MockDbContext();
            dbContext.LogInfo.Add(new LogInfo { ConversationId = "96", LogCategories = "alpha;nap", IsActive = true });
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
                    info.ConversationId == "1"));
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

        [Theory]
        [InlineData("9", true, "Running")]
        [InlineData("8", false, "Stopped")]
        public async Task HandleMessageAsync_ViewStatus_SendLogInfoMessage(string conversationId, bool isActive, string expectedActiveResult)
        {
            // Arrange
            var message = "log viewstatus";
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
        public async Task HandleMessageAsync_EnableNotifyingLogAllAsync_IsNotAdmin_SendErrorMessage()
        {
            // Arrange
            var message = "log adminstartall";
            _conversationFixture.Activity.Conversation.Returns(new ConversationAccount { Id = "333" });

            // Act
            await _logDialog.HandleMessageAsync(_conversationFixture.Activity, message);

            // Assert
            await _conversationFixture
              .Conversation
              .Received()
              .SendAsync(
                  Arg.Is(_conversationFixture.Activity),
                  Arg.Is("Sorry! You are not admin."));
        }

        [Fact]
        public async Task HandleMessageAsync_EnableNotifyingLogAllAsync_IsAdmin_SendSuccessMessage()
        {
            // Arrange
            var message = "log adminstartall";
            _conversationFixture.InitDbContextData();
            var dbContext = _conversationFixture.MockDbContext();
            dbContext.LogInfo.Add(new LogInfo { ConversationId = "223", LogCategories = "alpha;nap", IsActive = false });
            dbContext.SaveChanges();
            _conversationFixture.Activity.Conversation.Returns(new ConversationAccount { Id = "1" });
            // Act
            await _logDialog.HandleMessageAsync(_conversationFixture.Activity, message);

            // Assert

            Assert.True(_conversationFixture.BotDbContext.LogInfo.All(info => info.IsActive));
            await _conversationFixture
              .Conversation
              .Received()
              .SendAsync(
                  Arg.Is(_conversationFixture.Activity),
                  Arg.Is("Your request is accepted!"));
            await _conversationFixture
              .Conversation
              .Received()
              .SendAdminAsync(Arg.Is("All clients is active now!"));
        }

        [Fact]
        public async Task HandleMessageAsync_DisableNotifyingLogAllAsync_IsNotAdmin_SendErrorMessage()
        {
            // Arrange
            var message = "log adminstopall";
            _conversationFixture.Activity.Conversation.Returns(new ConversationAccount { Id = "444" });

            // Act
            await _logDialog.HandleMessageAsync(_conversationFixture.Activity, message);

            // Assert
            await _conversationFixture
              .Conversation
              .Received()
              .SendAsync(
                  Arg.Is(_conversationFixture.Activity),
                  Arg.Is("Sorry! You are not admin."));
        }

        [Fact]
        public async Task HandleMessageAsync_DisableNotifyingLogAllAsync_IsAdmin_SendSuccessMessage()
        {
            // Arrange
            var message = "log adminstopall";
            _conversationFixture.InitDbContextData();
            var dbContext = _conversationFixture.MockDbContext();
            dbContext.LogInfo.Add(new LogInfo { ConversationId = "224", LogCategories = "alpha;nap", IsActive = false });
            dbContext.SaveChanges();
            _conversationFixture.Activity.Conversation.Returns(new ConversationAccount { Id = "1" });
            // Act
            await _logDialog.HandleMessageAsync(_conversationFixture.Activity, message);

            // Assert

            Assert.False(_conversationFixture.BotDbContext.LogInfo.All(info => info.IsActive));
            await _conversationFixture
              .Conversation
              .Received()
              .SendAsync(
                  Arg.Is(_conversationFixture.Activity),
                  Arg.Is("Your request is accepted!"));
            await _conversationFixture
              .Conversation
              .Received()
              .SendAdminAsync(Arg.Is("All clients is inactive now!"));
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