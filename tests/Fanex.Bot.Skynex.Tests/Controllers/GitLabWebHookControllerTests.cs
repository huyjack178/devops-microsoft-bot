namespace Fanex.Bot.Tests.Controllers
{
    using System.Threading.Tasks;
    using Fanex.Bot.Models.GitLab;
    using Fanex.Bot.Skynex.Controllers;
    using Fanex.Bot.Skynex.Dialogs;
    using NSubstitute;
    using Xunit;

    public class GitLabWebHookControllerTests
    {
        private readonly IGitLabDialog gitLabDialog;

        public GitLabWebHookControllerTests()
        {
            gitLabDialog = Substitute.For<IGitLabDialog>();
        }

        [Fact]
        public async Task PushEventInfo_Always_HandlePushEventGitLab()
        {
            // Arrange
            var pushEvent = new PushEvent
            {
                Project = new Project { WebUrl = "http://gitlab.nexdev.net" }
            };

            // Act
            var result = await new GitLabWebHookController(gitLabDialog).PushEventInfo(pushEvent);

            // Assert
            await gitLabDialog.Received().HandlePushEventAsync(Arg.Is(pushEvent));
            Assert.Equal(0, result);
        }
    }
}