namespace Fanex.Bot.Client.Configuration
{
    using System;

    public class BotSettings
    {
        public BotSettings(Uri botServiceUrl, string clientId, string clientPassword)
        {
            BotServiceUrl = botServiceUrl;
            ClientId = clientId;
            ClientPassword = clientPassword;
        }

        public Uri BotServiceUrl { get; }

        public string ClientId { get; }

        public string ClientPassword { get; }
    }
}