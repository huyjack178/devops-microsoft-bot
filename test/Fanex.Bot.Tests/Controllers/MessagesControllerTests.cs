namespace Fanex.Bot.Tests.Controllers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fanex.Bot.Controllers;
    using Fanex.Bot.Skynex.Dialogs;
    using Fanex.Bot.Tests.Fixtures;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Bot.Connector;
    using NSubstitute;
    using Xunit;

    public class MessagesControllerTests : IClassFixture<BotConversationFixture>
    {
        private readonly BotConversationFixture _conversationFixture;
        private readonly IDialog _dialog;
        private readonly ILogDialog _logDialog;
        private readonly IGitLabDialog _gitLabDialog;
        private readonly ILineDialog _lineDialog;
        private readonly IUMDialog _umDialog;
        private readonly MessagesController _messagesController;

        public MessagesControllerTests(BotConversationFixture conversationFixture)
        {
            _conversationFixture = conversationFixture;
            _dialog = Substitute.For<IDialog>();
            _logDialog = Substitute.For<ILogDialog>();
            _gitLabDialog = Substitute.For<IGitLabDialog>();
            _lineDialog = Substitute.For<ILineDialog>();
            _umDialog = Substitute.For<IUMDialog>();

            _messagesController = new MessagesController(
                _dialog,
                _logDialog,
                _gitLabDialog,
                _lineDialog,
                _umDialog,
                _conversationFixture.Conversation,
                _conversationFixture.Configuration);
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
        public async Task Post_ActivityMessage_HandMessageCommand_OtherCommand_CallDialog()
        {
            // Arrange
            var activity = new Activity { Type = ActivityTypes.Message, Text = "group" };

            // Act
            await _messagesController.Post(activity);

            // Asserts
            await _dialog.Received().HandleMessageAsync(Arg.Is(activity), "group");
        }

        [Fact]
        public async Task Post_ActivityMessage_HandMessageCommand_TextHasBotName_RemoveBotName()
        {
            // Arrange
            var activity = new Activity { Type = ActivityTypes.Message, Text = "@Skynex group" };

            // Act
            await _messagesController.Post(activity);

            // Asserts
            await _dialog.Received().HandleMessageAsync(Arg.Is(activity), "group");
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
            await _dialog.Received().RemoveConversationData(Arg.Is(activity));
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
            await _dialog.Received().RegisterMessageInfo(Arg.Is(activity));
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
            await _dialog.Received().RemoveConversationData(Arg.Is(activity));
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
            await _dialog.Received().RegisterMessageInfo(Arg.Is(activity));
            await _conversationFixture
              .Conversation
              .Received()
              .ReplyAsync(Arg.Is(activity), "Hello. I am SkyNex.");
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