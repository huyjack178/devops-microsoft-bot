namespace Fanex.Bot.Skynex.Tests.Dialogs
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Fanex.Bot.Skynex.Dialogs;
    using Fanex.Bot.Models;
    using Fanex.Bot.Models.GitLab;
    using Microsoft.Bot.Connector;
    using Microsoft.EntityFrameworkCore;
    using NSubstitute;
    using Xunit;
    using Fanex.Bot.Skynex.Tests.Fixtures;
    using Fanex.Bot.Skynex.MessageHandlers.MessageBuilders;

    public class GitLabDialogTests : IClassFixture<BotConversationFixture>
    {
        private readonly BotConversationFixture conversationFixture;
        private readonly IGitLabDialog gitLabDialog;

        public GitLabDialogTests(BotConversationFixture conversationFixture)
        {
            this.conversationFixture = conversationFixture;
            gitLabDialog = new GitLabDialog(
                this.conversationFixture.MockDbContext(),
                this.conversationFixture.Conversation,
                new GitLabMessageBuilder());
        }

        [Fact]
        public async Task HandleMessageAsync_AddProject_ProjectUrlIsEmpty_SendErrorMessage()
        {
            // Arrange
            var message = "gitlab addproject";
            conversationFixture.Activity.Conversation.Returns(new ConversationAccount { Id = "13324" });

            // Act
            await gitLabDialog.HandleMessage(conversationFixture.Activity, message);

            // Assert
            await conversationFixture
                .Conversation
                .Received()
                .ReplyAsync(Arg.Is(conversationFixture.Activity), Arg.Is("Please input project url"));
        }

        [Theory]
        [InlineData("http://gitlab.nexdev.vn")]
        [InlineData("https://gitlab.nexdev.vn")]
        [InlineData("<a href='https://gitlab.nexdev.vn'></a>")]
        public async Task HandleMessageAsync_AddProject_ParseProjectUrl_SendMessage(string projectUrl)
        {
            // Arrange
            var message = $"gitlab addproject {projectUrl}";
            conversationFixture.Activity.Conversation.Returns(new ConversationAccount { Id = "1332433" });

            // Act
            await gitLabDialog.HandleMessage(conversationFixture.Activity, message);

            // Assert
            await conversationFixture
                .Conversation
                .Received()
                .ReplyAsync(
                    Arg.Is(conversationFixture.Activity),
                    Arg.Is($"You will receive notification of project {MessageFormatSignal.BOLD_START}gitlab.nexdev.vn{MessageFormatSignal.BOLD_END}"));
        }

        [Fact]
        public async Task HandleMessageAsync_AddProject_RegisterMessageInfo_ExistDb_NotSendAdminMessage()
        {
            // Arrange
            var botDbContext = conversationFixture.MockDbContext();
            botDbContext.MessageInfo.Add(new MessageInfo { ConversationId = "3333" });
            await botDbContext.SaveChangesAsync();
            var message = "gitlab addproject http://gitlab.nexdev.vn";
            conversationFixture.Activity.Conversation.Returns(new ConversationAccount { Id = "3333" });

            // Act
            await gitLabDialog.HandleMessage(conversationFixture.Activity, message);

            // Assert
            Assert.True(conversationFixture.BotDbContext.MessageInfo.AsNoTracking()
                .Any(info => info.ConversationId == "3333"));
        }

        [Fact]
        public async Task HandleMessageAsync_AddProject_SaveGitLabInfo_SendMessage()
        {
            // Arrange
            var message = "gitlab addproject http://gitlab.nexdev.vn";
            conversationFixture.Activity.Conversation.Returns(new ConversationAccount { Id = "5" });

            // Act
            await gitLabDialog.HandleMessage(conversationFixture.Activity, message);

            // Assert
            await conversationFixture
                .Conversation
                .Received()
                .ReplyAsync(
                    Arg.Is(conversationFixture.Activity),
                    Arg.Is($"You will receive notification of project {MessageFormatSignal.BOLD_START}gitlab.nexdev.vn{MessageFormatSignal.BOLD_END}"));

            Assert.True(conversationFixture.BotDbContext.GitLabInfo.AsNoTracking()
                .Any(info => info.ConversationId == "5" && info.ProjectUrl == "gitlab.nexdev.vn"));
        }

        [Fact]
        public async Task HandleMessageAsync_RemoveProject_ProjectUrlIsEmpty_SendMessage()
        {
            // Arrange
            var message = "gitlab removeproject";
            conversationFixture.Activity.Conversation.Returns(new ConversationAccount { Id = "6" });

            // Act
            await gitLabDialog.HandleMessage(conversationFixture.Activity, message);

            // Assert
            await conversationFixture
                .Conversation
                .Received()
                .ReplyAsync(Arg.Is(conversationFixture.Activity), Arg.Is("Please input project url"));
        }

        [Fact]
        public async Task HandleMessageAsync_RemoveProject_NotExistProject_SendMessage()
        {
            // Arrange
            var message = "gitlab removeproject http://gitlab.nexdev.vn";
            conversationFixture.Activity.Conversation.Returns(new ConversationAccount { Id = "7" });

            // Act
            await gitLabDialog.HandleMessage(conversationFixture.Activity, message);

            // Assert
            await conversationFixture
                .Conversation
                .Received()
                .ReplyAsync(Arg.Is(conversationFixture.Activity), Arg.Is("Project not found"));
        }

        [Fact]
        public async Task HandleMessageAsync_RemoveProject_ExistProject_SendMessage()
        {
            // Arrange
            var botDbContext = conversationFixture.MockDbContext();
            botDbContext.GitLabInfo.Add(new GitLabInfo { ConversationId = "33", ProjectUrl = "gitlab.nexdev.vn/Bot" });
            await botDbContext.SaveChangesAsync();

            var message = "gitlab removeproject http://gitlab.nexdev.vn/Bot";
            conversationFixture.Activity.Conversation.Returns(new ConversationAccount { Id = "33" });

            // Act
            await gitLabDialog.HandleMessage(conversationFixture.Activity, message);

            // Assert
            await conversationFixture
                .Conversation
                .Received()
                .ReplyAsync(
                    Arg.Is(conversationFixture.Activity),
                    Arg.Is($"You will not receive notification of project {MessageFormatSignal.BOLD_START}gitlab.nexdev.vn/Bot{MessageFormatSignal.BOLD_END}"));
        }

        [Fact]
        public async Task HandlePushEventAsync_MasterBranch_HasGitLabInfo_SendPushMessageMessage()
        {
            // Arrange
            var botDbContext = conversationFixture.MockDbContext();
            botDbContext.GitLabInfo.Add(
                new GitLabInfo
                {
                    ConversationId = "33",
                    ProjectUrl = "gitlab.nexdev.vn/bot",
                    IsActive = true
                });
            await botDbContext.SaveChangesAsync();
            var pushEvent = new PushEvent
            {
                Project = new Project { WebUrl = "http://gitlab.nexdev.vn/Bot" },
                Commits = new List<Commit> {
                    new Commit {
                        Author = new Author { Name = "Harrison" },
                        Id = "12345678910",
                        Message = "Push Master" }
                },
                Ref = "heads/master"
            };

            // Act
            await gitLabDialog.HandlePushEventAsync(pushEvent);

            // Assert
            var expectedMessage =
                    $"{MessageFormatSignal.BOLD_START}GitLab Master Branch Change{MessageFormatSignal.BOLD_END} (bell){MessageFormatSignal.NEWLINE}" +
                    $"{MessageFormatSignal.BOLD_START}Repository:{MessageFormatSignal.BOLD_END} http://gitlab.nexdev.vn/Bot" + MessageFormatSignal.NEWLINE +
                    $"{MessageFormatSignal.BOLD_START}Commits:{MessageFormatSignal.BOLD_END}{MessageFormatSignal.NEWLINE}" +
                    $"{MessageFormatSignal.BOLD_START}[12345678](http://gitlab.nexdev.vn/Bot/commit/12345678910){MessageFormatSignal.BOLD_END} " +
                    $"Push Master (Harrison){MessageFormatSignal.NEWLINE}" +
                    MessageFormatSignal.DIVIDER + MessageFormatSignal.NEWLINE;

            await conversationFixture.Conversation.Received().SendAsync("33", Arg.Is(expectedMessage));
        }

        [Fact]
        public async Task HandleMessageAsync_AnyMessage_SendCommandMessage()
        {
            // Arrange
            var message = "gitlab";
            conversationFixture.Activity.Conversation.Returns(new ConversationAccount { Id = "34" });

            // Act
            await gitLabDialog.HandleMessage(conversationFixture.Activity, message);

            // Assert
            await conversationFixture
                .Conversation
                .Received()
                .ReplyAsync(Arg.Is(conversationFixture.Activity), Arg.Is(conversationFixture.CommandMessage));
        }
    }
}