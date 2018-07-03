namespace Fanex.Bot.Skynex.Line.Controllers
{
    using System.Configuration;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;
    using global::Line.Messaging.Webhooks;

    public class LineBotController : ApiController
    {
        private readonly ILineBotApp _lineBotApp;
        private readonly string channelSecret = ConfigurationManager.AppSettings["ChannelSecret"];

        public LineBotController(ILineBotApp lineBotApp)
        {
            _lineBotApp = lineBotApp;
        }

#pragma warning disable S3216 // "ConfigureAwait(false)" should be used

        [HttpPost]
        public async Task<HttpResponseMessage> Post(HttpRequestMessage request)
        {
            var events = await request.GetWebhookEventsAsync(channelSecret);

            await _lineBotApp.RunAsync(events);

            return Request.CreateResponse(HttpStatusCode.OK);
        }

#pragma warning restore S3216 // "ConfigureAwait(false)" should be used
    }
}