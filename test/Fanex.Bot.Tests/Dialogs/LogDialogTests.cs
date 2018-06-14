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
    }
}