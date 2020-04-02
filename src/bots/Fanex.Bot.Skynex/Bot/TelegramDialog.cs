using System.Threading.Tasks;
using Fanex.Bot.Core._Shared.Constants;
using Fanex.Bot.Core._Shared.Database;
using Fanex.Bot.Skynex._Shared.MessageSenders;
using Microsoft.Bot.Connector;
using Microsoft.Extensions.Configuration;

namespace Fanex.Bot.Skynex.Bot
{
    public interface ITelegramDialog : IMessengerDialog
    {
    }

    public class TelegramDialog : SkypeDialog, ITelegramDialog
    {
        public TelegramDialog(BotDbContext dbContext, IConversation conversation, IConfiguration configuration)
            : base(dbContext, conversation, configuration)
        {
        }

        public override async Task HandleMessage(IMessageActivity activity, string message)
        {
            if (message.StartsWith("register"))
            {
                await RegisterMessageInfo(activity);
                await Conversation.ReplyAsync(activity,
                    $"You are connected with me! {MessageFormatSymbol.NEWLINE}" +
                    $"Your group id is: {activity.Conversation.Id}");

                return;
            }

            await base.HandleMessage(activity, message);
        }

        public override async Task HandleConversationUpdate(IMessageActivity activity)
        {
            await RegisterMessageInfo(activity);

            await Conversation.ReplyAsync(activity, "You are connected with me!");
        }
    }
}