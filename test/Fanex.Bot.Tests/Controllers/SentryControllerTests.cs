namespace Fanex.Bot.Skynex.Tests.Controllers
{
    using System.Threading.Tasks;
    using Fanex.Bot.Models.Sentry;
    using Fanex.Bot.Skynex.Controllers;
    using Fanex.Bot.Skynex.Dialogs;
    using NSubstitute;
    using Xunit;

    public class SentryControllerTests
    {
        private readonly ISentryDialog subSentryDialog;
        private readonly SentryController sentryController;

        public SentryControllerTests()
        {
            subSentryDialog = Substitute.For<ISentryDialog>();
            sentryController = new SentryController(subSentryDialog);
        }

        [Fact]
        public async Task Hook_Always_CallHandlePushEvent()
        {
            // Arrange
            var pushEvent = new PushEvent();

            // Act
            var result = await sentryController.Hook(pushEvent);

            // Assert
            Assert.Equal(0, result);
            await subSentryDialog.Received().HandlePushEventAsync(pushEvent);
        }
    }
}