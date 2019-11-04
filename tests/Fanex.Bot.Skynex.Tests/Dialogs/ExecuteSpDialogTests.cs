namespace Fanex.Bot.Skynex.Tests.Dialogs
{
    using System.Threading.Tasks;
    using Fanex.Bot.Core.ExecuteSP.Models;
    using Fanex.Bot.Core.ExecuteSP.Services;
    using Fanex.Bot.Skynex.ExecuteSP;
    using Fanex.Bot.Skynex.Tests.Fixtures;
    using Microsoft.Bot.Connector;
    using NSubstitute;
    using Xunit;

    public class ExecuteSpDialogTests : IClassFixture<BotConversationFixture>
    {
        private readonly BotConversationFixture conversationFixture;
        private readonly IExecuteSpDialog executeSpDialog;
        private readonly IExecuteSpService executeSpService;

        public ExecuteSpDialogTests(BotConversationFixture conversationFixture)
        {
            this.conversationFixture = conversationFixture;
            executeSpService = Substitute.For<IExecuteSpService>();
            executeSpDialog = new ExecuteSpDialog(
                this.conversationFixture.MockDbContext(),
                this.conversationFixture.Conversation,
                executeSpService);
        }

        [Fact]
#pragma warning disable S2699 // Tests should include assertions
        public async Task HandleMessageAsync_StopLogging_WithDefaultStopTime_SendSuccessMessage()
#pragma warning restore S2699 // Tests should include assertions
        {
            var message = "query commands";
            conversationFixture.Activity.Conversation.Returns(new ConversationAccount { Id = "5" });

            var result = new ExecuteSpResult
            {
                IsSuccessful = true,
                Message = "abc"
            };
            executeSpService.ExecuteSpWithParams(Arg.Any<string>(), Arg.Any<string>()).Returns(result);
            await executeSpDialog.HandleMessage(conversationFixture.Activity, message);

            await conversationFixture
                .Conversation
                .Received()
                .ReplyAsync(Arg.Is(conversationFixture.Activity),
                Arg.Is(result.Message));
        }
    }
}