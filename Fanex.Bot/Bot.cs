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
                    if (context.Activity.Type == ActivityTypes.Message)
                    {
                        var messageActivity = context.Activity.AsMessageActivity();
                        var message = messageActivity.Text.ToLowerInvariant();

                        if (message.Contains("log"))
                        {
                            await logDialog.NotifyLogAsync(context);
                            logDialog.NotifyLogPeriodically();
                        }
                        else if (message.Contains("group"))
                        {
                            await context.SendActivity($"Your group id is: {context.Activity.Conversation.Id}");
                        }

                        if (!context.Responded)
                        {
                            await context.SendActivity("Sorry, I don't understand. Ask Harrison for it");
                        }
                    }

                    break;

                case ActivityTypes.ConversationUpdate:
                    foreach (var newMember in context.Activity.MembersAdded)
                    {
                        if (newMember.Id != context.Activity.Recipient.Id)
                        {
                            await context.SendActivity($"Hello {newMember.Name}. I am Megatron. My current mission is getting error log from mSite");
                        }
                    }

                    break;

                case ActivityTypes.InstallationUpdate:
                case ActivityTypes.ContactRelationUpdate:
                    await context.SendActivity($"Hello all. I am Megatron. My current mission is getting error log from mSite");
                    break;
            }
        }
    }
}