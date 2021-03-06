using Fanex.Bot.Core._Shared.Constants;
using Fanex.Bot.Core.Bot.Models;
using Fanex.Bot.Core.Log.Models;
using Fanex.Bot.Core.Log.Services;
using Fanex.Bot.Core.UM.Services;
using Fanex.Bot.Skynex.Log;

namespace Fanex.Bot.Skynex.Tests.Dialogs
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
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
#pragma warning disable S2699 // Tests should include assertions
        public async Task HandleMessageAsync_AddCategory_CategoriesIsEmpty_SendErrorMessage()
#pragma warning restore S2699 // Tests should include assertions
        {
            // Arrange
            var message = "log_msite add";

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
#pragma warning disable S2699 // Tests should include assertions
        public async Task HandleMessageAsync_AddCategory_IsDisableAddAndNotAdmin_SendErrorMessage()
#pragma warning restore S2699 // Tests should include assertions
        {
            // Arrange
            var message = "log_msite add alpha";

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
            conversationFixture.InitDbContextData();
            var message = "log_msite add alpha";

            conversationFixture.Configuration
                .GetSection("LogInfo").GetSection("DisableAddCategories").Value
                .Returns("true");
            conversationFixture.Activity.Conversation.Returns(new ConversationAccount { Id = "2" });

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
                    Arg.Is($"You will receive log with categories contain {MessageFormatSymbol.BOLD_START}[alpha]{MessageFormatSymbol.BOLD_END}"));
        }

        [Fact]
        public async Task HandleMessageAsync_AddCategory_IsEnableAdd_SendSuccessMessage()
        {
            // Arrange
            var message = "log_msite add alpha";

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
                    Arg.Is($"You will receive log with categories contain {MessageFormatSymbol.BOLD_START}[alpha]{MessageFormatSymbol.BOLD_END}"));
        }

        #endregion AddCategory

        #region Remove Category

        [Fact]
#pragma warning disable S2699 // Tests should include assertions
        public async Task HandleMessageAsync_RemoveCategory_CategoriesIsEmpty_SendErrorMessage()
