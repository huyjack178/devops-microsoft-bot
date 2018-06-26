﻿namespace Fanex.Bot.Dialogs.Impl
{
    using System.Threading.Tasks;
    using Fanex.Bot.Models;
    using Fanex.Bot.Utilitites.Bot;
    using Microsoft.Bot.Connector;

    public class RootDialog : Dialog, IRootDialog
    {
        public RootDialog(
          BotDbContext dbContext,
           IConversation conversation)
          : base(dbContext, conversation)
        {
        }

        public async Task HandleMessageAsync(IMessageActivity activity, string messageCmd)
        {
            if (messageCmd.StartsWith("group"))
            {
                await Conversation.SendAsync(activity, $"Your group id is: {activity.Conversation.Id}");
            }
            else if (messageCmd.StartsWith("help"))
            {
                await Conversation.SendAsync(activity, GetCommandMessages());
            }
            else
            {
                await Conversation.SendAsync(activity, "Please send **help** to get my commands");
            }
        }
    }
}