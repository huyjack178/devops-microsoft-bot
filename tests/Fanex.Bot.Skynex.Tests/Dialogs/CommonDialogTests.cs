using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Bot.Core._Shared.Constants;
using Fanex.Bot.Core.Bot.Models;
using Fanex.Bot.Core.GitLab.Models;
using Fanex.Bot.Core.Log.Models;
using Fanex.Bot.Skynex.Bot;
using Fanex.Bot.Skynex.Tests.Fixtures;
using Microsoft.Bot.Connector;
using NSubstitute;
using Xunit;

namespace Fanex.Bot.Skynex.Tests.Dialogs
{
    public class CommonDialogTests : IClassFixture<BotConversationFixture>
    {
        private readonly BotConversationFixture conversationFixture;
        private readonly IMessengerDialog dialog;

        public CommonDialogTests(BotConversationFixture conversationFixture)
        {
            this.conversationFixture = conversationFixture;

            dialog = new SkypeDialog(
                conversationFixture.BotDbContext,
                conversationFixture.Conversation,
                conversationFixture.Configuration);
        }

        [Fact]
#pragma warning disable S2699 // Tests should include assertions
        public async Task HandleMessageAsync_MessageGroup_SendGroupId()
#pragma warning restore S2699 // Tests should include assertions
        {
            // Arrange
            var message = "group";
            conversationFixture.Activity.Conversation.Returns(new ConversationAccount { Id = "13324" });

            // Act
            await dialog.HandleMessage(conversationFixture.Activity, message);

            // Assert
            await conversationFixture
                .Conversation
                .Received()
                .ReplyAsync(Arg.Is(conversationFixture.Activity), Arg.Is("Your group id is: 13324"));
        }

        [Fact]
#pragma warning disable S2699 // Tests should include assertions
        public async Task HandleMessageAsync_MessageHelp_SendCommand()
#pragma warning restore S2699 // Tests should include assertions
        {
            // Arrange
            var message = "help";

            // Act
            await dialog.HandleMessage(conversationFixture.Activity, message);

            // Assert
            await conversationFixture
                .Conversation
                .Received()
                .ReplyAsync(Arg.Is(conversationFixture.Activity), Arg.Is(conversationFixture.CommandMessage));
        }

        [Fact]
#pragma warning disable S2699 // Tests should include assertions
        public async Task HandleMessageAsync_AnyMessage_SendDefaultMessage()
#pragma warning restore S2699 // Tests should include assertions
        {
            // Arrange
            var message = "any";

            // Act
            await dialog.HandleMessage(conversationFixture.Activity, message);

            // Assert
            await conversationFixture
                .Conversation
                .Received()
                .ReplyAsync(
                    Arg.Is(conversationFixture.Activity),
                    Arg.Is($"Please send {MessageFormatSymbol.BOLD_START}help{MessageFormatSymbol.BOLD_END} to get my commands"));
        }

        [Fact]
        public async Task HandleConversationUpdate_IsBotRemoved_RemoveConversationData()
        {
            // Arrange
            var dbContext = conversationFixture.MockDbContext();
            await dbContext.LogInfo.AddAsync(new LogInfo { ConversationId = "43235grerew", LogCategories = "alpha" });
            await dbContext.GitLabInfo.AddAsync(new GitLabInfo { ConversationId = "43235grerew", ProjectUrl = "3424" });
            await dbContext.MessageInfo.AddAsync(new MessageInfo { ConversationId = "43235grerew" });
            await dbContext.SaveChangesAsync();

            conversationFixture.Configuration.GetSection("BotId").Value.Returns("12324342345");
            var activity = new Activity
            {
                Type = ActivityTypes.ConversationUpdate,
                Conversation = new ConversationAccount { Id = "43235grerew" },
                MembersRemoved = new List<ChannelAccount> { new ChannelAccount { Id = "12324342345" } }
            };

            // Act
            await dialog.HandleConversationUpdate(activity);

            // Asserts
            Assert.False(
                conversationFixture
                    .BotDbContext
                    .MessageInfo
                    .Any(info => info.ConversationId == "43235grerew"));

            Assert.False(
                conversationFixture
                    .BotDbContext
                    .GitLabInfo
                    .Any(info => info.ConversationId == "43235grerew"));

            Assert.False(
                conversationFixture
                    .BotDbContext
                    .LogInfo
                    .Any(info => info.ConversationId == "43235grerew"));

            await conversationFixture
                .Conversation
                .Received()
                .SendAdminAsync($"Client {MessageFormatSymbol.BOLD_START}43235grerew{MessageFormatSymbol.BOLD_END} has been removed");
        }

