namespace Fanex.Bot.Models
{
    using Microsoft.Bot.Connector.Authentication;

    public class MessageInfo
    {
        public string FromId { get; set; }

        public string FromName { get; set; }

        public string ToId { get; set; }

        public string ToName { get; set; }

        public string ServiceUrl { get; set; }

        public string ChannelId { get; set; }

        public string ConversationId { get; set; }

        public MicrosoftAppCredentials AppCredentials { get; set; }

        public string Text { get; set; }
    }
}