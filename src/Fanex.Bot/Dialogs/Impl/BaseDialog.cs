namespace Fanex.Bot.Dialogs.Impl
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Fanex.Bot.Models;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Connector;
    using Microsoft.Bot.Schema;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;

    public class BaseDialog : IDialog
    {
        private readonly IConfiguration _configuration;
        private readonly MessageInfo _adminMessageInfo;
        private readonly BotDbContext _dbContext;

        public BaseDialog(
            IConfiguration configuration,
            BotDbContext dbContext,
            IOptions<MessageInfo> adminMessageInfo)
        {
            _configuration = configuration;
            _dbContext = dbContext;
            _adminMessageInfo = adminMessageInfo.Value;
        }

        public async Task SendAsync(MessageInfo messageInfo)
        {
            var connector = new ConnectorClient(
                new Uri(messageInfo.ServiceUrl),
                _configuration.GetSection("MicrosoftAppId").Value,
                _configuration.GetSection("MicrosoftAppPassword").Value);

            var message = CreateMessageActivity(messageInfo);

            var adminMessage = CreateMessageActivity(_adminMessageInfo);
            adminMessage.Text = $"Log has been sent to client {messageInfo.ConversationId}";

            try
            {
                if (!string.IsNullOrEmpty(messageInfo.Text))
                {
                    message.Text = messageInfo.Text;
                    await connector.Conversations.SendToConversationAsync((Activity)message);
                }
            }
            catch (Exception ex)
            {
                adminMessage.Text =
                  $"Error happen in client {messageInfo.ConversationId}\n\n" +
                  $"Exception: {ex.InnerException.Message}";
            }

            await connector.Conversations.SendToConversationAsync((Activity)adminMessage);
        }

        public async Task RegisterConversation(ITurnContext context)
        {
            var messageInfo = GenerateMessageInfo(context);

            await SaveMessageInfo(messageInfo);
        }

        protected async Task SaveMessageInfo(MessageInfo messageInfo)
        {
            _dbContext.Entry(messageInfo).State =
                _dbContext.MessageInfo.Any(e => e.ConversationId == messageInfo.ConversationId) ?
                    EntityState.Modified : EntityState.Added;

            await _dbContext.SaveChangesAsync();
        }

        protected static MessageInfo GenerateMessageInfo(ITurnContext context)
        {
            var message = context.Activity;
            var messageInfo = new MessageInfo
            {
                ToId = message.From.Id,
                ToName = message.From.Name,
                FromId = message.Recipient.Id,
                FromName = message.Recipient.Name,
                ServiceUrl = message.ServiceUrl,
                ChannelId = message.ChannelId,
                ConversationId = message.Conversation.Id,
            };

            return messageInfo;
        }

        private static IMessageActivity CreateMessageActivity(MessageInfo messageInfo)
        {
            var userAccount = new ChannelAccount(messageInfo.ToId, messageInfo.ToName);
            var botAccount = new ChannelAccount(messageInfo.FromId, messageInfo.FromName);
            var message = Activity.CreateMessageActivity();
            message.ChannelId = messageInfo.ChannelId;
            message.From = botAccount;
            message.Recipient = userAccount;
            message.Conversation = new ConversationAccount(id: messageInfo.ConversationId);

            return message;
        }
    }
}