#pragma warning restore S2699 // Tests should include assertions
        {
            // Arrange
            var message = "log_msite remove";

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
#pragma warning disable S2699 // Tests should include assertions
        public async Task HandleMessageAsync_RemoveCategory_NotFoundLogInfo_SendErrorMessage()
#pragma warning restore S2699 // Tests should include assertions
        {
            // Arrange
            var message = "log_msite remove alpha";
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
            var message = "log_msite remove nap";
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
                    Arg.Is($"You will not receive log with categories contain {MessageFormatSymbol.BOLD_START}[nap]{MessageFormatSymbol.BOLD_END}"));
        }

        #endregion Remove Category

        [Fact]
        public async Task HandleMessageAsync_StartLogging_SendSuccessMessage()
        {
            // Arrange
            var message = "log_msite start";
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
            var message = "log_msite stop";
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
            var message = "log_msite stop 3h";
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
#pragma warning disable S2699 // Tests should include assertions
        public async Task GetAndSendLogAsync_HasNoMessageInfo_DontSendMesage()
#pragma warning restore S2699 // Tests should include assertions
        {
            // Arrange
            var dbContext = conversationFixture.MockDbContext();
            dbContext.LogInfo.Add(new LogInfo { ConversationId = "9643534", LogCategories = "alpha;nap", IsActive = true });
            dbContext.SaveChanges();
            logService.GetErrorLogs().Returns(new List<Core.Log.Models.Log>(){
                new Core.Log.Models.Log {
                    CategoryName = "alpha",
                    MachineIP = "machine",
                    FormattedMessage = "log"
                },
                new Core.Log.Models.Log {
                    CategoryName = "alpha",
                    MachineIP = "machine",
                    FormattedMessage = "log"
                },
                new Core.Log.Models.Log {
                    CategoryName = "lrf",
                    MachineIP = "machine",
                    FormattedMessage = "log_lrf"
                }
            });
            umService.GetActualInfo().Returns(new Dictionary<int, Core.UM.Models.UM> { { 1, new Core.UM.Models.UM { IsUnderMaintenanceTime = true } } });

            // Act
            await logDialog.GetAndSendLogAsync();

            // Assert
            var expectedAlphaMessage = "**Category**: alpha\n\nlog\n\n**#Log Id**: 0 **Count**: 0\n\n\n\n====================================\n\n";
            await conversationFixture.Conversation
                 .DidNotReceive()
                 .SendAsync(Arg.Is("96435341"), Arg.Is(expectedAlphaMessage));
        }

        [Fact]
#pragma warning disable S2699 // Tests should include assertions
        public async Task GetAndSendLogAsync_InUM_NotAllowSendLogInUM_DontSendMesage()
#pragma warning restore S2699 // Tests should include assertions
        {
            // Arrange
            var dbContext = conversationFixture.MockDbContext();
            dbContext.MessageInfo.Add(new MessageInfo { ConversationId = "cd341234cdfa" });
            dbContext.LogInfo.Add(new LogInfo { ConversationId = "cd341234cdfa", LogCategories = "alpha;nap", IsActive = true });
            dbContext.SaveChanges();
            logService.GetErrorLogs().Returns(new List<Core.Log.Models.Log>(){
                new Core.Log.Models.Log {
                    CategoryName = "alpha",
                    MachineIP = "machine",
                    FormattedMessage = "log"
                }
            });
            umService.GetActualInfo().Returns(new Dictionary<int, Core.UM.Models.UM> { { 1, new Core.UM.Models.UM { IsUnderMaintenanceTime = true } } });
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
#pragma warning disable S2699 // Tests should include assertions
        public async Task GetAndSendLogAsync_InUM_AllowSendLogInUM_SendMesage()
#pragma warning restore S2699 // Tests should include assertions
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

            logService.GetErrorLogs().Returns(new List<Core.Log.Models.Log>(){
                new Core.Log.Models.Log {
                    CategoryName = "alpha",
                    MachineIP = "machine",
                    FormattedMessage = "log"
                }
            });
            umService.GetActualInfo().Returns(new Dictionary<int, Core.UM.Models.UM> { { 1, new Core.UM.Models.UM { IsUnderMaintenanceTime = true } } });
            conversationFixture.Configuration
                .GetSection("LogInfo")?.GetSection("SendLogInUM")?.Value
                .Returns("true");

            // Act
            await logDialog.GetAndSendLogAsync();

            // Assert
            var expectedAlphaMessage =
                $"{MessageFormatSymbol.BOLD_START}Category{MessageFormatSymbol.BOLD_END}: alpha{MessageFormatSymbol.NEWLINE}" +
                $"log{MessageFormatSymbol.NEWLINE}" +
                $"{MessageFormatSymbol.BOLD_START}#Log Id{MessageFormatSymbol.BOLD_END}: 0 " +
                $"{MessageFormatSymbol.BOLD_START}Count{MessageFormatSymbol.BOLD_END}: 0" +
                $"{MessageFormatSymbol.DOUBLE_NEWLINE}{MessageFormatSymbol.DIVIDER}";

            await conversationFixture.Conversation
                 .Received()
                 .SendAsync(Arg.Is("cd3412234234353454334cdfa"), Arg.Is(expectedAlphaMessage));
        }

        [Fact]
#pragma warning disable S2699 // Tests should include assertions
        public async Task GetAndSendLogAsync_HasLogInfoData_IsActive_HasLogCategory_SendLogToClient()
#pragma warning restore S2699 // Tests should include assertions
        {
            // Arrange
            conversationFixture.InitDbContextData();
            var dbContext = conversationFixture.MockDbContext();
            dbContext.LogInfo.Add(new LogInfo { ConversationId = "1", LogCategories = "alpha;nap", IsActive = true });
            dbContext.SaveChanges();
            logService.GetErrorLogs().Returns(new List<Core.Log.Models.Log>(){
                new Core.Log.Models.Log {
                    CategoryName = "alpha",
                    MachineIP = "machine",
                    FormattedMessage = "log"
                },
                new Core.Log.Models.Log {
                    CategoryName = "alpha",
                    MachineIP = "machine",
                    FormattedMessage = "log"
                },
                new Core.Log.Models.Log {
                    CategoryName = "lrf",
                    MachineIP = "machine",
                    FormattedMessage = "log_lrf"
                }
            });
            umService.GetActualInfo().Returns(new Dictionary<int, Core.UM.Models.UM> { { 1, new Core.UM.Models.UM { IsUnderMaintenanceTime = false } } });

            // Act
            await logDialog.GetAndSendLogAsync();

            // Assert
            var expectedAlphaMessage =
                    $"{MessageFormatSymbol.BOLD_START}Category{MessageFormatSymbol.BOLD_END}: alpha{MessageFormatSymbol.NEWLINE}" +
                    $"log{MessageFormatSymbol.NEWLINE}" +
                    $"{MessageFormatSymbol.BOLD_START}#Log Id{MessageFormatSymbol.BOLD_END}: 0 " +
                    $"{MessageFormatSymbol.BOLD_START}Count{MessageFormatSymbol.BOLD_END}: 0" +
                    $"{MessageFormatSymbol.DOUBLE_NEWLINE}{MessageFormatSymbol.DIVIDER}";
            await conversationFixture.Conversation
                 .Received(1)
                 .SendAsync(Arg.Is("1"), Arg.Is(expectedAlphaMessage));

            var expectedLrfMessage = "**Category**: lrf\n\nlog_lrf\n\n**#Log Id**: 0 **Count**: 0\n\n\n\n====================================\n\n";
            await conversationFixture.Conversation
                 .DidNotReceive()
                 .SendAsync(Arg.Is("1"), Arg.Is(expectedLrfMessage));
        }

        [Fact]
#pragma warning disable S2699 // Tests should include assertions
        public async Task GetAndSendLogAsync_IsUM_DontAllowSendLogInUM_NotSendLog()
#pragma warning restore S2699 // Tests should include assertions
        {
            // Arrange
            umService.GetActualInfo().Returns(new Dictionary<int, Core.UM.Models.UM> { { 1, new Core.UM.Models.UM { IsUnderMaintenanceTime = true } } });
            conversationFixture.Configuration.GetSection("LogInfo").GetSection("SendLogInUM").Value.Returns("false");

            var dbContext = conversationFixture.MockDbContext();
            dbContext.MessageInfo.Add(new MessageInfo { ConversationId = "234234223423342311cd1" });
            dbContext.LogInfo.Add(new LogInfo { ConversationId = "234234223423342311cd1", LogCategories = "alpha;nap", IsActive = true });
            dbContext.SaveChanges();
            logService.GetErrorLogs().Returns(new List<Core.Log.Models.Log>(){
                new Core.Log.Models.Log {
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
#pragma warning disable S2699 // Tests should include assertions
        public async Task GetAndSendLogAsync_HasLogInfoData_IsActive_HasLogCategory_HasIgnoreMessage_NotSendLogToClient()
#pragma warning restore S2699 // Tests should include assertions
        {
            // Arrange
            conversationFixture.InitDbContextData();
            var dbContext = conversationFixture.MockDbContext();
            dbContext.MessageInfo.Add(new MessageInfo { ConversationId = "2342342342311cd1" });
            dbContext.LogInfo.Add(new LogInfo { ConversationId = "2342342342311cd1", LogCategories = "alpha;nap", IsActive = true });
            dbContext.LogIgnoreMessage.Add(new LogIgnoreMessage { Category = "alpha", IgnoreMessage = "Thread was being aborted" });
            dbContext.SaveChanges();
            logService.GetErrorLogs().Returns(new List<Core.Log.Models.Log>(){
                new Core.Log.Models.Log {
                    CategoryName = "alpha",
                    MachineIP = "machine",
                    FormattedMessage = "thread was being aborted"
                },
            });
            umService.GetActualInfo().Returns(new Dictionary<int, Core.UM.Models.UM> { { 1, new Core.UM.Models.UM { IsUnderMaintenanceTime = false } } });

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
#pragma warning disable S2699 // Tests should include assertions
        public async Task GetAndSendLogAsync_HasLogInfoData_IsActive_HasLogCategory_HasNoIgnoreMessage_NotSendLogToClient()
#pragma warning restore S2699 // Tests should include assertions
        {
            // Arrange
            conversationFixture.InitDbContextData();
            var dbContext = conversationFixture.MockDbContext();
            dbContext.MessageInfo.Add(new MessageInfo { ConversationId = "234234231" });
            dbContext.LogInfo.Add(new LogInfo { ConversationId = "234234231", LogCategories = "alpha;nap", IsActive = true });
            dbContext.LogIgnoreMessage.Add(new LogIgnoreMessage { Category = "nap", IgnoreMessage = "thread was being aborted" });
            dbContext.SaveChanges();
            logService.GetErrorLogs().Returns(new List<Core.Log.Models.Log>(){
                new Core.Log.Models.Log {
                    CategoryName = "nap",
                    MachineIP = "machine",
                    FormattedMessage = "thread was not being aborted"
                },
            });
            umService.GetActualInfo().Returns(new Dictionary<int, Core.UM.Models.UM> { { 1, new Core.UM.Models.UM { IsUnderMaintenanceTime = false } } });

            // Act
            await logDialog.GetAndSendLogAsync();

            // Assert
            var expectedAlphaMessage =
                    $"{MessageFormatSymbol.BOLD_START}Category{MessageFormatSymbol.BOLD_END}: nap{MessageFormatSymbol.NEWLINE}" +
                    $"thread was not being aborted{MessageFormatSymbol.NEWLINE}" +
                    $"{MessageFormatSymbol.BOLD_START}#Log Id{MessageFormatSymbol.BOLD_END}: 0 " +
                    $"{MessageFormatSymbol.BOLD_START}Count{MessageFormatSymbol.BOLD_END}: 0" +
                    $"{MessageFormatSymbol.DOUBLE_NEWLINE}{MessageFormatSymbol.DIVIDER}";
            await conversationFixture.Conversation
                 .Received(1)
                 .SendAsync(Arg.Is("234234231"), Arg.Is(expectedAlphaMessage));
        }

        [Theory]
        [InlineData("9", true, "Running")]
        [InlineData("8", false, "Stopped")]
#pragma warning disable S2699 // Tests should include assertions
        public async Task HandleMessageAsync_ViewStatus_SendLogInfoMessage(string conversationId, bool isActive, string expectedActiveResult)
#pragma warning restore S2699 // Tests should include assertions
        {
            // Arrange
            var message = "log_msite status";
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
                    $"Your log status {MessageFormatSymbol.NEWLINE}" +
                    $"{MessageFormatSymbol.BOLD_START}Log Categories:{MessageFormatSymbol.BOLD_END} " +
                    $"[alpha;nap]{MessageFormatSymbol.NEWLINE}" +
                    MessageFormatSymbol.BOLD_START + expectedActiveResult + MessageFormatSymbol.BOLD_END +
                    MessageFormatSymbol.NEWLINE));
        }

        [Fact]
#pragma warning disable S2699 // Tests should include assertions
        public async Task HandleMessageAsync_AnyMessage_SendCommandMessage()
#pragma warning restore S2699 // Tests should include assertions
        {
            // Arrange
            var message = "log_msite";
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