namespace Fanex.Bot.Utilitites.Bot
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Fanex.Bot.Models;
    using Microsoft.Bot.Connector;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;

    public class Conversation : IConversation
    {
        private readonly IConfiguration _configuration;
        private readonly BotDbContext _dbContext;

        public Conversation(
            IConfiguration configuration,
            BotDbContext dbContext)
        {
            _configuration = configuration;
            _dbContext = dbContext;
        }

        public async Task SendAsync(IMessageActivity activity, string message)
        {
            var connector = CreateConnectorClient(new Uri(activity.ServiceUrl));
            var reply = (activity as Activity).CreateReply(message);

            await connector.Conversations.ReplyToActivityAsync(reply);
        }

        public async Task SendAdminAsync(string message)
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

        public async Task SendAsync(MessageInfo messageInfo)
        {
            var connector = CreateConnectorClient(new Uri(messageInfo.ServiceUrl));
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

        public ConnectorClient CreateConnectorClient(Uri serviceUrl)
           => new ConnectorClient(
                serviceUrl,
                _configuration.GetSection("MicrosoftAppId").Value,
                _configuration.GetSection("MicrosoftAppPassword").Value);

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