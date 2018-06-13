namespace Fanex.Bot.Controllers
{
    using System.Threading.Tasks;
    using Fanex.Bot.Dialogs;
    using Fanex.Bot.Utilitites.Bot;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Bot.Connector;

    [Route("api/[controller]")]
    public class MessagesController : Controller
    {
        private readonly IDialog _dialog;
        private readonly IRootDialog _rootDialog;
        private readonly ILogDialog _logDialog;
        private readonly IGitLabDialog _gitLabDialog;
        private readonly IConversation _conversation;

        public MessagesController(
            IDialog dialog,
            IRootDialog rootDialog,
            ILogDialog logDialog,
            IGitLabDialog gitLabDialog,
             IConversation conversation)
        {
            _dialog = dialog;
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
                    await _conversation.SendAsync(activity, $"Hello. I am SkyNex.");
                    break;

                case ActivityTypes.EndOfConversation:
                    // TODO: Remove message info
                    break;

                default:
                    return Ok();
            }

            return Ok();
        }

        private async Task HandleMessageCommands(IMessageActivity activity)
        {
            var message = activity.Text.ToLowerInvariant().Trim();
            message = GenerateMessage(message);

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

        private static string GenerateMessage(string message)
        {
            var returnMessage = message;

            if (message.StartsWith("@"))
            {
                var indexOfCommand = message.IndexOf(' ');

                if (indexOfCommand > 0)
                {
                    returnMessage = message.Remove(0, indexOfCommand).Trim();
                }
            }

            return returnMessage;
        }
    }
}