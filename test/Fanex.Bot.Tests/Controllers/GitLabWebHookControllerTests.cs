namespace Fanex.Bot.Tests.Controllers
{
    using System.Threading.Tasks;
    using Fanex.Bot.Controllers;
    using Fanex.Bot.Dialogs;
    using Fanex.Bot.Models.GitLab;
    using NSubstitute;
    using Xunit;

    public class GitLabWebHookControllerTests
    {
        private readonly IGitLabDialog _gitLabDialog;

        public GitLabWebHookControllerTests()
        {
            _gitLabDialog = Substitute.For<IGitLabDialog>();
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
            var result = await new GitLabWebHookController(_gitLabDialog).PushEventInfo(pushEvent);

            // Assert
            await _gitLabDialog.Received().HandlePushEventAsync(Arg.Is(pushEvent));
            Assert.Equal(0, result);
        }
    }
}