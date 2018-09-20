namespace Fanex.Bot.Skynex.MessageHandlers.MessageSenders
{
    using System.Threading.Tasks;
    using Fanex.Bot.Models;
    using Fanex.Bot.Skynex.MessageHandlers.MessengerFormatters;
    using Line.Messaging;
    using Microsoft.Bot.Connector;
    using Microsoft.Extensions.Configuration;

    public interface ILineConversation : ISkypeConversation
    {
    }

    public class LineConversation : ILineConversation
    {
        private readonly IConfiguration configuration;
        private readonly ILineFormatter messengerFormatter;

        public LineConversation(IConfiguration configuration, ILineFormatter messengerFormatter)
        {
            this.configuration = configuration;
            this.messengerFormatter = messengerFormatter;
        }

        public async Task ReplyAsync(IMessageActivity activity, string message)
        {
            var lineMessagingClient = CreateLineMessagingClient();

            await lineMessagingClient.PushMessageAsync(
                activity.From.Id,
                new[] { messengerFormatter.Format(message) });
        }

        public async Task SendAsync(MessageInfo messageInfo)
        {
            var lineMessagingClient = CreateLineMessagingClient();

            await lineMessagingClient.PushMessageAsync(
                messageInfo.ConversationId,
                new[] { messengerFormatter.Format(messageInfo.Text) });
        }

        private LineMessagingClient CreateLineMessagingClient()
            => new LineMessagingClient(
                    configuration.GetSection("LINE")?.GetSection("ChannelAccessToken")?.Value);
    }
}