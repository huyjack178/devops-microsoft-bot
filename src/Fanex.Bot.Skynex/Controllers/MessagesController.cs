namespace Fanex.Bot.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;
    using Fanex.Bot.Core.Utilities.Bot;
    using Fanex.Bot.Skynex.Dialogs;
    using Fanex.Bot.Skynex.Utilities.Bot;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Bot.Connector;
    using Microsoft.Extensions.Configuration;

    [Route("api/[controller]")]
    public class MessagesController : Controller
    {
        private readonly IDialog _dialog;
        private readonly ILogDialog _logDialog;
        private readonly IGitLabDialog _gitLabDialog;
        private readonly ILineDialog _lineDialog;
        private readonly IConversation _conversation;
        private readonly IConfiguration _configuration;

        public MessagesController(
            IDialog dialog,
            ILogDialog logDialog,
            IGitLabDialog gitLabDialog,
            ILineDialog lineDialog,
            IConversation conversation,
            IConfiguration configuration)
        {
            _dialog = dialog;
            _logDialog = logDialog;
            _gitLabDialog = gitLabDialog;
            _lineDialog = lineDialog;
            _conversation = conversation;
            _configuration = configuration;
        }

        [Authorize(Roles = "Bot")]
        [HttpPost]
        public async Task<OkResult> Post([FromBody] Activity activity)
        {
            switch (activity.Type)
            {
                case ActivityTypes.Message:
                    await HandleMessage(await HandleForChannels(activity));
                    break;

                case ActivityTypes.ConversationUpdate:
                    await HandleConversationUpdate(activity);
                    break;

                case ActivityTypes.ContactRelationUpdate:
                    await HandleContactRelationUpdate(activity);
                    break;

                default:
                    return Ok();
            }

            return Ok();
        }

        [Authorize(Roles = "Bot")]
        [HttpPost]
        [Route("Forward")]
        public async Task<OkObjectResult> Forward(string message, string conversationId)
        {
            var result = await _conversation.SendAsync(conversationId, message);

            return Ok(result);
        }

        private async Task HandleMessage(IMessageActivity activity)
        {
            var message = activity.Text.ToLowerInvariant().Trim();
            message = BotHelper.GenerateMessage(message);

            if (message.StartsWith("log"))
            {
                await _logDialog.HandleMessageAsync(activity, message);
            }
            else if (message.StartsWith("gitlab"))
            {
                await _gitLabDialog.HandleMessageAsync(activity, message);
            }
            else
            {
                await _dialog.HandleMessageAsync(activity, message);
            }
        }

        private async Task HandleConversationUpdate(Activity activity)
        {
            var conversationUpdate = activity.AsConversationUpdateActivity();
            var botId = _configuration.GetSection("BotId")?.Value;

            if (conversationUpdate.MembersRemoved != null &&
                conversationUpdate.MembersRemoved.Any(mem => mem.Id == botId))
            {
                await _dialog.RemoveConversationData(activity);
                return;
            }

            if (conversationUpdate.MembersAdded != null &&
                conversationUpdate.MembersAdded.Any(mem => mem.Id == botId))
            {
                await _dialog.RegisterMessageInfo(activity);
            }

            await _conversation.ReplyAsync(activity, "Hello. I am SkyNex.");
        }

        private async Task HandleContactRelationUpdate(Activity activity)
        {
            if (activity.Action?.ToLowerInvariant() == "remove")
            {
                await _dialog.RemoveConversationData(activity);
            }
            else
            {
                await _dialog.RegisterMessageInfo(activity);
                await _conversation.ReplyAsync(activity, "Hello. I am SkyNex.");
            }
        }

        private async Task<Activity> HandleForChannels(Activity activity)
        {
            if (activity.From?.Name?.ToLowerInvariant() == "line")
            {
                activity.Conversation.Id = activity.From.Id;
                activity.ChannelId = "line";
                await _lineDialog.RegisterMessageInfo(activity);
            }

            return activity;
        }
    }
}