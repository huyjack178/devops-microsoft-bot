namespace Fanex.Bot.Skynex.Tests.Controllers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fanex.Bot.Controllers;
    using Fanex.Bot.Models;
    using Fanex.Bot.Skynex.Dialogs;
    using Fanex.Bot.Skynex.Tests.Fixtures;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Bot.Connector;
    using NSubstitute;
    using Xunit;

    public class MessagesControllerTests : IClassFixture<BotConversationFixture>
    {
        private readonly BotConversationFixture _conversationFixture;
        private readonly ICommonDialog _commonDialog;
        private readonly ILogDialog _logDialog;
        private readonly IGitLabDialog _gitLabDialog;
        private readonly ILineDialog _lineDialog;
        private readonly IUnderMaintenanceDialog _umDialog;
        private readonly IDBLogDialog dbLogDialog;
        private readonly MessagesController _messagesController;

        public MessagesControllerTests(BotConversationFixture conversationFixture)
        {
            _conversationFixture = conversationFixture;
            _commonDialog = Substitute.For<ICommonDialog>();
            _logDialog = Substitute.For<ILogDialog>();
            _gitLabDialog = Substitute.For<IGitLabDialog>();
            _lineDialog = Substitute.For<ILineDialog>();
            _umDialog = Substitute.For<IUnderMaintenanceDialog>();
            dbLogDialog = Substitute.For<IDBLogDialog>();
            _messagesController = new MessagesController(
                _commonDialog,
                _logDialog,
                _gitLabDialog,
                _lineDialog,
                _umDialog,
                _conversationFixture.Conversation,
                _conversationFixture.Configuration,
                dbLogDialog);
        }

        [Fact]
        public async Task Post_ActivityMessage_HandMessageCommand_LogCommand_CallLogDialog()
        {
            // Arrange
            var activity = new Activity { Type = ActivityTypes.Message, Text = "log add" };

            // Act
            await _messagesController.Post(activity);

            // Asserts
            await _logDialog.Received().HandleMessage(Arg.Is(activity), "log add");
        }

        [Fact]
        public async Task Post_ActivityMessage_HandMessageCommand_GitlabCommand_CallGitLabDialog()
        {
            // Arrange
            var activity = new Activity { Type = ActivityTypes.Message, Text = "gitlab" };

            // Act
            await _messagesController.Post(activity);

            // Asserts
            await _gitLabDialog.Received().HandleMessage(Arg.Is(activity), "gitlab");
        }

        [Fact]
        public async Task Post_ActivityMessage_HandMessageCommand_UMCommand_CallUMDialog()
        {
            // Arrange
            var activity = new Activity { Type = ActivityTypes.Message, Text = "um" };

            // Act
            await _messagesController.Post(activity);

            // Asserts
            await _umDialog.Received().HandleMessage(Arg.Is(activity), "um");
        }

        [Fact]
        public async Task Post_ActivityMessage_HandMessageCommand_OtherCommand_CallDialog()
        {
            // Arrange
            var activity = new Activity { Type = ActivityTypes.Message, Text = "group" };

            // Act
            await _messagesController.Post(activity);

            // Asserts
            await _commonDialog.Received().HandleMessage(Arg.Is(activity), "group");
        }

        [Theory]
        [InlineData("@Skynex group")]
        [InlineData("Skynex group")]
        public async Task Post_ActivityMessage_HandMessageCommand_TextHasBotName_RemoveBotName(string message)
        {
            // Arrange
            var activity = new Activity { Type = ActivityTypes.Message, Text = message };
            _conversationFixture.Configuration.GetSection("BotName")?.Value.Returns("Skynex");

            // Act
            await _messagesController.Post(activity);

            // Asserts
            await _commonDialog.Received().HandleMessage(Arg.Is(activity), "group");
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
                .ReplyAsync(Arg.Is(activity), "Hello. I am SkyNex.");
        }

        [Fact]
        public async Task Post_ActivityContactRelationUpdate_ActionIsRemove_RemoveConversationData()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.ContactRelationUpdate,
                Conversation = new ConversationAccount { Id = "43235grerew" },
                Action = "remove"
            };

            // Act
            await _messagesController.Post(activity);

            // Asserts
            await _commonDialog.Received().RemoveConversationData(Arg.Is(activity));
        }

        [Fact]
        public async Task Post_ActivityContactRelationUpdate_ActionIsNotRemove_RemoveConversationData()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.ContactRelationUpdate,
                Conversation = new ConversationAccount { Id = "43235grerew" },
            };

            // Act
            await _messagesController.Post(activity);

            // Asserts
            await _commonDialog.Received().RegisterMessageInfo(Arg.Is(activity));
            await _conversationFixture
               .Conversation
               .Received()
               .ReplyAsync(Arg.Is(activity), "Hello. I am SkyNex.");
        }

        [Fact]
        public async Task Post_ActivityConverstationUpdate_IsBotRemoved_RemoveConversationData()
        {
            // Arrange
            _conversationFixture.Configuration.GetSection("BotId").Value.Returns("12324342345");
            var activity = new Activity
            {
                Type = ActivityTypes.ConversationUpdate,
                Conversation = new ConversationAccount { Id = "43235grerew" },
                MembersRemoved = new List<ChannelAccount> { new ChannelAccount { Id = "12324342345" } }
            };

            // Act
            await _messagesController.Post(activity);

            // Asserts
            await _commonDialog.Received().RemoveConversationData(Arg.Is(activity));
        }

        [Fact]
        public async Task Post_ActivityConverstationUpdate_IsBotAdded_RemoveConversationData()
        {
            // Arrange
            _conversationFixture.Configuration.GetSection("BotId").Value.Returns("12324342345");
            var activity = new Activity
            {
                Type = ActivityTypes.ConversationUpdate,
                Conversation = new ConversationAccount { Id = "43235grerew" },
                MembersAdded = new List<ChannelAccount> { new ChannelAccount { Id = "12324342345" } }
            };

            // Act
            await _messagesController.Post(activity);

            // Asserts
            await _commonDialog.Received().RegisterMessageInfo(Arg.Is(activity));
            await _conversationFixture
              .Conversation
              .Received()
              .ReplyAsync(Arg.Is(activity), "Hello. I am SkyNex.");
        }

        [Fact]
        public async Task Post_MessageActivity_HandleForLINE()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.Message,
                From = new ChannelAccount { Id = "234234", Name = "line" },
                Conversation = new ConversationAccount(),
                Text = "hello"
            };

            // Act
            await _messagesController.Post(activity);

            // Assert
            await _lineDialog.Received().RegisterMessageInfo(
                 Arg.Is<Activity>(a => a.Conversation.Id == "234234" && a.ChannelId == "line"));
        }

        [Fact]
        public async Task Forward_Always_SendToConversation_ReturnOk()
        {
            // Arrange
            var conversationId = "@#423424";
            var message = "hello";
            var expectedResult = Result.CreateSuccessfulResult();
            _conversationFixture.Conversation.SendAsync(conversationId, message).Returns(expectedResult);

            // Act
            var result = await _messagesController.Forward(message, conversationId);

            // Assert
            await _conversationFixture.Conversation.Received(1).SendAsync(conversationId, message);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(expectedResult, result.Value);
        }
    }
}