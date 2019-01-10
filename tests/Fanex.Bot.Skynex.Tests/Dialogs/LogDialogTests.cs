namespace Fanex.Bot.Skynex.Tests.Dialogs
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Fanex.Bot.Models;
    using Fanex.Bot.Models.Log;
    using Fanex.Bot.Models.UM;
    using Fanex.Bot.Services;
    using Fanex.Bot.Skynex.Dialogs;
    using Fanex.Bot.Skynex.MessageHandlers.MessageBuilders;
    using Fanex.Bot.Skynex.Tests.Fixtures;
    using Hangfire;
    using Hangfire.Common;
    using Hangfire.States;
    using Microsoft.Bot.Connector;
    using Microsoft.Extensions.Caching.Memory;
    using NSubstitute;
    using Xunit;

    public class LogDialogTests : IClassFixture<BotConversationFixture>
    {
        private readonly BotConversationFixture conversationFixture;
        private readonly ILogDialog logDialog;
        private readonly ILogService logService;
        private readonly IUnderMaintenanceService umService;
        private readonly IRecurringJobManager recurringJobManager;
        private readonly IBackgroundJobClient backgroundJobClient;
        private readonly IMemoryCache memoryCache;

        public LogDialogTests(BotConversationFixture conversationFixture)
        {
            this.conversationFixture = conversationFixture;
            logService = Substitute.For<ILogService>();
            umService = Substitute.For<IUnderMaintenanceService>();
            recurringJobManager = Substitute.For<IRecurringJobManager>();
            backgroundJobClient = Substitute.For<IBackgroundJobClient>();
            memoryCache = new MemoryCache(new MemoryCacheOptions());
            logDialog = new LogDialog(
                this.conversationFixture.Configuration,
                logService,
                umService,
                this.conversationFixture.MockDbContext(),
                this.conversationFixture.Conversation,
                recurringJobManager,
                backgroundJobClient,
                memoryCache,
                new WebLogMessageBuilder());

            this.conversationFixture.Configuration
               .GetSection("LogInfo").GetSection("DisableAddCategories").Value
               .Returns("true");
            this.conversationFixture.Configuration
               .GetSection("LogInfo").GetSection("SendLogInUM").Value
               .Returns("false");
            this.conversationFixture.Configuration
                .GetSection("LogInfo").GetSection("IsProduction").Value
                .Returns("true");
        }

        #region AddCategory

        [Fact]
        public async Task HandleMessageAsync_AddCategory_CategoriesIsEmpty_SendErrorMessage()
        {
            // Arrange
            var message = "log add";

            // Act
            await logDialog.HandleMessage(conversationFixture.Activity, message);

            // Assert
            await conversationFixture
                .Conversation
                .Received()
                .ReplyAsync(
                    Arg.Is(conversationFixture.Activity),
                    Arg.Is("You need to add [LogCategory], otherwise, you will not get any log info"));
        }

        [Fact]
        public async Task HandleMessageAsync_AddCategory_IsDisableAddAndNotAdmin_SendErrorMessage()
        {
            // Arrange
            var message = "log add alpha";

            conversationFixture.Configuration
                .GetSection("LogInfo").GetSection("DisableAddCategories").Value
                .Returns("true");
            conversationFixture.Activity.Conversation.Returns(new ConversationAccount { Id = "3" });
            conversationFixture.InitDbContextData();

            // Act
            await logDialog.HandleMessage(conversationFixture.Activity, message);

            // Assert
            await conversationFixture
                .Conversation
                .Received()
                .ReplyAsync(
                    Arg.Is(conversationFixture.Activity),
                    Arg.Is("Add log categories is disabled, please contact NexOps."));
        }

        [Fact]
        public async Task HandleMessageAsync_AddCategory_IsDisableAddButAdmin_SendSuccessMessage()
        {
            // Arrange
            var message = "log add alpha";

            conversationFixture.Configuration
                .GetSection("LogInfo").GetSection("DisableAddCategories").Value
                .Returns("true");
            conversationFixture.Activity.Conversation.Returns(new ConversationAccount { Id = "2" });
            conversationFixture.InitDbContextData();

            // Act
            await logDialog.HandleMessage(conversationFixture.Activity, message);

            // Assert
            Assert.Equal(
                "alpha;",
                conversationFixture
                    .BotDbContext
                    .LogInfo
                    .FirstOrDefault(info => info.ConversationId == "2")
                    .LogCategories);
            await conversationFixture
                .Conversation
                .Received()
                .ReplyAsync(
                    Arg.Is(conversationFixture.Activity),
                    Arg.Is($"You will receive log with categories contain {MessageFormatSignal.BOLD_START}[alpha]{MessageFormatSignal.BOLD_END}"));
        }

        [Fact]
        public async Task HandleMessageAsync_AddCategory_IsEnableAdd_SendSuccessMessage()
        {
            // Arrange
            var message = "log add alpha";

            conversationFixture.Configuration
                .GetSection("LogInfo").GetSection("DisableAddCategories").Value
                .Returns("false");
            conversationFixture.Activity.Conversation.Returns(new ConversationAccount { Id = "5555" });

            // Act
            await logDialog.HandleMessage(conversationFixture.Activity, message);

            // Assert
            Assert.Equal(
                "alpha;",
                conversationFixture
                    .BotDbContext
                    .LogInfo
                    .FirstOrDefault(info => info.ConversationId == "5555")
                    .LogCategories);
            await conversationFixture
                .Conversation
                .Received()
                .ReplyAsync(
                    Arg.Is(conversationFixture.Activity),
                    Arg.Is($"You will receive log with categories contain {MessageFormatSignal.BOLD_START}[alpha]{MessageFormatSignal.BOLD_END}"));
        }

        #endregion AddCategory

        #region Remove Category

        [Fact]
        public async Task HandleMessageAsync_RemoveCategory_CategoriesIsEmpty_SendErrorMessage()
        {
            // Arrange
            var message = "log remove";

            // Act
            await logDialog.HandleMessage(conversationFixture.Activity, message);

            // Assert
            await conversationFixture
                .Conversation
                .Received()
                .ReplyAsync(
                    Arg.Is(conversationFixture.Activity),
                    Arg.Is("You need to add [LogCategory], otherwise, you will not get any log info"));
        }

        [Fact]
        public async Task HandleMessageAsync_RemoveCategory_NotFoundLogInfo_SendErrorMessage()
        {
            // Arrange
            var message = "log remove alpha";
            conversationFixture.Activity.Conversation.Returns(new ConversationAccount { Id = "55" });

            // Act
            await logDialog.HandleMessage(conversationFixture.Activity, message);

            // Assert
            await conversationFixture
                .Conversation
                .Received()
                .ReplyAsync(
                    Arg.Is(conversationFixture.Activity),
                    Arg.Is("You don't have any log categories data"));
        }

        [Fact]
        public async Task HandleMessageAsync_RemoveCategory_SendSuccessMessage()
        {
            // Arrange
            var message = "log remove nap";
            conversationFixture.Activity.Conversation.Returns(new ConversationAccount { Id = "10" });
            conversationFixture.InitDbContextData();

            // Act
            await logDialog.HandleMessage(conversationFixture.Activity, message);

            // Assert
            Assert.Equal(
             ";alpha",
             conversationFixture
                 .BotDbContext
                 .LogInfo
                 .FirstOrDefault(info => info.ConversationId == "10")
                 .LogCategories);
            await conversationFixture
                .Conversation
                .Received()
                .ReplyAsync(
                    Arg.Is(conversationFixture.Activity),
                    Arg.Is($"You will not receive log with categories contain {MessageFormatSignal.BOLD_START}[nap]{MessageFormatSignal.BOLD_END}"));
        }

        #endregion Remove Category

        [Fact]
        public async Task HandleMessageAsync_StartLogging_SendSuccessMessage()
        {
            // Arrange
            var message = "log start";
            conversationFixture.Activity.Conversation.Returns(new ConversationAccount { Id = "5" });
            memoryCache.Set("5", "1234");

            // Act
            await logDialog.HandleMessage(conversationFixture.Activity, message);

            // Assert
            Assert.True(conversationFixture
                    .BotDbContext
                    .LogInfo
                    .FirstOrDefault(info => info.ConversationId == "5")
                    .IsActive);

            backgroundJobClient.Received().ChangeState("1234", Arg.Any<DeletedState>(), null);

            recurringJobManager.Received()
              .AddOrUpdate(
                  Arg.Is("NotifyLogPeriodically"),
                  Arg.Any<Job>(),
                  Cron.Minutely(),
                  Arg.Any<RecurringJobOptions>());

            await conversationFixture
              .Conversation
              .Received()
              .ReplyAsync(
                  Arg.Is(conversationFixture.Activity),
                  Arg.Is($"Log has been started!"));
        }

        [Fact]
        public async Task HandleMessageAsync_StopLogging_WithDefaultStopTime_SendSuccessMessage()
        {
            // Arrange
            var message = "log stop";
            conversationFixture.Activity.Conversation.Returns(new ConversationAccount { Id = "6" });
            memoryCache.Set("6", "1234");

            // Act
            await logDialog.HandleMessage(conversationFixture.Activity, message);

            // Assert
            Assert.False(conversationFixture
                    .BotDbContext
                    .LogInfo
                    .FirstOrDefault(info => info.ConversationId == "6")
                    .IsActive);

            // Remove old job
            backgroundJobClient.Received().ChangeState("1234", Arg.Any<DeletedState>(), null);

            Assert.Equal("", memoryCache.Get("6"));

            await conversationFixture
              .Conversation
              .Received()
              .ReplyAsync(
                  Arg.Is(conversationFixture.Activity),
                  Arg.Is($"Log has been stopped for 10 minutes"));
        }

        [Fact]
        public async Task HandleMessageAsync_StopLogging_WithStopTime_SendSuccessMessage()
        {
            // Arrange
            var message = "log stop 3h";
            conversationFixture.Activity.Conversation.Returns(new ConversationAccount { Id = "126" });

            // Act
            await logDialog.HandleMessage(conversationFixture.Activity, message);

            // Assert
            Assert.False(conversationFixture
               .BotDbContext
               .LogInfo
               .FirstOrDefault(info => info.ConversationId == "126")
               .IsActive);

            await conversationFixture
              .Conversation
              .Received()
              .ReplyAsync(
                  Arg.Is(conversationFixture.Activity),
                  Arg.Is($"Log has been stopped for 3 hours"));
        }

        [Fact]
        public async Task HandleMessageAsync_RestartLogging_FoundInfo_SendSuccessMessage()
        {
            // Arrange
            var dbContext = conversationFixture.MockDbContext();
            dbContext.LogInfo.Add(new LogInfo { ConversationId = "666", LogCategories = "alpha;nap", IsActive = false });
            dbContext.MessageInfo.Add(new MessageInfo { ConversationId = "666" });
            dbContext.SaveChanges();
            conversationFixture.Activity.Conversation.Returns(new ConversationAccount { Id = "666" });

            // Act
            await logDialog.RestartNotifyingLog(conversationFixture.Activity.Conversation.Id);

            // Assert
            Assert.True(conversationFixture
              .BotDbContext
              .LogInfo
              .FirstOrDefault(info => info.ConversationId == "666")
              .IsActive);

            await conversationFixture
              .Conversation
              .Received()
              .SendAsync(Arg.Is("666"), Arg.Is("Log has been restarted!"));
        }

        [Fact]
        public async Task GetAndSendLogAsync_HasNoMessageInfo_DontSendMesage()
        {
            // Arrange
            var dbContext = conversationFixture.MockDbContext();
            dbContext.LogInfo.Add(new LogInfo { ConversationId = "9643534", LogCategories = "alpha;nap", IsActive = true });
            dbContext.SaveChanges();
            logService.GetErrorLogs().Returns(new List<Log>(){
                new Log {
                    CategoryName = "alpha",
                    MachineIP = "machine",
                    FormattedMessage = "log"
                },
                new Log {
                    CategoryName = "alpha",
                    MachineIP = "machine",
                    FormattedMessage = "log"
                },
                new Log {
                    CategoryName = "lrf",
                    MachineIP = "machine",
                    FormattedMessage = "log_lrf"
                }
            });
            umService.GetActualInfo().Returns(new Dictionary<int, UM> { { 1, new UM { IsUnderMaintenanceTime = true } } });

            // Act
            await logDialog.GetAndSendLogAsync();

            // Assert
            var expectedAlphaMessage = "**Category**: alpha\n\nlog\n\n**#Log Id**: 0 **Count**: 0\n\n\n\n====================================\n\n";
            await conversationFixture.Conversation
                 .DidNotReceive()
                 .SendAsync(Arg.Is("96435341"), Arg.Is(expectedAlphaMessage));
        }

        [Fact]
        public async Task GetAndSendLogAsync_InUM_NotAllowSendLogInUM_DontSendMesage()
        {
            // Arrange
            var dbContext = conversationFixture.MockDbContext();
            dbContext.MessageInfo.Add(new MessageInfo { ConversationId = "cd341234cdfa" });
            dbContext.LogInfo.Add(new LogInfo { ConversationId = "cd341234cdfa", LogCategories = "alpha;nap", IsActive = true });
            dbContext.SaveChanges();
            logService.GetErrorLogs().Returns(new List<Log>(){
                new Log {
                    CategoryName = "alpha",
                    MachineIP = "machine",
                    FormattedMessage = "log"
                }
            });
            umService.GetActualInfo().Returns(new Dictionary<int, UM> { { 1, new UM { IsUnderMaintenanceTime = true } } });
            conversationFixture.Configuration
                .GetSection("LogInfo").GetSection("SendLogInUM").Value
                .Returns("false");

            // Act
            await logDialog.GetAndSendLogAsync();

            // Assert
            var expectedAlphaMessage = "**Category**: alpha\n\nlog\n\n**#Log Id**: 0 **Count**: 0\n\n\n\n====================================\n\n";
            await conversationFixture.Conversation
                 .DidNotReceive()
                 .SendAsync(Arg.Is("cd341234cdfa"), Arg.Is(expectedAlphaMessage));
        }

        [Fact]
        public async Task GetAndSendLogAsync_InUM_AllowSendLogInUM_SendMesage()
        {
            // Arrange
            var dbContext = conversationFixture.MockDbContext();
            dbContext.MessageInfo.Add(new MessageInfo { ConversationId = "cd3412234234353454334cdfa" });
            dbContext.LogInfo.Add(new LogInfo
            {
                ConversationId = "cd3412234234353454334cdfa",
                LogCategories = "alpha;nap;",
            });

            dbContext.SaveChanges();

            logService.GetErrorLogs().Returns(new List<Log>(){
                new Log {
                    CategoryName = "alpha",
                    MachineIP = "machine",
                    FormattedMessage = "log"
                }
            });
            umService.GetActualInfo().Returns(new Dictionary<int, UM> { { 1, new UM { IsUnderMaintenanceTime = true } } });
            conversationFixture.Configuration
                .GetSection("LogInfo")?.GetSection("SendLogInUM")?.Value
                .Returns("true");

            // Act
            await logDialog.GetAndSendLogAsync();

            // Assert
            var expectedAlphaMessage =
                $"{MessageFormatSignal.BOLD_START}Category{MessageFormatSignal.BOLD_END}: alpha{MessageFormatSignal.NEWLINE}" +
                $"log{MessageFormatSignal.NEWLINE}" +
                $"{MessageFormatSignal.BOLD_START}#Log Id{MessageFormatSignal.BOLD_END}: 0 " +
                $"{MessageFormatSignal.BOLD_START}Count{MessageFormatSignal.BOLD_END}: 0" +
                $"{MessageFormatSignal.DOUBLE_NEWLINE}{MessageFormatSignal.DIVIDER}";

            await conversationFixture.Conversation
                 .Received()
                 .SendAsync(Arg.Is("cd3412234234353454334cdfa"), Arg.Is(expectedAlphaMessage));
        }

        [Fact]
        public async Task GetAndSendLogAsync_HasLogInfoData_IsActive_HasLogCategory_SendLogToClient()
        {
            // Arrange
            conversationFixture.InitDbContextData();
            var dbContext = conversationFixture.MockDbContext();
            dbContext.LogInfo.Add(new LogInfo { ConversationId = "1", LogCategories = "alpha;nap", IsActive = true });
            dbContext.SaveChanges();
            logService.GetErrorLogs().Returns(new List<Log>(){
                new Log {
                    CategoryName = "alpha",
                    MachineIP = "machine",
                    FormattedMessage = "log"
                },
                new Log {
                    CategoryName = "alpha",
                    MachineIP = "machine",
                    FormattedMessage = "log"
                },
                new Log {
                    CategoryName = "lrf",
                    MachineIP = "machine",
                    FormattedMessage = "log_lrf"
                }
            });
            umService.GetActualInfo().Returns(new Dictionary<int, UM> { { 1, new UM { IsUnderMaintenanceTime = false } } });

            // Act
            await logDialog.GetAndSendLogAsync();

            // Assert
            var expectedAlphaMessage =
                    $"{MessageFormatSignal.BOLD_START}Category{MessageFormatSignal.BOLD_END}: alpha{MessageFormatSignal.NEWLINE}" +
                    $"log{MessageFormatSignal.NEWLINE}" +
                    $"{MessageFormatSignal.BOLD_START}#Log Id{MessageFormatSignal.BOLD_END}: 0 " +
                    $"{MessageFormatSignal.BOLD_START}Count{MessageFormatSignal.BOLD_END}: 0" +
                    $"{MessageFormatSignal.DOUBLE_NEWLINE}{MessageFormatSignal.DIVIDER}";
            await conversationFixture.Conversation
                 .Received(1)
                 .SendAsync(Arg.Is("1"), Arg.Is(expectedAlphaMessage));

            var expectedLrfMessage = "**Category**: lrf\n\nlog_lrf\n\n**#Log Id**: 0 **Count**: 0\n\n\n\n====================================\n\n";
            await conversationFixture.Conversation
                 .DidNotReceive()
                 .SendAsync(Arg.Is("1"), Arg.Is(expectedLrfMessage));
        }

        [Fact]
        public async Task GetAndSendLogAsync_IsUM_DontAllowSendLogInUM_NotSendLog()
        {
            // Arrange
            umService.GetActualInfo().Returns(new Dictionary<int, UM> { { 1, new UM { IsUnderMaintenanceTime = true } } });
            conversationFixture.Configuration.GetSection("LogInfo").GetSection("SendLogInUM").Value.Returns("false");

            var dbContext = conversationFixture.MockDbContext();
            dbContext.MessageInfo.Add(new MessageInfo { ConversationId = "234234223423342311cd1" });
            dbContext.LogInfo.Add(new LogInfo { ConversationId = "234234223423342311cd1", LogCategories = "alpha;nap", IsActive = true });
            dbContext.SaveChanges();
            logService.GetErrorLogs().Returns(new List<Log>(){
                new Log {
                    CategoryName = "alpha",
                    MachineIP = "machine",
                    FormattedMessage = "log"
                },
            });

            // Act
            await logDialog.GetAndSendLogAsync();

            // Assert
            await conversationFixture.Conversation
               .DidNotReceive()
               .SendAsync(Arg.Any<MessageInfo>());
        }

        [Fact]
        public async Task GetAndSendLogAsync_HasLogInfoData_IsActive_HasLogCategory_HasIgnoreMessage_NotSendLogToClient()
        {
            // Arrange
            conversationFixture.InitDbContextData();
            var dbContext = conversationFixture.MockDbContext();
            dbContext.MessageInfo.Add(new MessageInfo { ConversationId = "2342342342311cd1" });
            dbContext.LogInfo.Add(new LogInfo { ConversationId = "2342342342311cd1", LogCategories = "alpha;nap", IsActive = true });
            dbContext.LogIgnoreMessage.Add(new LogIgnoreMessage { Category = "alpha", IgnoreMessage = "Thread was being aborted" });
            dbContext.SaveChanges();
            logService.GetErrorLogs().Returns(new List<Log>(){
                new Log {
                    CategoryName = "alpha",
                    MachineIP = "machine",
                    FormattedMessage = "thread was being aborted"
                },
            });
            umService.GetActualInfo().Returns(new Dictionary<int, UM> { { 1, new UM { IsUnderMaintenanceTime = false } } });

            // Act
            await logDialog.GetAndSendLogAsync();

            // Assert
            var expectedAlphaMessage = "**Category**: alpha\n\nthread was being aborted\n\n**#Log Id**: 0 **Count**: 0\n\n\n\n====================================\n\n";
            await conversationFixture.Conversation
                 .DidNotReceive()
                 .SendAsync(Arg.Is<MessageInfo>(info =>
                    info.Text == expectedAlphaMessage &&
                    info.ConversationId == "2342342342311cd1"));
        }

        [Fact]
        public async Task GetAndSendLogAsync_HasLogInfoData_IsActive_HasLogCategory_HasNoIgnoreMessage_NotSendLogToClient()
        {
            // Arrange
            conversationFixture.InitDbContextData();
            var dbContext = conversationFixture.MockDbContext();
            dbContext.MessageInfo.Add(new MessageInfo { ConversationId = "234234231" });
            dbContext.LogInfo.Add(new LogInfo { ConversationId = "234234231", LogCategories = "alpha;nap", IsActive = true });
            dbContext.LogIgnoreMessage.Add(new LogIgnoreMessage { Category = "nap", IgnoreMessage = "thread was being aborted" });
            dbContext.SaveChanges();
            logService.GetErrorLogs().Returns(new List<Log>(){
                new Log {
                    CategoryName = "nap",
                    MachineIP = "machine",
                    FormattedMessage = "thread was not being aborted"
                },
            });
            umService.GetActualInfo().Returns(new Dictionary<int, UM> { { 1, new UM { IsUnderMaintenanceTime = false } } });

            // Act
            await logDialog.GetAndSendLogAsync();

            // Assert
            var expectedAlphaMessage =
                    $"{MessageFormatSignal.BOLD_START}Category{MessageFormatSignal.BOLD_END}: nap{MessageFormatSignal.NEWLINE}" +
                    $"thread was not being aborted{MessageFormatSignal.NEWLINE}" +
                    $"{MessageFormatSignal.BOLD_START}#Log Id{MessageFormatSignal.BOLD_END}: 0 " +
                    $"{MessageFormatSignal.BOLD_START}Count{MessageFormatSignal.BOLD_END}: 0" +
                    $"{MessageFormatSignal.DOUBLE_NEWLINE}{MessageFormatSignal.DIVIDER}";
            await conversationFixture.Conversation
                 .Received(1)
                 .SendAsync(Arg.Is("234234231"), Arg.Is(expectedAlphaMessage));
        }

        [Theory]
        [InlineData("9", true, "Running")]
        [InlineData("8", false, "Stopped")]
        public async Task HandleMessageAsync_ViewStatus_SendLogInfoMessage(string conversationId, bool isActive, string expectedActiveResult)
        {
            // Arrange
            var message = "log status";
            var dbContext = conversationFixture.MockDbContext();
            dbContext.LogInfo.Add(new LogInfo { ConversationId = conversationId, LogCategories = "alpha;nap", IsActive = isActive });
            dbContext.SaveChanges();
            conversationFixture.Activity.Conversation.Returns(new ConversationAccount { Id = conversationId });

            // Act
            await logDialog.HandleMessage(conversationFixture.Activity, message);

            // Assert
            await conversationFixture
              .Conversation
              .Received()
              .ReplyAsync(
                  Arg.Is(conversationFixture.Activity),
                  Arg.Is(
                    $"Your log status {MessageFormatSignal.NEWLINE}" +
                    $"{MessageFormatSignal.BOLD_START}Log Categories:{MessageFormatSignal.BOLD_END} " +
                    $"[alpha;nap]{MessageFormatSignal.NEWLINE}" +
                    MessageFormatSignal.BOLD_START + expectedActiveResult + MessageFormatSignal.BOLD_END +
                    MessageFormatSignal.NEWLINE));
        }

        [Fact]
        public async Task HandleMessageAsync_AnyMessage_SendCommandMessage()
        {
            // Arrange
            var message = "log";
            conversationFixture.Activity.Conversation.Returns(new ConversationAccount { Id = "669" });

            // Act
            await logDialog.HandleMessage(conversationFixture.Activity, message);

            // Assert
            await conversationFixture
                .Conversation
                .Received()
                .ReplyAsync(Arg.Is(conversationFixture.Activity), Arg.Is(conversationFixture.CommandMessage));
        }
    }
}