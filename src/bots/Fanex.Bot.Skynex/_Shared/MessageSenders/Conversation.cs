using System;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Bot.Core._Shared.Constants;
using Fanex.Bot.Core._Shared.Database;
using Fanex.Bot.Core._Shared.Enumerations;
using Fanex.Bot.Core.Bot.Models;
using Microsoft.Bot.Connector;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Fanex.Bot.Skynex._Shared.MessageSenders
{
    public interface IConversation
    {
        Task ReplyAsync(IMessageActivity activity, string message);

        Task SendAsync(MessageInfo messageInfo);

        Task<Result> SendAsync(string conversationId, string message, MessageType messageType = MessageType.Markdown);

        Task SendAdminAsync(string message);
    }

#pragma warning disable S1450 // Private fields only used as local variables in methods should become local variables

    public class Conversation : IConversation
    {
        private readonly BotDbContext dbContext;
        private readonly ILogger<Conversation> logger;
        private readonly Func<string, IMessengerConversation> messengerFactory;

        public Conversation(
            BotDbContext dbContext,
            ILogger<Conversation> logger,
            Func<string, IMessengerConversation> messengerFactory)
        {
            this.dbContext = dbContext;
            this.logger = logger;
            this.messengerFactory = messengerFactory;
        }

#pragma warning disable S1301 // "switch" statements should have at least 3 "case" clauses

        public async Task ReplyAsync(IMessageActivity activity, string message)
        {
            var messenger = messengerFactory(activity.ChannelId);

            await messenger.ReplyAsync(activity, message);
        }

        public async Task SendAdminAsync(string message)
        {
            var adminMessageInfos = dbContext.MessageInfo.Where(messageInfo => messageInfo.IsAdmin).AsNoTracking();

            foreach (var adminMessageInfo in adminMessageInfos)
            {
                adminMessageInfo.Text = message;
                adminMessageInfo.Type = MessageType.Markdown;

                await ForwardMessage(adminMessageInfo);
            }
        }

        public async Task<Result> SendAsync(string conversationId, string message, MessageType messageType = MessageType.Markdown)
        {
            var messageInfo = await dbContext
                .MessageInfo
                .FirstOrDefaultAsync(info => info.ConversationId == conversationId);

            if (messageInfo != null)
            {
                messageInfo.Text = message;
                messageInfo.Type = messageType;
                await SendAsync(messageInfo);

                return Result.CreateSuccessfulResult();
            }
            else
            {
                var errorMessage = $"Error: {MessageFormatSymbol.BOLD_START}{conversationId}{MessageFormatSymbol.BOLD_END} not found";
                await SendAdminAsync(errorMessage);

                return Result.CreateFailedResult(errorMessage);
            }
        }

        public async Task SendAsync(MessageInfo messageInfo)
        {
            try
            {
                if (!string.IsNullOrEmpty(messageInfo.Text))
                {
                    await ForwardMessage(messageInfo);
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"{ex.Message}\n{ex.StackTrace}");

                await SendAdminAsync(
                    $"Can not send message to {MessageFormatSymbol.BOLD_START}{messageInfo?.ConversationId}{MessageFormatSymbol.BOLD_END} {MessageFormatSymbol.NEWLINE}" +
                    $"{MessageFormatSymbol.BOLD_START}Exception:{MessageFormatSymbol.BOLD_END} {ex.Message} {MessageFormatSymbol.NEWLINE}" +
                    $"Message: {messageInfo?.Text}" +
                    $"{MessageFormatSymbol.DIVIDER}").ConfigureAwait(false);
            }
        }

        private async Task ForwardMessage(MessageInfo messageInfo)
        {
            var messenger = messengerFactory(messageInfo.ChannelId);

            await messenger.SendAsync(messageInfo);
        }

#pragma warning restore S1301 // "switch" statements should have at least 3 "case" clauses
    }

#pragma warning restore S1450 // Private fields only used as local variables in methods should become local variables
}