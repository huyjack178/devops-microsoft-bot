using System.Threading.Tasks;
using Fanex.Bot.Core.Sentry.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Fanex.Bot.Skynex.Sentry
{
    [Route("api/[controller]")]
    public class SentryController : Controller
    {
        private readonly ISentryDialog sentryDialog;

        public SentryController(ISentryDialog sentryDialog)
        {
            this.sentryDialog = sentryDialog;
        }

        [HttpPost("hook")]
        public async Task<int> Hook([FromBody]object payload)
        {
            var pushEvent = JsonConvert.DeserializeObject<PushEvent>(payload.ToString());
            await sentryDialog.HandlePushEventAsync(pushEvent);

            return 0;
        }
    }
}