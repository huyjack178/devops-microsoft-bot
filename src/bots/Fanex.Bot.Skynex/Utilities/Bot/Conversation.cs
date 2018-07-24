namespace Fanex.Bot.Skynex.Utilities.Bot
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Fanex.Bot.Skynex.Models;
    using Microsoft.Bot.Connector;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    public interface IConversation
    {
        Task ReplyAsync(IMessageActivity activity, string message);

        Task SendAsync(MessageInfo messageInfo);

        Task<string> SendAsync(string conversationId, string message);

        Task SendAdminAsync(string message);
    }

#pragma warning disable S1450 // Private fields only used as local variables in methods should become local variables

    public class Conversation : IConversation
    {
        private readonly BotDbContext _dbContext;
        private readonly ILogger<Conversation> _logger;
        private readonly ISkypeConversation _skypeConversation;
        private readonly ILineConversation _lineConversation;

        public Conversation(
            IConfiguration configuration,
            BotDbContext dbContext,
            ILogger<Conversation> logger,
            ISkypeConversation skypeConversation,
            ILineConversation lineConversation)
        {
            _dbContext = dbContext;
            _logger = logger;
            _skypeConversation = skypeConversation;
            _lineConversation = lineConversation;
        }

#pragma warning disable S1301 // "switch" statements should have at least 3 "case" clauses

        public async Task ReplyAsync(IMessageActivity activity, string message)
        {
            switch (activity.ChannelId)
            {
                case Channel.Line:
                    await _lineConversation.ReplyAsync(activity, message);
                    break;

                default:
                    await _skypeConversation.ReplyAsync(activity, message);
                    break;
            }
        }

        public async Task SendAdminAsync(string message)
        {
            var adminMessageInfos = _dbContext.MessageInfo.Where(messageInfo => messageInfo.IsAdmin);

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
                _logger.LogError($"{ex.Message}\n{ex.StackTrace}");

                await SendAdminAsync(
                    $"Can not send message to **{messageInfo?.ConversationId}** {Constants.NewLine}" +
                    $"**Exception:** {ex.Message} {Constants.NewLine}" +
                    $"==========================");
            }
        }

        public async Task<string> SendAsync(string conversationId, string message)
        {
            var messageInfo = await _dbContext
                .MessageInfo
                .FirstOrDefaultAsync(info => info.ConversationId == conversationId);

            if (messageInfo != null)
            {
                messageInfo.Text = message;
                await SendAsync(messageInfo);

                return "Success";
            }
            else
            {
                var errorMessage = $"Error: **{conversationId}** not found";
                await SendAdminAsync(errorMessage);

                return errorMessage;
            }
        }

        private async Task ForwardMessage(MessageInfo message)
        {
            switch (message.ChannelId)
            {
                case Channel.Line:
                    await _lineConversation.SendAsync(message);
                    break;

                default:
                    await _skypeConversation.SendAsync(message);
                    break;
            }
        }

#pragma warning restore S1301 // "switch" statements should have at least 3 "case" clauses
    }

#pragma warning restore S1450 // Private fields only used as local variables in methods should become local variables
}