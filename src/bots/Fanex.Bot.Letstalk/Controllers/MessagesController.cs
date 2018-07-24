namespace Fanex.Bot.Letstalk.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Fanex.Bot.Core.Utilities.Bot;
    using Fanex.Bot.Core.Utilities.Common;
    using Fanex.Bot.Core.Utilities.Web;
    using Fanex.Bot.Letstalk.Models.WebHookRequest;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Bot.Connector;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using RestSharp;

    [Route("api/[controller]")]
    public class MessagesController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _memoryCache;
        private readonly IWebClient _webClient;

        public MessagesController(
            IConfiguration configuration,
            IMemoryCache memoryCache,
            IWebClient webClient)
        {
            _configuration = configuration;
            _memoryCache = memoryCache;
            _webClient = webClient;
        }

        [Authorize(Roles = "Bot")]
        [HttpPost]
        public async Task<OkResult> Post([FromBody] Activity activity)
        {
            _memoryCache.Set(
                activity.Conversation.Id,
                activity,
                new MemoryCacheEntryOptions { SlidingExpiration = TimeSpan.FromHours(24) });

            if (activity.Type == ActivityTypes.Message)
            {
                var message = BotHelper.GenerateMessage(activity.Text);

                if (message.StartsWith("group"))
                {
                    await Send(activity, $"Your group id is: {activity.Conversation.Id}");
                }
                else
                {
                    await PushToAlphaWebhook(activity, message);
                }
            }

            return Ok();
        }

        [Authorize(Roles = "Bot")]
        [HttpPost]
        [Route("Forward")]
        public async Task<IActionResult> Forward(string message, string conversationId)
        {
            var activity = _memoryCache.Get<Activity>(conversationId);
            await Send(activity, message);

            return Ok();
        }

        private async Task PushToAlphaWebhook(Activity activity, string message)
        {
            var webHookUrl = _configuration.GetSection("AlphaWebHookUrl").Value;
            var statusCode = await _webClient.PostJsonAsync(new Uri(webHookUrl), new WebHookRequestData
            {
                Type = "message",
                Timestamp = DateTime.Now.ToUnixTime(),
                Source = new Source
                {
                    Type = activity.Conversation.Id == activity.From.Id ? "user" : "group",
                    UserId = activity.Conversation.Id
                },
                Payload = message
            });

            await Send(activity, $"Hooked completed, status {statusCode}");
        }

        private async Task Send(Activity activity, string message)
        {
            var connector = CreateConnectorClient(new Uri(activity.ServiceUrl));
            var reply = activity.CreateReply(message);
            await connector.Conversations.ReplyToActivityAsync(reply);
        }

        private ConnectorClient CreateConnectorClient(Uri serviceUrl)
          => new ConnectorClient(
               serviceUrl,
               _configuration.GetSection("MicrosoftAppId").Value,
               _configuration.GetSection("MicrosoftAppPassword").Value);
    }
}