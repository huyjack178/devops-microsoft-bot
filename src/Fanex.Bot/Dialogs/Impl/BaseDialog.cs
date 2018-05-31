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

    public class BaseDialog
    {
        private readonly IConfiguration _configuration;
        private readonly BotDbContext _dbContext;

        public BaseDialog(
            IConfiguration configuration,
            BotDbContext dbContext)
        {
            _configuration = configuration;
            _dbContext = dbContext;
        }

        public async Task SendAsync(MessageInfo messageInfo)
        {
            ConnectorClient connector = CreateConnectorClient(messageInfo);

            var message = CreateMessageActivity(messageInfo);

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
                await SendAdminAsync(
                    $"Error happen in client {messageInfo?.ConversationId}\n\n" +
                    $"Exception: {ex.InnerException.Message}");
            }

            await SendAdminAsync($"Log has been sent to client {messageInfo?.ConversationId}");
        }

        protected async Task SendAdminAsync(string message)
        {
            var adminMessageInfos = _dbContext.MessageInfo.Where(messageInfo => messageInfo.IsAdmin);

            foreach (var adminMessageInfo in adminMessageInfos)
            {
                var connector = CreateConnectorClient(adminMessageInfo);
                var adminMessage = CreateMessageActivity(adminMessageInfo);
                adminMessage.Text = message;

                await connector.Conversations.SendToConversationAsync((Activity)adminMessage);
            }
        }

        protected virtual MessageInfo GetMessageInfo(ITurnContext context)
        {
            var message = context.Activity;
            var messageInfo = _dbContext.MessageInfo.FirstOrDefault(e => e.ConversationId == message.Conversation.Id);

            if (messageInfo == null)
            {
                messageInfo = GenerateMessageInfo(context);
            }

            return messageInfo;
        }

        protected virtual async Task SaveMessageInfoAsync(MessageInfo messageInfo)
        {
            _dbContext.Entry(messageInfo).State =
                _dbContext.MessageInfo.Any(e => e.ConversationId == messageInfo.ConversationId) ?
                    EntityState.Modified : EntityState.Added;

            await _dbContext.SaveChangesAsync();
        }

        protected virtual MessageInfo GenerateMessageInfo(ITurnContext context)
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
                IsActive = true
            };

            return messageInfo;
        }

        private ConnectorClient CreateConnectorClient(MessageInfo messageInfo)
        {
            return new ConnectorClient(
                            new Uri(messageInfo.ServiceUrl),
                            _configuration.GetSection("MicrosoftAppId").Value,
                            _configuration.GetSection("MicrosoftAppPassword").Value);
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