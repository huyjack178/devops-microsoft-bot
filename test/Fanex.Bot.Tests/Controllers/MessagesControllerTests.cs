﻿namespace Fanex.Bot.Tests.Controllers
{
    using System.Threading.Tasks;
    using Fanex.Bot.Controllers;
    using Fanex.Bot.Dialogs;
    using Fanex.Bot.Tests.Fixtures;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Bot.Connector;
    using NSubstitute;
    using Xunit;

    public class MessagesControllerTests : IClassFixture<BotConversationFixture>
    {
        private readonly BotConversationFixture _conversationFixture;
        private readonly IRootDialog _rootDialog;
        private readonly ILogDialog _logDialog;
        private readonly IGitLabDialog _gitLabDialog;
        private readonly MessagesController _messagesController;

        public MessagesControllerTests(BotConversationFixture conversationFixture)
        {
            _conversationFixture = conversationFixture;
            _rootDialog = Substitute.For<IRootDialog>();
            _logDialog = Substitute.For<ILogDialog>();
            _gitLabDialog = Substitute.For<IGitLabDialog>();
            _messagesController = new MessagesController(
                _rootDialog,
                _logDialog,
                _gitLabDialog,
                _conversationFixture.Conversation);
        }

        [Fact]
        public async Task Post_ActivityMessage_HandMessageCommand_LogCommand_CallLogDialog()
        {
            // Arrange
            var activity = new Activity { Type = ActivityTypes.Message, Text = "log add" };

            // Act
            await _messagesController.Post(activity);

            // Asserts
            await _logDialog.Received().HandleMessageAsync(Arg.Is(activity), "log add");
        }

        [Fact]
        public async Task Post_ActivityMessage_HandMessageCommand_GitlabCommand_CallGitLabDialog()
        {
            // Arrange
            var activity = new Activity { Type = ActivityTypes.Message, Text = "gitlab" };

            // Act
            await _messagesController.Post(activity);

            // Asserts
            await _gitLabDialog.Received().HandleMessageAsync(Arg.Is(activity), "gitlab");
        }

        [Fact]
        public async Task Post_ActivityMessage_HandMessageCommand_OtherCommand_CallRootDialog()
        {
            // Arrange
            var activity = new Activity { Type = ActivityTypes.Message, Text = "group" };

            // Act
            await _messagesController.Post(activity);

            // Asserts
            await _rootDialog.Received().HandleMessageAsync(Arg.Is(activity), "group");
        }

        [Fact]
        public async Task Post_ActivityMessage_HandMessageCommand_TextHasBotName_RemoveBotName()
        {
            // Arrange
            var activity = new Activity { Type = ActivityTypes.Message, Text = "@Skynex group" };

            // Act
            await _messagesController.Post(activity);

            // Asserts
            await _rootDialog.Received().HandleMessageAsync(Arg.Is(activity), "group");
        }

        [Fact]
        public async Task Post_ActivityConversationUpdate_SendHelloMessage()
        {
            // Arrange
            var activity = new Activity { Type = ActivityTypes.ConversationUpdate };

            // Act
            await _messagesController.Post(activity);

            // Asserts
            await _conversationFixture
                .Conversation
                .Received()
                .SendAsync(Arg.Is(activity), "Hello. I am SkyNex.");
        }

        [Fact]
        public async Task Post_ActivityInstallationUpdate_SendHelloMessage()
        {
            // Arrange
            var activity = new Activity { Type = ActivityTypes.InstallationUpdate };

            // Act
            await _messagesController.Post(activity);

            // Asserts
            await _conversationFixture
                .Conversation
                .Received()
                .SendAsync(Arg.Is(activity), "Hello. I am SkyNex.");
        }

        [Fact]
        public async Task Post_ActivityContactRelationUpdate_SendHelloMessage()
        {
            // Arrange
            var activity = new Activity { Type = ActivityTypes.ContactRelationUpdate };

            // Act
            await _messagesController.Post(activity);

            // Asserts
            await _conversationFixture
                .Conversation
                .Received()
                .SendAsync(Arg.Is(activity), "Hello. I am SkyNex.");
        }

        [Fact]
        public async Task Post_ActivityEndOfConversation_SendHelloMessage()
        {
            // Arrange
            var activity = new Activity { Type = ActivityTypes.EndOfConversation };

            // Act
            await _messagesController.Post(activity);

            // Asserts
            await _conversationFixture
                .Conversation
                .Received()
                .SendAsync(Arg.Is(activity), "See you again!");
        }

        [Fact]
        public async Task Post_ActivityOthers_ReturnOk()
        {
            // Arrange
            var activity = new Activity { Type = ActivityTypes.Ping };

            // Act
            var result = await _messagesController.Post(activity);

            // Asserts
            Assert.IsType<OkResult>(result);
        }
    }
}