        [Fact]
        public async Task HandleConversationUpdate_IsBotAdded_RegisterConversationData()
        {
            // Arrange
            conversationFixture.Configuration.GetSection("BotId").Value.Returns("12324342345");
            var activity = new Activity
            {
                Type = ActivityTypes.ConversationUpdate,
                Conversation = new ConversationAccount { Id = "13324dfwer234" },
                MembersAdded = new List<ChannelAccount> { new ChannelAccount { Id = "12324342345" } }
            };

            // Act
            await dialog.HandleConversationUpdate(activity);

            // Asserts
            Assert.True(
                conversationFixture
                    .BotDbContext
                    .MessageInfo
                    .Any(info => info.ConversationId == "13324dfwer234"));

            await conversationFixture
                .Conversation
                .Received()
                .SendAdminAsync($"New client {MessageFormatSymbol.BOLD_START}13324dfwer234{MessageFormatSymbol.BOLD_END} has been added");

            await conversationFixture
                .Conversation
                .Received()
                .ReplyAsync(Arg.Is(activity), "Hello. I am SkyNex.");
        }

        [Fact]
        public async Task HandleContactRelationUpdate_IsNotRemovedAction_RegisterMessageInfo()
        {
            // Arrange
            var activity = new Activity
            {
                Type = ActivityTypes.ContactRelationUpdate,
                Conversation = new ConversationAccount { Id = "43235g43235gre324234rewrerew" },
            };

            // Act
            await dialog.HandleContactRelationUpdate(activity);

            // Asserts
            Assert.True(
                conversationFixture
                    .BotDbContext
                    .MessageInfo
                    .Any(info => info.ConversationId == "43235g43235gre324234rewrerew"));

            await conversationFixture
                .Conversation
                .Received()
                .SendAdminAsync($"New client {MessageFormatSymbol.BOLD_START}43235g43235gre324234rewrerew{MessageFormatSymbol.BOLD_END} has been added");

            await conversationFixture
                .Conversation
                .Received()
                .ReplyAsync(Arg.Is(activity), "Hello. I am SkyNex.");
        }

        [Fact]
        public async Task HandleContactRelationUpdate_RemoveDataAndSendAdmin()
        {
            // Arrange
            conversationFixture.Activity = new Activity
            {
                Type = ActivityTypes.ContactRelationUpdate,
                Conversation = new ConversationAccount { Id = "13324d234234fwer234" },
                Action = "remove"
            };

            var dbContext = conversationFixture.MockDbContext();
            await dbContext.LogInfo.AddAsync(new LogInfo { ConversationId = "13324d234234fwer234", LogCategories = "alpha" });
            await dbContext.GitLabInfo.AddAsync(new GitLabInfo { ConversationId = "13324d234234fwer234", ProjectUrl = "3424" });
            await dbContext.MessageInfo.AddAsync(new MessageInfo { ConversationId = "13324d234234fwer234" });
            await dbContext.SaveChangesAsync();

            // Act
            await dialog.HandleContactRelationUpdate(conversationFixture.Activity);

            // Assert
            Assert.False(
                conversationFixture
                    .BotDbContext
                    .MessageInfo
                    .Any(info => info.ConversationId == "13324d234234fwer234"));

            Assert.False(
                conversationFixture
                    .BotDbContext
                    .GitLabInfo
                    .Any(info => info.ConversationId == "13324d234234fwer234"));

            Assert.False(
              conversationFixture
                  .BotDbContext
                  .LogInfo
                  .Any(info => info.ConversationId == "13324d234234fwer234"));

            await conversationFixture
                .Conversation
                .Received()
                .SendAdminAsync($"Client {MessageFormatSymbol.BOLD_START}13324d234234fwer234{MessageFormatSymbol.BOLD_END} has been removed");
        }
    }
}