namespace Fanex.Bot.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Fanex.Bot.Dialogs;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Bot.Connector;
    using Microsoft.Extensions.Configuration;

    [Route("api/[controller]")]
    public class MessagesController : Controller
    {
        private readonly ILogDialog _logDialog;
        private readonly IConfiguration _configuration;

        public MessagesController(IConfiguration configuration, ILogDialog logDialog)
        {
            _logDialog = logDialog;
            _configuration = configuration;
        }

        public static string GetCommandMessages()
        {
            return $"SkyNex's available commands: \n\n" +
                    $"**log add [Contains-LogCategory]** " +
                        $"==> Register to get log which has category name **contains [Contains-LogCategory]**. Example: log add Alpha;NAP \n\n" +
                    $"**log remove [LogCategory]**\n\n" +
                    $"**log start** ==> Start receiving logs \n\n" +
                    $"**log stop** ==> Stop receiving logs \n\n" +
                    $"**log detail [LogId] (BETA)** ==> Get log detail \n\n" +
                    $"**log viewStatus** ==> Get your current subscribing Log Categories and Receiving Logs status \n\n" +
                    $"**group** ==> Get your group ID";
        }

        [Authorize(Roles = "Bot")]
        [HttpPost]
        public async Task<OkResult> Post([FromBody] Activity activity)
        {
            var appCredentials = new MicrosoftAppCredentials(_configuration);
            var connector = new ConnectorClient(new Uri(activity.ServiceUrl), appCredentials);

            switch (activity.Type)
            {
                case ActivityTypes.Message:

                    await HandleMessageCommands(activity, connector);
                    break;

                case ActivityTypes.ConversationUpdate:
                    await HandleConverationUpdate(activity, connector);
                    break;

                default:
                    await connector.Conversations.ReplyToActivityAsync(
                        activity.CreateReply($"Hello all. I am SkyNex."));
                    break;
            }

            return Ok();
        }

        private async Task HandleMessageCommands(Activity activity, ConnectorClient connector)
        {
            var message = activity.Text.ToLowerInvariant();
            message = GenerateMessage(message);

            if (message.StartsWith("log"))
            {
                await _logDialog.HandleLogMessageAsync(activity, message);
            }
            else if (message.StartsWith("group"))
            {
                await connector.Conversations.ReplyToActivityAsync(
                    activity.CreateReply($"Your group id is: {activity.Conversation.Id}"));
            }
            else if (message.StartsWith("help"))
            {
                await connector.Conversations.ReplyToActivityAsync(
                    activity.CreateReply(GetCommandMessages()));
            }
            else
            {
                await connector.Conversations.ReplyToActivityAsync(
                    activity.CreateReply("Please send **help** to get my commands"));
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

        private static async Task HandleConverationUpdate(Activity activity, ConnectorClient connector)
        {
            foreach (var newMember in activity.MembersAdded)
            {
                if (newMember.Id != activity.Recipient.Id)
                {
                    await connector.Conversations.ReplyToActivityAsync(
                        activity.CreateReply($"Hello {newMember.Name}. I am SkyNex."));
                }
            }
        }
    }
}