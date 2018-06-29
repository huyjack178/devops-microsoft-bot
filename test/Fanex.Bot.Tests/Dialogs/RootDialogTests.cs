namespace Fanex.Bot.Tests.Dialogs
{
    using System.Threading.Tasks;
    using Fanex.Bot.Dialogs;
    using Fanex.Bot.Tests.Fixtures;
    using Microsoft.Bot.Connector;
    using NSubstitute;
    using Xunit;

    public class RootDialogTests : IClassFixture<BotConversationFixture>
    {
        private readonly BotConversationFixture _conversationFixture;

        public RootDialogTests(BotConversationFixture conversationFixture)
        {
            _conversationFixture = conversationFixture;
        }

        [Fact]
        public async Task HandleMessageAsync_MessageGroup_SendGroupId()
        {
            // Arrange
            var message = "group";
            var rootDialog = new RootDialog(
                _conversationFixture.BotDbContext,
                _conversationFixture.Conversation);

            _conversationFixture.Activity.Conversation.Returns(new ConversationAccount { Id = "13324" });

            // Act
            await rootDialog.HandleMessageAsync(_conversationFixture.Activity, message);

            // Assert
            await _conversationFixture
                .Conversation
                .Received()
                .SendAsync(Arg.Is(_conversationFixture.Activity), Arg.Is("Your group id is: 13324"));
        }

        [Fact]
        public async Task HandleMessageAsync_MessageHelp_SendCommand()
        {
            // Arrange
            var message = "help";
            var rootDialog = new RootDialog(
                _conversationFixture.BotDbContext,
                _conversationFixture.Conversation);
            _conversationFixture.Activity.Conversation.Returns(new ConversationAccount { Id = "9999" });
            // Act
            await rootDialog.HandleMessageAsync(_conversationFixture.Activity, message);

            // Assert
            await _conversationFixture
                .Conversation
                .Received()
                .SendAsync(Arg.Is(_conversationFixture.Activity), Arg.Is(_conversationFixture.CommandMessage));
        }

        [Fact]
        public async Task HandleMessageAsync_AnyMessage_SendDefaultMessage()
        {
            // Arrange
            var message = "any";
            var rootDialog = new RootDialog(
                _conversationFixture.BotDbContext,
                _conversationFixture.Conversation);

            // Act
            await rootDialog.HandleMessageAsync(_conversationFixture.Activity, message);

            // Assert
            await _conversationFixture
                .Conversation
                .Received()
                .SendAsync(Arg.Is(_conversationFixture.Activity), Arg.Is("Please send **help** to get my commands"));
        }
    }
}