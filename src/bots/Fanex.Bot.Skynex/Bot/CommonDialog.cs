using System;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Bot.Core._Shared.Constants;
using Fanex.Bot.Core._Shared.Database;
using Fanex.Bot.Core.Bot.Models;
using Fanex.Bot.Skynex._Shared.Base;
using Fanex.Bot.Skynex._Shared.MessageSenders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Connector = Microsoft.Bot.Connector;

namespace Fanex.Bot.Skynex.Bot
{
    public interface ICommonDialog : IDialog
    {
        Task HandleConversationUpdate(Connector.IMessageActivity activity);

        Task HandleContactRelationUpdate(Connector.IMessageActivity activity);

        Task RegisterMessageInfo(Connector.IMessageActivity activity);

        Task RemoveConversationData(Connector.IMessageActivity activity);
    }

    public class CommonDialog : BaseDialog, ICommonDialog
    {
        private readonly IConfiguration configuration;

        public CommonDialog(
            BotDbContext dbContext,
            IConversation conversation,
            IConfiguration configuration) : base(dbContext, conversation)
        {
            this.configuration = configuration;
        }

        public async Task HandleMessage(Connector.IMessageActivity activity, string message)
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

            await Conversation.ReplyAsync(
                activity,
                $"Please send {MessageFormatSymbol.BOLD_START}help{MessageFormatSymbol.BOLD_END} to get my commands");
        }

        public async Task HandleConversationUpdate(Connector.IMessageActivity activity)
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

        public async Task HandleContactRelationUpdate(Connector.IMessageActivity activity)
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

        public virtual async Task RegisterMessageInfo(Connector.IMessageActivity activity)
        {
            var messageInfo = await DbContext.MessageInfo.FirstOrDefaultAsync(
                e => e.ConversationId == activity.Conversation.Id);

            if (messageInfo == null)
            {
                messageInfo = InitMessageInfo(activity);
                await SaveMessageInfo(messageInfo);
                await Conversation.SendAdminAsync(
                    $"New client {MessageFormatSymbol.BOLD_START}{activity.Conversation.Id}{MessageFormatSymbol.BOLD_END} has been added");
            }
        }

        public virtual async Task RemoveConversationData(Connector.IMessageActivity activity)
        {
            var messageInfo = await DbContext.MessageInfo.SingleOrDefaultAsync(
                info => info.ConversationId == activity.Conversation.Id);

            if (messageInfo != null)
            {
                DbContext.MessageInfo.Remove(messageInfo);
            }

            var logInfo = await DbContext.LogInfo.SingleOrDefaultAsync(
                info => info.ConversationId == activity.Conversation.Id);

            if (logInfo != null)
            {
                DbContext.LogInfo.Remove(logInfo);
            }

            var gitlabInfo = await DbContext.GitLabInfo.SingleOrDefaultAsync(
               info => info.ConversationId == activity.Conversation.Id);

            if (gitlabInfo != null)
            {
                DbContext.GitLabInfo.Remove(gitlabInfo);
            }

            await DbContext.SaveChangesAsync();
            await Conversation.SendAdminAsync(
                $"Client {MessageFormatSymbol.BOLD_START}{activity.Conversation.Id}{MessageFormatSymbol.BOLD_END} has been removed");
        }

        private static MessageInfo InitMessageInfo(Connector.IMessageActivity activity)
        {
            return new MessageInfo
            {
                ToId = activity.From?.Id,
                ToName = activity.From?.Name,
                FromId = activity.Recipient?.Id,
                FromName = activity.Recipient?.Name,
                ServiceUrl = activity.ServiceUrl,
                ChannelId = activity.ChannelId,
                ConversationId = activity.Conversation?.Id,
#pragma warning disable S109 // Magic numbers should not be used
                CreatedTime = DateTime.UtcNow.AddHours(7)
#pragma warning restore S109 // Magic numbers should not be used
            };
        }
    }
}