using Fanex.Bot.Models;
using Fanex.Bot.Models.Sentry;
using Fanex.Bot.Skynex.Dialogs;
using Fanex.Bot.Skynex.MessageHandlers.MessageSenders;
using Fanex.Bot.Skynex.Tests.Fixtures;
using Microsoft.Bot.Connector;
using NSubstitute;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Fanex.Bot.Skynex.Tests.Dialogs
{
    public class SentryDialogTests : IClassFixture<BotConversationFixture>
    {
        private readonly BotConversationFixture conversationFixture;
        private readonly ISentryDialog sentryDialog;

        public SentryDialogTests(BotConversationFixture conversationFixture)
        {
            this.conversationFixture = conversationFixture;
            sentryDialog = new SentryDialog(
                conversationFixture.BotDbContext,
                conversationFixture.Conversation);
        }

        [Fact]
        public async Task HandleMessage_StartCommand_HasSentryInfo_EnableLog()
        {
            // Arrange
            var message = "sentrylog start";
            conversationFixture.BotDbContext.SentryInfo.Add(new SentryInfo { ConversationId = "12345", Project = "all" });
            await conversationFixture.BotDbContext.SaveChangesAsync();
            conversationFixture.Activity.Conversation.Id = "12345";

            // Act
            await sentryDialog.HandleMessage(conversationFixture.Activity, message);

            // Assert
            Assert.True(conversationFixture.BotDbContext.SentryInfo.Any(info => info.ConversationId == "12345" && info.IsActive));
            await conversationFixture.Conversation.Received().ReplyAsync(conversationFixture.Activity, "Sentry Log has been enabled!");
        }

        [Fact]
        public async Task HandleMessage_StartCommand_HasNoSentryInfo_EnableLog()
        {
            // Arrange
            var message = "sentrylog start";
            conversationFixture.Activity.Conversation.Id = "1234567";

            // Act
            await sentryDialog.HandleMessage(conversationFixture.Activity, message);

            // Assert
            Assert.True(conversationFixture.BotDbContext.SentryInfo.Any(info => info.ConversationId == "1234567" && info.Project == "all" && info.IsActive));
        }

        [Fact]
        public async Task HandleMessage_StopCommand_HasSentryInfo_DisableLog()
        {
            // Arrange
            var message = "sentrylog stop";
            conversationFixture.BotDbContext.SentryInfo.Add(new SentryInfo { ConversationId = "123456", Project = "all" });
            await conversationFixture.BotDbContext.SaveChangesAsync();
            conversationFixture.Activity.Conversation.Id = "123456";

            // Act
            await sentryDialog.HandleMessage(conversationFixture.Activity, message);

            // Assert
            Assert.True(conversationFixture.BotDbContext.SentryInfo.Any(info => info.ConversationId == "123456" && !info.IsActive));
            await conversationFixture.Conversation.Received().ReplyAsync(conversationFixture.Activity, "Sentry Log has been disabled!");
        }

        [Fact]
        public async Task HandlePushEvent_Always_SendMessageToActiveSentryInfo()
        {
            // Arrange
            conversationFixture.BotDbContext.SentryInfo.Add(new SentryInfo { ConversationId = "123456789", Project = "all", IsActive = true });
            await conversationFixture.BotDbContext.SaveChangesAsync();
            var pushEvent = new PushEvent
            {
                Project = "Alpha",
                Event = new Event
                {
                    LogTime = "1541148185",
                    Message = new Message { MessageInfo = "Message" },
                    User = new User { UserName = "harrison", Email = "harrison@gmail.com" }
                },
                Url = "http://sentry.log"
            };

            // Act
            await sentryDialog.HandlePushEventAsync(pushEvent);

            // Assert
            await conversationFixture.Conversation.Received().SendAsync(
                "123456789",
                "{{BeginBold}}Project:{{EndBold}} {{NewLine}}{{BeginBold}}Timestamp:{{EndBold}} 11/2/2018 8:43:05 AM +00:00{{NewLine}}{{BeginBold}}Message:{{EndBold}} Message{{NewLine}}{{BeginBold}}User:{{EndBold}} harrison (harrison@gmail.com){{NewLine}}{{BeginBold}}Url:{{EndBold}} http://sentry.log{{NewLine}}{{BreakLine}}");
        }
    }
}