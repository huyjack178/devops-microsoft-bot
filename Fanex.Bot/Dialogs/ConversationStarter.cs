namespace Fanex.Bot
{
    using System;
    using System.Threading.Tasks;
    using Fanex.Bot.Models;
    using Microsoft.Bot.Connector;
    using Microsoft.Bot.Schema;

    public class ConversationStarter
    {
        public static async Task Resume(MessageInfo messageInfo)
        {
            var userAccount = new ChannelAccount(messageInfo.ToId, messageInfo.ToName);
            var botAccount = new ChannelAccount(messageInfo.FromId, messageInfo.FromName);
            var connector = new ConnectorClient(new Uri(messageInfo.ServiceUrl), messageInfo.AppCredentials);

            var message = Activity.CreateMessageActivity();

            message.ChannelId = messageInfo.ChannelId;
            message.From = botAccount;
            message.Recipient = userAccount;
            message.Conversation = new ConversationAccount(id: messageInfo.ConversationId);

            if (!string.IsNullOrEmpty(message.Text))
            {
                message.Text = message.Text;
                await connector.Conversations.SendToConversationAsync((Activity)message);
            }
        }
    }
}