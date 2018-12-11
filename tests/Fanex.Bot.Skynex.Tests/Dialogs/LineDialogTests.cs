namespace Fanex.Bot.Skynex.Tests.Dialogs
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Fanex.Bot.Skynex.Dialogs;
    using Fanex.Bot.Skynex.Tests.Fixtures;
    using Microsoft.Bot.Connector;
    using NSubstitute;
    using Xunit;

    public class LineDialogTests : IClassFixture<BotConversationFixture>
    {
        private readonly BotConversationFixture _conversationFixture;
        private readonly ILineDialog _dialog;

        public LineDialogTests(BotConversationFixture conversationFixture)
        {
            _conversationFixture = conversationFixture;
            _dialog = new LineDialog(_conversationFixture.BotDbContext, conversationFixture.Conversation);
        }

        [Fact]
        public async Task RegisterMessageInfo_MessageDoesNotExist_AddMessageInfoAndSendAdmin()
        {
            // Arrange
            _conversationFixture.Activity.From.Returns(new ChannelAccount { Id = "13324dfwer223423434" });

            // Act
            await _dialog.RegisterMessageInfo(_conversationFixture.Activity);

            // Assert
            Assert.True(
                _conversationFixture
                    .BotDbContext
                    .MessageInfo
                    .Any(info => info.ConversationId == "13324dfwer223423434" && info.ChannelId == "line"));

            await _conversationFixture
                .Conversation
                .Received()
                .SendAdminAsync($"New client {MessageFormatSignal.BeginBold}13324dfwer223423434{MessageFormatSignal.EndBold} has been added");
        }

        [Fact]
        public async Task RemoveConversationDatat_RemoveDataAndSendAdmin()
        {
            // Assert
            await Assert.ThrowsAsync<NotImplementedException>(
                async () => await _dialog.RemoveConversationData(_conversationFixture.Activity));
        }
    }
}