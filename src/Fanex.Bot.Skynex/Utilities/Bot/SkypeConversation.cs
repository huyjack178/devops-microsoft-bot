namespace Fanex.Bot.Skynex.Utilities.Bot
{
    using System;
    using System.Threading.Tasks;
    using Fanex.Bot.Skynex.Models;
    using Microsoft.Bot.Connector;
    using Microsoft.Extensions.Configuration;

    public interface ISkypeConversation
    {
        Task ReplyAsync(IMessageActivity activity, string message);

        Task SendAsync(MessageInfo messageInfo);
    }

    public class SkypeConversation : ISkypeConversation
    {
        private readonly IConfiguration _configuration;

        public SkypeConversation(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task ReplyAsync(IMessageActivity activity, string message)
        {
            var connector = CreateConnectorClient(new Uri(activity.ServiceUrl));
            var reply = (activity as Activity).CreateReply(message);

            await connector.Conversations.ReplyToActivityAsync(reply);
        }

        public async Task SendAsync(MessageInfo messageInfo)
        {
            var connector = CreateConnectorClient(new Uri(messageInfo.ServiceUrl));
            var message = CreateMessageActivity(messageInfo);

            if (!string.IsNullOrEmpty(messageInfo.Text))
            {
                message.Text = messageInfo.Text;
                await connector.Conversations.SendToConversationAsync((Activity)message);
            }
        }

        private ConnectorClient CreateConnectorClient(Uri serviceUrl)
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