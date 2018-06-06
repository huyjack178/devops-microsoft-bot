namespace Fanex.Bot.Dialogs.Impl
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Fanex.Bot.Models;
    using Microsoft.Bot.Connector;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;

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

        public async Task SendActivity(Activity activity, string message)
        {
            var connector = CreateConnectorClient(new Uri(activity.ServiceUrl));
            var reply = activity.CreateReply(message);

            await connector.Conversations.ReplyToActivityAsync(reply);
        }

        public async Task SendAsync(MessageInfo messageInfo)
        {
            ConnectorClient connector = CreateConnectorClient(new Uri(messageInfo.ServiceUrl));

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
                var connector = CreateConnectorClient(new Uri(adminMessageInfo.ServiceUrl));
                var adminMessage = CreateMessageActivity(adminMessageInfo);
                adminMessage.Text = message;

                await connector.Conversations.SendToConversationAsync((Activity)adminMessage);
            }
        }

        protected virtual MessageInfo GetMessageInfo(Activity activity)
        {
            var messageInfo = _dbContext.MessageInfo.FirstOrDefault(e => e.ConversationId == activity.Conversation.Id);

            if (messageInfo == null)
            {
                messageInfo = GenerateMessageInfo(activity);
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

        protected virtual MessageInfo GenerateMessageInfo(Activity activity)
        {
            var messageInfo = new MessageInfo
            {
                ToId = activity.From.Id,
                ToName = activity.From.Name,
                FromId = activity.Recipient.Id,
                FromName = activity.Recipient.Name,
                ServiceUrl = activity.ServiceUrl,
                ChannelId = activity.ChannelId,
                ConversationId = activity.Conversation.Id,
                IsActive = true
            };

            return messageInfo;
        }

        private ConnectorClient CreateConnectorClient(Uri serviceUrl)
        {
            return new ConnectorClient(
                            serviceUrl,
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