﻿namespace Fanex.Bot.Skynex.Tests.Dialogs
{
    using System.Linq;
    using System.Threading.Tasks;
    using Fanex.Bot.Models;
    using Fanex.Bot.Models.GitLab;
    using Fanex.Bot.Models.Log;
    using Fanex.Bot.Skynex.Dialogs;
    using Fanex.Bot.Skynex.Tests.Fixtures;
    using Microsoft.Bot.Connector;
    using NSubstitute;
    using Xunit;

    public class CommonDialogTests : IClassFixture<BotConversationFixture>
    {
        private readonly BotConversationFixture _conversationFixture;
        private readonly ICommonDialog _dialog;

        public CommonDialogTests(BotConversationFixture conversationFixture)
        {
            _conversationFixture = conversationFixture;
            _dialog = new CommonDialog(_conversationFixture.BotDbContext, conversationFixture.Conversation);
        }

        [Fact]
        public async Task HandleMessageAsync_MessageGroup_SendGroupId()
        {
            // Arrange
            var message = "group";
            _conversationFixture.Activity.Conversation.Returns(new ConversationAccount { Id = "13324" });

            // Act
            await _dialog.HandleMessage(_conversationFixture.Activity, message);

            // Assert
            await _conversationFixture
                .Conversation
                .Received()
                .ReplyAsync(Arg.Is(_conversationFixture.Activity), Arg.Is("Your group id is: 13324"));
        }

        [Fact]
        public async Task HandleMessageAsync_MessageHelp_SendCommand()
        {
            // Arrange
            var message = "help";

            // Act
            await _dialog.HandleMessage(_conversationFixture.Activity, message);

            // Assert
            await _conversationFixture
                .Conversation
                .Received()
                .ReplyAsync(Arg.Is(_conversationFixture.Activity), Arg.Is(_conversationFixture.CommandMessage));
        }

        [Fact]
        public async Task HandleMessageAsync_AnyMessage_SendDefaultMessage()
        {
            // Arrange
            var message = "any";

            // Act
            await _dialog.HandleMessage(_conversationFixture.Activity, message);

            // Assert
            await _conversationFixture
                .Conversation
                .Received()
                .ReplyAsync(
                    Arg.Is(_conversationFixture.Activity),
                    Arg.Is($"Please send {MessageFormatSignal.BeginBold}help{MessageFormatSignal.EndBold} to get my commands"));
        }

        [Fact]
        public async Task RegisterMessageInfo_MessageDoesNotExist_AddMessageInfoAndSendAdmin()
        {
            // Arrange
            _conversationFixture.Activity.Conversation.Returns(new ConversationAccount { Id = "13324dfwer234" });

            // Act
            await _dialog.RegisterMessageInfo(_conversationFixture.Activity);

            // Assert
            Assert.True(
                _conversationFixture
                    .BotDbContext
                    .MessageInfo
                    .Any(info => info.ConversationId == "13324dfwer234"));

            await _conversationFixture
                .Conversation
                .Received()
                .SendAdminAsync($"New client {MessageFormatSignal.BeginBold}13324dfwer234{MessageFormatSignal.EndBold} has been added");
        }

        [Fact]
        public async Task RemoveConversationDatat_RemoveDataAndSendAdmin()
        {
            // Arrange
            _conversationFixture.Activity.Conversation.Returns(new ConversationAccount { Id = "13324d234234fwer234" });
            var dbContext = _conversationFixture.MockDbContext();
            await dbContext.LogInfo.AddAsync(new LogInfo { ConversationId = "13324d234234fwer234", LogCategories = "alpha" });
            await dbContext.GitLabInfo.AddAsync(new GitLabInfo { ConversationId = "13324d234234fwer234", ProjectUrl = "3424" });
            await dbContext.MessageInfo.AddAsync(new MessageInfo { ConversationId = "13324d234234fwer234" });
            await dbContext.SaveChangesAsync();

            // Act
            await _dialog.RemoveConversationData(_conversationFixture.Activity);

            // Assert
            Assert.False(
                _conversationFixture
                    .BotDbContext
                    .MessageInfo
                    .Any(info => info.ConversationId == "13324d234234fwer234"));

            Assert.False(
                _conversationFixture
                    .BotDbContext
                    .GitLabInfo
                    .Any(info => info.ConversationId == "13324d234234fwer234"));

            Assert.False(
              _conversationFixture
                  .BotDbContext
                  .LogInfo
                  .Any(info => info.ConversationId == "13324d234234fwer234"));

            await _conversationFixture
                .Conversation
                .Received()
                .SendAdminAsync($"Client {MessageFormatSignal.BeginBold}13324d234234fwer234{MessageFormatSignal.EndBold} has been removed");
        }
    }
}