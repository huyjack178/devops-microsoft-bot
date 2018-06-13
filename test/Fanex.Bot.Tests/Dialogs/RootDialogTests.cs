namespace Fanex.Bot.Tests.Dialogs
{
    using System.Threading.Tasks;
    using Fanex.Bot.Dialogs.Impl;
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
                _conversationFixture.Configuration,
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
                _conversationFixture.Configuration,
                _conversationFixture.BotDbContext,
                _conversationFixture.Conversation);

            // Act
            await rootDialog.HandleMessageAsync(_conversationFixture.Activity, message);

            // Assert
            var commandMessage = $"Skynex's available commands:{Constants.NewLine}" +
                    $"**log add [Contains-LogCategory]** " +
                        $"==> Register to get log which has category name **contains [Contains-LogCategory]**. " +
                        $"Example: log add Alpha;NAP {Constants.NewLine}" +
                    $"**log remove [LogCategory]**{Constants.NewLine}" +
                    $"**log start** ==> Start receiving logs{Constants.NewLine}" +
                    $"**log stop** ==> Stop receiving logs{Constants.NewLine}" +
                    $"**log detail [LogId] (BETA)** ==> Get log detail{Constants.NewLine}" +
                    $"**log viewStatus** ==> Get your current subscribing Log Categories and Receiving Logs status{Constants.NewLine}" +
                    $"**gitlab addProject [GitlabProjectUrl]** => Register to get notification of Gitlab's project{Constants.NewLine}" +
                    $"**gitlab removeProject [GitlabProjectUrl]** => Disable getting notification of Gitlab's project{Constants.NewLine}" +
                    $"**group** ==> Get your group ID";

            await _conversationFixture
                .Conversation
                .Received()
                .SendAsync(Arg.Is(_conversationFixture.Activity), Arg.Is(commandMessage));
        }

        [Fact]
        public async Task HandleMessageAsync_AnyMessage_SendDefaultMessage()
        {
            // Arrange
            var message = "any";
            var rootDialog = new RootDialog(
                _conversationFixture.Configuration,
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