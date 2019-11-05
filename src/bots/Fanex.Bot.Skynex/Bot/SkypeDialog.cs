using System.Linq;
using System.Threading.Tasks;
using Fanex.Bot.Core._Shared.Constants;
using Fanex.Bot.Core._Shared.Database;
using Fanex.Bot.Skynex._Shared.Base;
using Fanex.Bot.Skynex._Shared.MessageSenders;
using Microsoft.Extensions.Configuration;
using Connector = Microsoft.Bot.Connector;

namespace Fanex.Bot.Skynex.Bot
{
    public interface IMessengerDialog : IDialog
    {
        Task HandleConversationUpdate(Connector.IMessageActivity activity);

        Task HandleContactRelationUpdate(Connector.IMessageActivity activity);
    }

    public class SkypeDialog : BaseDialog, IMessengerDialog
    {
        private readonly IConfiguration configuration;

        public SkypeDialog(
            BotDbContext dbContext,
            IConversation conversation,
            IConfiguration configuration) : base(dbContext, conversation)
        {
            this.configuration = configuration;
        }

        public virtual async Task HandleMessage(Connector.IMessageActivity activity, string message)
        {
            if (message.StartsWith("group"))
            {
                await Conversation.ReplyAsync(activity, $"Your group id is: {activity.Conversation.Id}");
                return;
            }

            if (message.StartsWith("help"))
            {
                await Conversation.ReplyAsync(activity, GetCommandMessages());
                return;
            }

            if (message.StartsWith(MessageCommand.START))
            {
                await HandleConversationUpdate(activity);
                return;
            }

            await Conversation.ReplyAsync(
                activity,
                $"Please send {MessageFormatSymbol.BOLD_START}help{MessageFormatSymbol.BOLD_END} to get my commands");
        }

        public virtual async Task HandleConversationUpdate(Connector.IMessageActivity activity)
        {
            var conversationUpdate = activity.AsConversationUpdateActivity();
            var botId = configuration.GetSection("BotId")?.Value;

            if (conversationUpdate.MembersRemoved != null &&
                conversationUpdate.MembersRemoved.Any(mem => mem.Id == botId))
            {
                await RemoveConversationData(activity);
                return;
            }

            if (conversationUpdate.MembersAdded != null &&
                conversationUpdate.MembersAdded.Any(mem => mem.Id == botId))
            {
                await RegisterMessageInfo(activity);
            }

            await Conversation.ReplyAsync(activity, "Hello. I am SkyNex.");
        }

        public virtual async Task HandleContactRelationUpdate(Connector.IMessageActivity activity)
        {
            if ((activity as Connector.Activity)?.Action?.ToLowerInvariant() == "remove")
            {
                await RemoveConversationData(activity);
            }
            else
            {
                await RegisterMessageInfo(activity);
                await Conversation.ReplyAsync(activity, "Hello. I am SkyNex.");
            }
        }
    }
}