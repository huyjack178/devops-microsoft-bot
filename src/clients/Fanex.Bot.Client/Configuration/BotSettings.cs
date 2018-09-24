namespace Fanex.Bot.Client.Configuration
{
    using System;

    public class BotSettings
    {
        public BotSettings(Uri botServiceUrl, string clientId, string clientPassword, int cacheTimeout = 120)
        {
            BotServiceUrl = botServiceUrl;
            ClientId = clientId;
            ClientPassword = clientPassword;
            CacheTimeout = cacheTimeout;
        }

        public Uri BotServiceUrl { get; }

        public string ClientId { get; }

        public string ClientPassword { get; }

        public int CacheTimeout { get; }
    }
}