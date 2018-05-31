namespace Fanex.Bot
{
    using System.Threading.Tasks;
    using Fanex.Bot.Dialogs;
    using Microsoft.Bot;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;

    public class Bot : IBot
    {
        private readonly ILogDialog _logDialog;

        public Bot(ILogDialog logDialog)
        {
            _logDialog = logDialog;
        }

        public async Task OnTurn(ITurnContext turnContext)
        {
            switch (turnContext.Activity.Type)
            {
                case ActivityTypes.Message:
                    var messageActivity = turnContext.Activity.AsMessageActivity();
                    var message = messageActivity.Text.ToLowerInvariant();

                    message = GenerateMessage(message);

                    await HandleMessageCommands(turnContext, message);

                    await HandleNotResponded(turnContext);

                    break;

                case ActivityTypes.ConversationUpdate:
                    await HandleConverationUpdate(turnContext);
                    break;

                default:
                    await turnContext.SendActivity($"Hello all. I am SkyNex.");
                    break;
            }
        }

        public static string GetCommandMessages()
        {
            return $"SkyNex's available commands: \n\n" +
                    $"**log add [Contains-LogCategory]** " +
                        $"==> Register to get log which has category name **contains [Contains-LogCategory]**. Example: log add Alpha;NAP \n\n" +
                    $"**log remove [LogCategory]**\n\n" +
                    $"**log start** ==> Start receiving logs \n\n" +
                    $"**log stop** ==> Stop receiving logs \n\n" +
                    $"**log viewStatus** ==> Get your current subscribing Log Categories and Receiving Logs status \n\n" +
                    $"**group** ==> Get your group ID";
        }

        private static async Task HandleNotResponded(ITurnContext context)
        {
            if (!context.Responded)
            {
                await context.SendActivity("Sorry, I don't understand. Ask Harrison for it");
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

        private async Task HandleMessageCommands(ITurnContext context, string message)
        {
            if (message.StartsWith("log"))
            {
                await _logDialog.HandleLogMessageAsync(context, message);
            }
            else if (message.StartsWith("group"))
            {
                await context.SendActivity($"Your group id is: {context.Activity.Conversation.Id}");
            }
            else
            {
                await context.SendActivity(GetCommandMessages());
            }
        }

        private static async Task HandleConverationUpdate(ITurnContext context)
        {
            foreach (var newMember in context.Activity.MembersAdded)
            {
                if (newMember.Id != context.Activity.Recipient.Id)
                {
                    await context.SendActivity($"Hello {newMember.Name}. I am SkyNex.");
                }
            }
        }
    }
}