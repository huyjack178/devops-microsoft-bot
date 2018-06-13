namespace Fanex.Bot.Tests.Dialogs
{
    using System.Linq;
    using System.Threading.Tasks;
    using Fanex.Bot.Dialogs;
    using Fanex.Bot.Dialogs.Impl;
    using Fanex.Bot.Models;
    using Fanex.Bot.Models.GitLab;
    using Fanex.Bot.Tests.Fixtures;
    using Microsoft.Bot.Connector;
    using Microsoft.EntityFrameworkCore;
    using NSubstitute;
    using Xunit;

    public class GitLabDialogTests : IClassFixture<BotConversationFixture>
    {
        private readonly BotConversationFixture _conversationFixture;
        private readonly IGitLabDialog _gitLabDialog;

        public GitLabDialogTests(BotConversationFixture conversationFixture)
        {
            _conversationFixture = conversationFixture;
            _gitLabDialog = new GitLabDialog(
               _conversationFixture.BotDbContext,
               _conversationFixture.Conversation);
        }

        [Fact]
        public async Task HandleMessageAsync_AddProject_ProjectUrlIsEmpty_SendErrorMessage()
        {
            // Arrange
            var message = "gitlab addproject";
            _conversationFixture.Activity.Conversation.Returns(new ConversationAccount { Id = "13324" });

            // Act
            await _gitLabDialog.HandleMessageAsync(_conversationFixture.Activity, message);

            // Assert
            await _conversationFixture
                .Conversation
                .Received()
                .SendAsync(Arg.Is(_conversationFixture.Activity), Arg.Is("Please input project url"));
        }

        [Fact]
        public async Task HandleMessageAsync_AddProject_RegisterMessageInfo_NotExistDb_SendAdminMessage()
        {
            // Arrange
            var message = "gitlab addproject http://gitlab.nexdev.vn";
            _conversationFixture.Activity.Conversation.Returns(new ConversationAccount { Id = "4" });

            // Act
            await _gitLabDialog.HandleMessageAsync(_conversationFixture.Activity, message);

            // Assert
            await _conversationFixture
                .Conversation
                .Received()
                .SendAdminAsync(Arg.Is("New client **4** has been added"));
            Assert.True(_conversationFixture.BotDbContext.MessageInfo.AsNoTracking().Any(info => info.ConversationId == "4"));
        }

        [Fact]
        public async Task HandleMessageAsync_AddProject_RegisterMessageInfo_ExistDb_NotSendAdminMessage()
        {
            // Arrange
            var botDbContext = _conversationFixture.MockDbContext();
            botDbContext.MessageInfo.Add(new MessageInfo { ConversationId = "3" });
            await botDbContext.SaveChangesAsync();
            var message = "gitlab addproject http://gitlab.nexdev.vn";
            _conversationFixture.Activity.Conversation.Returns(new ConversationAccount { Id = "3" });

            // Act
            await _gitLabDialog.HandleMessageAsync(_conversationFixture.Activity, message);

            // Assert
            Assert.True(_conversationFixture.BotDbContext.MessageInfo.AsNoTracking().Any(info => info.ConversationId == "3"));
        }

        [Fact]
        public async Task HandleMessageAsync_AddProject_SaveGitLabInfo_SendMessage()
        {
            // Arrange
            var message = "gitlab addproject http://gitlab.nexdev.vn";
            _conversationFixture.Activity.Conversation.Returns(new ConversationAccount { Id = "5" });

            // Act
            await _gitLabDialog.HandleMessageAsync(_conversationFixture.Activity, message);

            // Assert
            await _conversationFixture
                .Conversation
                .Received()
                .SendAsync(Arg.Is(_conversationFixture.Activity), Arg.Is("You will receive notification of project **gitlab.nexdev.vn**"));

            Assert.True(_conversationFixture.BotDbContext.GitLabInfo.AsNoTracking()
                .Any(info => info.ConversationId == "5" && info.ProjectUrl == "gitlab.nexdev.vn"));
        }

        [Fact]
        public async Task HandleMessageAsync_RemoveProject_ProjectUrlIsEmpty_SendMessage()
        {
            // Arrange
            var message = "gitlab removeproject";
            _conversationFixture.Activity.Conversation.Returns(new ConversationAccount { Id = "6" });

            // Act
            await _gitLabDialog.HandleMessageAsync(_conversationFixture.Activity, message);

            // Assert
            await _conversationFixture
                .Conversation
                .Received()
                .SendAsync(Arg.Is(_conversationFixture.Activity), Arg.Is("Please input project url"));
        }

        [Fact]
        public async Task HandleMessageAsync_RemoveProject_NotExistProject_SendMessage()
        {
            // Arrange
            var message = "gitlab removeproject http://gitlab.nexdev.vn";
            _conversationFixture.Activity.Conversation.Returns(new ConversationAccount { Id = "7" });

            // Act
            await _gitLabDialog.HandleMessageAsync(_conversationFixture.Activity, message);

            // Assert
            await _conversationFixture
                .Conversation
                .Received()
                .SendAsync(Arg.Is(_conversationFixture.Activity), Arg.Is("Project not found"));
        }

        [Fact]
        public async Task HandleMessageAsync_RemoveProject_ExistProject_SendMessage()
        {
            // Arrange
            var botDbContext = _conversationFixture.MockDbContext();
            botDbContext.GitLabInfo.Add(new GitLabInfo { ConversationId = "33", ProjectUrl = "gitlab.nexdev.vn/Bot" });
            await botDbContext.SaveChangesAsync();

            var message = "gitlab removeproject http://gitlab.nexdev.vn/Bot";
            _conversationFixture.Activity.Conversation.Returns(new ConversationAccount { Id = "33" });

            // Act
            await _gitLabDialog.HandleMessageAsync(_conversationFixture.Activity, message);

            // Assert
            await _conversationFixture
                .Conversation
                .Received()
                .SendAsync(Arg.Is(_conversationFixture.Activity), Arg.Is("You will not receive notification of project **gitlab.nexdev.vn/Bot**"));
        }
    }
}