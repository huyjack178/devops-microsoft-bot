namespace Fanex.Bot.Tests.Dialogs
{
    using System.Linq;
    using System.Threading.Tasks;
    using Fanex.Bot.Dialogs;
    using Fanex.Bot.Dialogs.Impl;
    using Fanex.Bot.Models;
    using Fanex.Bot.Models.GitLab;
    using Fanex.Bot.Models.Log;
    using Fanex.Bot.Tests.Fixtures;
    using Microsoft.Bot.Connector;
    using Microsoft.EntityFrameworkCore;
    using NSubstitute;
    using Xunit;

    public class DialogTests : IClassFixture<BotConversationFixture>
    {
        private readonly BotConversationFixture _conversationFixture;
        private readonly IDialog _dialog;

        public DialogTests(BotConversationFixture conversationFixture)
        {
            _conversationFixture = conversationFixture;
            _dialog = new Dialog(_conversationFixture.BotDbContext, conversationFixture.Conversation);
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
                .SendAdminAsync($"New client **13324dfwer234** has been added");
        }

        [Fact]
        public async Task RemoveConversationDatat_RemoveDataAndSendAdmin()
        {
            // Arrange
            _conversationFixture.Activity.Conversation.Returns(new ConversationAccount { Id = "13324d234234fwer234" });
            var dbContext = _conversationFixture.MockDbContext();
            await dbContext.LogInfo.AddAsync(new LogInfo { ConversationId = "13324d234234fwer234" });
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
                .SendAdminAsync($"Client **13324d234234fwer234** has been removed");
        }
    }
}