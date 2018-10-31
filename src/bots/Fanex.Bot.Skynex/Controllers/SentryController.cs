namespace Fanex.Bot.Skynex.Controllers
{
    using System.Threading.Tasks;
    using Fanex.Bot.Models.Sentry;
    using Fanex.Bot.Skynex.Dialogs;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/[controller]")]
    public class SentryController : Controller
    {
        private readonly ISentryDialog sentryDialog;

        public SentryController(ISentryDialog sentryDialog)
        {
            this.sentryDialog = sentryDialog;
        }

        [HttpPost("hook")]
        public async Task<int> Hook([FromBody]PushEvent pushEvent)
        {
            await sentryDialog.HandlePushEventAsync(pushEvent);

            return 0;
        }
    }
}