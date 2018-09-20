namespace Fanex.Bot.Skynex.MessageHandlers.MessageSenders
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Fanex.Bot.Models;
    using Microsoft.Bot.Connector;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    public interface IConversation
    {
        Task ReplyAsync(IMessageActivity activity, string message);

        Task SendAsync(MessageInfo messageInfo);

        Task<Result> SendAsync(string conversationId, string message);

        Task SendAdminAsync(string message);
    }

#pragma warning disable S1450 // Private fields only used as local variables in methods should become local variables

    public class Conversation : IConversation
    {
        private readonly BotDbContext dbContext;
        private readonly ILogger<Conversation> logger;
        private readonly ISkypeConversation skypeConversation;
        private readonly ILineConversation lineConversation;

        public Conversation(
            BotDbContext dbContext,
            ILogger<Conversation> logger,
            ISkypeConversation skypeConversation,
            ILineConversation lineConversation)
        {
            this.dbContext = dbContext;
            this.logger = logger;
            this.skypeConversation = skypeConversation;
            this.lineConversation = lineConversation;
        }

#pragma warning disable S1301 // "switch" statements should have at least 3 "case" clauses

        public async Task ReplyAsync(IMessageActivity activity, string message)
        {
            switch (activity.ChannelId)
            {
                case Channel.Line:
                    await lineConversation.ReplyAsync(activity, message);
                    break;

                default:
                    await skypeConversation.ReplyAsync(activity, message);
                    break;
            }
        }

        public async Task SendAdminAsync(string message)
        {
            var adminMessageInfos = dbContext.MessageInfo.Where(messageInfo => messageInfo.IsAdmin).AsNoTracking();

            foreach (var adminMessageInfo in adminMessageInfos)
            {
                adminMessageInfo.Text = message;

                await ForwardMessage(adminMessageInfo);
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
                    $"Can not send message to **{messageInfo?.ConversationId}** {Constants.NewLine}" +
                    $"**Exception:** {ex.Message} {Constants.NewLine}" +
                    $"==========================");
            }
        }

        public async Task<Result> SendAsync(string conversationId, string message)
        {
            var messageInfo = await dbContext
                .MessageInfo
                .FirstOrDefaultAsync(info => info.ConversationId == conversationId);

            if (messageInfo != null)
            {
                messageInfo.Text = message;
                await SendAsync(messageInfo);

                return Result.CreateSuccessfulResult();
            }
            else
            {
                var errorMessage = $"Error: **{conversationId}** not found";
                await SendAdminAsync(errorMessage);

                return Result.CreateFailedResult(errorMessage);
            }
        }

        private async Task ForwardMessage(MessageInfo message)
        {
            switch (message.ChannelId)
            {
                case Channel.Line:
                    await lineConversation.SendAsync(message);
                    break;

                default:
                    await skypeConversation.SendAsync(message);
                    break;
            }
        }

#pragma warning restore S1301 // "switch" statements should have at least 3 "case" clauses
    }

#pragma warning restore S1450 // Private fields only used as local variables in methods should become local variables
}