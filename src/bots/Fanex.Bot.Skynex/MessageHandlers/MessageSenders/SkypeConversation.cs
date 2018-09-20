namespace Fanex.Bot.Skynex.MessageHandlers.MessageSenders
{
    using System;
    using System.Threading.Tasks;
    using Fanex.Bot.Models;
    using Fanex.Bot.Skynex.MessageHandlers.MessengerFormatters;
    using Microsoft.Bot.Connector;
    using Microsoft.Extensions.Configuration;

    public interface ISkypeConversation
    {
        Task ReplyAsync(IMessageActivity activity, string message);

        Task SendAsync(MessageInfo messageInfo);
    }

    public class SkypeConversation : ISkypeConversation
    {
        private readonly IConfiguration configuration;
        private readonly IMessengerFormatter messengerFormatter;

        public SkypeConversation(IConfiguration configuration, IMessengerFormatter messengerFormatter)
        {
            this.configuration = configuration;
            this.messengerFormatter = messengerFormatter;
        }

        public async Task ReplyAsync(IMessageActivity activity, string message)
        {
            var connector = await CreateConnectorClient(new Uri(activity.ServiceUrl));
            var reply = (activity as Activity).CreateReply(messengerFormatter.Format(message));

            await connector.Conversations.ReplyToActivityAsync(reply);
        }

        public async Task SendAsync(MessageInfo messageInfo)
        {
            var connector = await CreateConnectorClient(new Uri(messageInfo.ServiceUrl));
            var message = CreateMessageActivity(messageInfo);

            if (!string.IsNullOrEmpty(messageInfo.Text))
            {
                message.Text = messengerFormatter.Format(messageInfo.Text);
                await connector.Conversations.SendToConversationAsync((Activity)message);
            }
        }

        private async Task<ConnectorClient> CreateConnectorClient(Uri serviceUrl)
        {
            var account = new MicrosoftAppCredentials(
                configuration.GetSection("MicrosoftAppId").Value,
                configuration.GetSection("MicrosoftAppPassword").Value);

            var jwtToken = await account.GetTokenAsync(forceRefresh: true);

            return new ConnectorClient(
                 serviceUrl,
                 account,
                 handlers: new BotDelegatingHandler(jwtToken));
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