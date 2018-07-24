namespace Fanex.Bot.Skynex.Utilities.Bot
{
    using System.Threading.Tasks;
    using Fanex.Bot.Skynex.Models;
    using Line.Messaging;
    using Microsoft.Bot.Connector;
    using Microsoft.Extensions.Configuration;

    public interface ILineConversation : ISkypeConversation
    {
    }

    public class LineConversation : ILineConversation
    {
        private readonly IConfiguration _configuration;

        public LineConversation(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task ReplyAsync(IMessageActivity activity, string message)
        {
            var lineMessagingClient = CreateLineMessagingClient();

            await lineMessagingClient.PushMessageAsync(
                activity.From.Id,
                new[] { FormatMessage(message) });
        }

        public async Task SendAsync(MessageInfo messageInfo)
        {
            var lineMessagingClient = CreateLineMessagingClient();

            await lineMessagingClient.PushMessageAsync(
                messageInfo.ConversationId,
                new[] { FormatMessage(messageInfo.Text) });
        }

        private LineMessagingClient CreateLineMessagingClient()
            => new LineMessagingClient(
                    _configuration.GetSection("LINE")?.GetSection("ChannelAccessToken")?.Value);

        private static string FormatMessage(string message)
        {
            return message
                .Replace("**", string.Empty)
                .Replace("*", string.Empty)
                .Replace("\n\n \n\n", "\n")
                .Replace("\n\n\n\n", "\n")
                .Replace("\n\n\n", "\n")
                .Replace("\n\n", "\n");
        }
    }
}