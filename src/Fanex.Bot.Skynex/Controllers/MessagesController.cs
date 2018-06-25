namespace Fanex.Bot.Controllers
{
    using System.Threading.Tasks;
    using Fanex.Bot.Core.Utilities.Bot;
    using Fanex.Bot.Dialogs;
    using Fanex.Bot.Utilitites.Bot;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Bot.Connector;

    [Route("api/[controller]")]
    public class MessagesController : Controller
    {
        private readonly IRootDialog _rootDialog;
        private readonly ILogDialog _logDialog;
        private readonly IGitLabDialog _gitLabDialog;
        private readonly IConversation _conversation;

        public MessagesController(
            IRootDialog rootDialog,
            ILogDialog logDialog,
            IGitLabDialog gitLabDialog,
             IConversation conversation)
        {
            _rootDialog = rootDialog;
            _logDialog = logDialog;
            _gitLabDialog = gitLabDialog;
            _conversation = conversation;
        }

        [Authorize(Roles = "Bot")]
        [HttpPost]
        public async Task<OkResult> Post([FromBody] Activity activity)
        {
            switch (activity.Type)
            {
                case ActivityTypes.Message:
                    await HandleMessageCommands(activity);
                    break;

                case ActivityTypes.ConversationUpdate:
                case ActivityTypes.InstallationUpdate:
                case ActivityTypes.ContactRelationUpdate:
                    await _conversation.SendAsync(activity, "Hello. I am SkyNex.");
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

        private async Task HandleMessageCommands(IMessageActivity activity)
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
                await _rootDialog.HandleMessageAsync(activity, message);
            }
        }
    }
}