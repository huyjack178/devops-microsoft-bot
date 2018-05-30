namespace Fanex.Bot
{
    using System.Threading.Tasks;
    using Fanex.Bot.Dialogs;
    using Microsoft.Bot;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;

    public class Bot : IBot
    {
        private readonly ILogDialog logDialog;

        public Bot(ILogDialog logDialog)
        {
            this.logDialog = logDialog;
        }

        public async Task OnTurn(ITurnContext context)
        {
            switch (context.Activity.Type)
            {
                case ActivityTypes.Message:
                    var messageActivity = context.Activity.AsMessageActivity();
                    var message = messageActivity.Text.ToLowerInvariant();

                    if (message.StartsWith("@"))
                    {
                        var indexOfCommand = message.IndexOf(' ');

                        if (indexOfCommand > 0)
                        {
                            message = message.Remove(0, indexOfCommand).Trim();
                        }
                    }

                    if (message.StartsWith("log"))
                    {
                        if (message.StartsWith("log subscribe"))
                        {
                            await ProcessSubcribingLogAsync(context, message);
                        }
                        else
                        {
                            await context.SendActivity(GetCommandMessage());
                        }
                    }
                    else if (message.StartsWith("group"))
                    {
                        await context.SendActivity($"Your group id is: {context.Activity.Conversation.Id}");
                    }
                    else
                    {
                        await context.SendActivity(GetCommandMessage());
                    }

                    if (!context.Responded)
                    {
                        await context.SendActivity("Sorry, I don't understand. Ask Harrison for it");
                    }

                    break;

                case ActivityTypes.ConversationUpdate:
                    foreach (var newMember in context.Activity.MembersAdded)
                    {
                        if (newMember.Id != context.Activity.Recipient.Id)
                        {
                            await context.SendActivity($"Hello {newMember.Name}. I am Megatron.");
                        }
                    }

                    break;

                default:
                    await context.SendActivity($"Hello all. I am Megatron. My current mission is getting error log from mSite");
                    break;
            }
        }

        private async Task ProcessSubcribingLogAsync(ITurnContext context, string message)
        {
            var logCategory = message.Substring(13).Trim();

            if (string.IsNullOrEmpty(logCategory))
            {
                await context.SendActivity("You need to add [LogCategory], otherwise, you will not get any log info");
            }
            else
            {
                await logDialog.RegisterLogCategoryAsync(context, logCategory);
                await logDialog.NotifyLogAsync(context);
            }
        }

        private static string GetCommandMessage()
        {
            return $"Megatron's available commands: \n\n" +
                                       $"**log subscribe [LogCategory]** ==> Register to get log notification with specific [LogCategory]. Example: log add Alpha;NAP \n\n" +
                                       $"**group** ==> Get your group ID";
        }
    }
}